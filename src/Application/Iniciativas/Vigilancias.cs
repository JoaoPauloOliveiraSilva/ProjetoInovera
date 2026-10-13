using Innovera.Application.Charters;

namespace Innovera.Application.Iniciativas;

// Execuções das vigilâncias (Mod.246 › Vigilâncias): cada uma tem data prevista e realizada (KPI 24) e o
// resultado entra automaticamente no repositório de conhecimento codificado (Mod.237, RF10).

public record ExecucaoDto(int Id, DateOnly DataPrevista, DateOnly? DataRealizada, string? Resultado, string? LinkResultado, bool NoPrazo,
    bool Atrasada, int? ConhecimentoId);

public record ExecucaoDados(DateOnly DataPrevista, DateOnly? DataRealizada, string? Resultado, string? LinkResultado);

internal static class Vigilancias
{
    public static async Task<Vigilancia> ObterAsync(IApplicationDbContext context, int id, CancellationToken cancellationToken)
    {
        var iniciativa = await context.Iniciativas.Include(i => ((Vigilancia)i).Execucoes)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        Guard.Against.NotFound(id, iniciativa);
        return iniciativa as Vigilancia ?? throw new RegraDeNegocioException("Só as vigilâncias têm execuções.");
    }

    /// <summary>Data da execução seguinte, conforme a periodicidade (null se não se repete).</summary>
    public static DateOnly? Seguinte(DateOnly data, Periodicidade periodicidade) => periodicidade switch
    {
        Periodicidade.Semanal => data.AddDays(7),
        Periodicidade.Mensal => data.AddMonths(1),
        Periodicidade.Trimestral => data.AddMonths(3),
        Periodicidade.Semestral => data.AddMonths(6),
        Periodicidade.Anual => data.AddYears(1),
        _ => null,
    };
}

[Authorize]
public record ListarExecucoesQuery(int VigilanciaId) : IRequest<IReadOnlyList<ExecucaoDto>>;

public class ListarExecucoesQueryHandler(IApplicationDbContext context, TimeProvider relogio)
    : IRequestHandler<ListarExecucoesQuery, IReadOnlyList<ExecucaoDto>>
{
    public async Task<IReadOnlyList<ExecucaoDto>> Handle(ListarExecucoesQuery request, CancellationToken cancellationToken)
    {
        var vigilancia = await Vigilancias.ObterAsync(context, request.VigilanciaId, cancellationToken);
        var ids = vigilancia.Execucoes.Select(e => e.Id).ToList();
        var conhecimento = await context.ConhecimentosCodificados.AsNoTracking()
            .Where(c => c.ExecucaoVigilanciaId != null && ids.Contains(c.ExecucaoVigilanciaId.Value))
            .Select(c => new { c.Id, ExecucaoId = c.ExecucaoVigilanciaId!.Value })
            .ToListAsync(cancellationToken);
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        return vigilancia.Execucoes
            .OrderByDescending(e => e.DataPrevista)
            .Select(e => new ExecucaoDto(e.Id, e.DataPrevista, e.DataRealizada, e.Resultado, e.LinkResultado, e.NoPrazo,
                e.DataRealizada is null && e.DataPrevista < hoje,
                conhecimento.FirstOrDefault(c => c.ExecucaoId == e.Id)?.Id))
            .ToList();
    }
}

/// <summary>
/// Planeia uma execução ou regista o resultado. Com resultado, a entrada no conhecimento codificado é criada (ou
/// atualizada) e, se a vigilância se repete e não houver outra por fazer, fica planeada a seguinte.
/// </summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarExecucaoCommand(int VigilanciaId, int? Id, ExecucaoDados Dados) : IRequest<int>;

public class GuardarExecucaoCommandValidator : AbstractValidator<GuardarExecucaoCommand>
{
    public GuardarExecucaoCommandValidator()
    {
        RuleFor(c => c.Dados.Resultado).MaximumLength(4000);
        RuleFor(c => c.Dados.Resultado).NotEmpty().When(c => c.Dados.DataRealizada is not null)
            .WithMessage("Escreva o resultado da vigilância (ou apague a data de realização).");
        RuleFor(c => c.Dados.DataRealizada).NotNull().When(c => !string.IsNullOrWhiteSpace(c.Dados.Resultado))
            .WithMessage("Indique a data em que a vigilância foi feita.");
        RuleFor(c => c.Dados.LinkResultado).MaximumLength(2000).Must(ValidacaoLinks.EValido)
            .When(c => !string.IsNullOrWhiteSpace(c.Dados.LinkResultado)).WithMessage("O link tem de ser um endereço https://… (ex.: OneDrive).");
    }
}

public class GuardarExecucaoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarExecucaoCommand, int>
{
    public async Task<int> Handle(GuardarExecucaoCommand request, CancellationToken cancellationToken)
    {
        var vigilancia = await Vigilancias.ObterAsync(context, request.VigilanciaId, cancellationToken);
        ExecucaoVigilancia e;
        if (request.Id is null)
        {
            e = new ExecucaoVigilancia { Vigilancia = vigilancia };
            vigilancia.Execucoes.Add(e);
        }
        else
        {
            var existente = vigilancia.Execucoes.FirstOrDefault(x => x.Id == request.Id.Value);
            Guard.Against.NotFound(request.Id.Value, existente);
            e = existente;
        }

        var d = request.Dados;
        e.DataPrevista = d.DataPrevista;
        var link = string.IsNullOrWhiteSpace(d.LinkResultado) ? null : d.LinkResultado.Trim();
        if (!string.IsNullOrWhiteSpace(d.Resultado) && d.DataRealizada is { } realizada)
        {
            var novo = e.Resultado != d.Resultado.Trim() || e.DataRealizada != realizada || e.LinkResultado != link;
            if (novo)
            {
                var primeiraVez = e.DataRealizada is null;
                e.RegistarResultado(d.Resultado, realizada, link);
                if (primeiraVez && Vigilancias.Seguinte(e.DataPrevista, vigilancia.Periodicidade) is { } seguinte
                    && !vigilancia.Execucoes.Any(x => x != e && x.DataRealizada is null))
                {
                    vigilancia.Execucoes.Add(new ExecucaoVigilancia { Vigilancia = vigilancia, DataPrevista = seguinte });
                }
            }
        }
        else
        {
            e.Resultado = null;
            e.DataRealizada = null;
            e.LinkResultado = link;
        }

        await context.SaveChangesAsync(cancellationToken);
        return e.Id;
    }
}

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record ApagarExecucaoCommand(int VigilanciaId, int Id) : IRequest;

public class ApagarExecucaoCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarExecucaoCommand>
{
    public async Task Handle(ApagarExecucaoCommand request, CancellationToken cancellationToken)
    {
        var vigilancia = await Vigilancias.ObterAsync(context, request.VigilanciaId, cancellationToken);
        var e = vigilancia.Execucoes.FirstOrDefault(x => x.Id == request.Id);
        Guard.Against.NotFound(request.Id, e);
        if (e.DataRealizada is not null)
        {
            throw new RegraDeNegocioException("Esta execução já tem resultado no repositório de conhecimento: não se pode apagar.");
        }

        vigilancia.Execucoes.Remove(e);
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Resultado de vigilância → conhecimento codificado (Mod.237). Corre dentro do SaveChanges.</summary>
public class ResultadoVigilanciaParaConhecimentoCodificado(IApplicationDbContext context) : INotificationHandler<ResultadoVigilanciaRegistadoEvent>
{
    public async Task Handle(ResultadoVigilanciaRegistadoEvent notification, CancellationToken cancellationToken)
    {
        var execucao = notification.Execucao;
        var vigilancia = execucao.Vigilancia;

        var existente = execucao.Id == 0
            ? null
            : await context.ConhecimentosCodificados.FirstOrDefaultAsync(c => c.ExecucaoVigilanciaId == execucao.Id, cancellationToken);
        var registo = existente ?? new ConhecimentoCodificado
        {
            Origem = OrigemConhecimento.Vigilancia,
            ExecucaoVigilancia = execucao,
            Iniciativa = vigilancia,
            ModoConsulta = RelatorioParaConhecimentoCodificado.ModoConsultaOneDrive,
            Resultado = "Síntese e partilha do conhecimento gerado",
        };

        var realizada = execucao.DataRealizada ?? execucao.DataPrevista;
        var documento = $"Vigilância {vigilancia.Titulo} — {realizada:MM/yyyy}";
        registo.Documento = documento.Length > 300 ? documento[..300] : documento;
        registo.Descricao = execucao.Resultado;
        registo.Url = execucao.LinkResultado;
        registo.Data = realizada;
        registo.ArquivadoEm = realizada;

        if (existente is null)
        {
            context.ConhecimentosCodificados.Add(registo);
        }
    }
}
