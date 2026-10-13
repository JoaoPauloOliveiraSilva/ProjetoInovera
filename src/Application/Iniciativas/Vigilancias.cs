using Innovera.Application.Charters;
using Innovera.Application.Ideias.Queries;

namespace Innovera.Application.Iniciativas;

// Execuções das vigilâncias (Mod.246 › Vigilâncias): cada uma tem data prevista e realizada (KPI 24) e o
// resultado entra automaticamente no repositório de conhecimento codificado (Mod.237, RF10).

public record ExecucaoDto(int Id, DateOnly DataPrevista, DateOnly? DataRealizada, string? Resultado, string? LinkResultado, bool NoPrazo,
    bool Atrasada, int? ConhecimentoId, string? NomeFicheiro = null, long? TamanhoBytes = null);

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

    public const long TamanhoMaximoBytes = 25 * 1024 * 1024;

    public static string LinkFicheiro(int vigilanciaId, int execucaoId) => $"/api/Iniciativas/{vigilanciaId}/execucoes/{execucaoId}/ficheiro";

    public static ExecucaoDto ParaDto(ExecucaoVigilancia e, DateOnly hoje, IReadOnlyDictionary<int, int> conhecimentoPorExecucao)
        => new(e.Id, e.DataPrevista, e.DataRealizada, e.Resultado, e.LinkResultado, e.NoPrazo,
            e.DataRealizada is null && e.DataPrevista < hoje,
            conhecimentoPorExecucao.TryGetValue(e.Id, out var c) ? c : null, e.NomeFicheiro, e.TamanhoBytes);

    /// <summary>Entrada do conhecimento codificado de cada execução (para mostrar "no conhecimento codificado").</summary>
    public static async Task<Dictionary<int, int>> ConhecimentoPorExecucaoAsync(IApplicationDbContext context, IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
        => await context.ConhecimentosCodificados.AsNoTracking()
            .Where(c => c.ExecucaoVigilanciaId != null && ids.Contains(c.ExecucaoVigilanciaId.Value))
            .GroupBy(c => c.ExecucaoVigilanciaId!.Value)
            .Select(g => new { ExecucaoId = g.Key, Id = g.Min(c => c.Id) })
            .ToDictionaryAsync(x => x.ExecucaoId, x => x.Id, cancellationToken);

    public static bool Ativa(Vigilancia v) => v.Estado is not (EstadoIniciativa.Concluida or EstadoIniciativa.Cancelada or EstadoIniciativa.Rejeitada);

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
        var conhecimento = await Vigilancias.ConhecimentoPorExecucaoAsync(context, vigilancia.Execucoes.Select(e => e.Id).ToList(), cancellationToken);
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        return vigilancia.Execucoes
            .OrderByDescending(e => e.DataPrevista)
            .Select(e => Vigilancias.ParaDto(e, hoje, conhecimento))
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

// ---------- Página das vigilâncias: lista, relatórios carregados e retirar/repor ----------

public record VigilanciaPainelDto(int Id, string Codigo, string Titulo, EstadoIniciativa Estado, string? Responsavel, string? Descritivo,
    Periodicidade Periodicidade, DateOnly? DataInicio, DateOnly? ProximaExecucao, bool Ativa, IReadOnlyList<ExecucaoDto> Execucoes);

/// <summary>As vigilâncias com a informação e os relatórios de cada uma (as retiradas só se pedidas).</summary>
[Authorize]
public record ListarVigilanciasQuery(bool IncluirRetiradas = false) : IRequest<IReadOnlyList<VigilanciaPainelDto>>;

public class ListarVigilanciasQueryHandler(IApplicationDbContext context, TimeProvider relogio)
    : IRequestHandler<ListarVigilanciasQuery, IReadOnlyList<VigilanciaPainelDto>>
{
    public async Task<IReadOnlyList<VigilanciaPainelDto>> Handle(ListarVigilanciasQuery request, CancellationToken cancellationToken)
    {
        var vigilancias = await context.Iniciativas.OfType<Vigilancia>().AsNoTracking()
            .Include(v => v.Responsavel)
            .Include(v => v.Execucoes)
            .OrderBy(v => v.Titulo)
            .ToListAsync(cancellationToken);
        var conhecimento = await Vigilancias.ConhecimentoPorExecucaoAsync(context,
            vigilancias.SelectMany(v => v.Execucoes).Select(e => e.Id).ToList(), cancellationToken);
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        return vigilancias
            .Select(v => new VigilanciaPainelDto(v.Id, v.Codigo, v.Titulo, v.Estado, v.Responsavel?.Nome ?? v.ResponsavelNome, v.Descritivo,
                v.Periodicidade, v.DataInicio,
                v.Execucoes.Where(e => e.DataRealizada is null).OrderBy(e => e.DataPrevista).Select(e => (DateOnly?)e.DataPrevista).FirstOrDefault(),
                Vigilancias.Ativa(v),
                v.Execucoes.OrderByDescending(e => e.DataRealizada ?? e.DataPrevista).Select(e => Vigilancias.ParaDto(e, hoje, conhecimento)).ToList()))
            .Where(v => request.IncluirRetiradas || v.Ativa)
            .ToList();
    }
}

/// <summary>
/// Carrega o relatório de uma vigilância (o ficheiro gerado pela tarefa automática): fica na execução por fazer mais antiga
/// (ou numa nova, se não houver), conta como feita nessa data e entra no conhecimento codificado (Mod.237).
/// </summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record CarregarRelatorioVigilanciaCommand(int VigilanciaId, string NomeFicheiro, string TipoConteudo, long TamanhoBytes, Stream Conteudo,
    string? Descricao, DateOnly? Data) : IRequest<int>;

public class CarregarRelatorioVigilanciaCommandValidator : AbstractValidator<CarregarRelatorioVigilanciaCommand>
{
    public CarregarRelatorioVigilanciaCommandValidator()
    {
        RuleFor(c => c.NomeFicheiro).NotEmpty().MaximumLength(260);
        RuleFor(c => c.TamanhoBytes).GreaterThan(0).LessThanOrEqualTo(Vigilancias.TamanhoMaximoBytes)
            .WithMessage("O relatório tem de ter no máximo 25 MB.");
        RuleFor(c => c.Descricao).MaximumLength(4000);
    }
}

public class CarregarRelatorioVigilanciaCommandHandler(IApplicationDbContext context, IArmazenamentoFicheiros armazenamento, TimeProvider relogio)
    : IRequestHandler<CarregarRelatorioVigilanciaCommand, int>
{
    public async Task<int> Handle(CarregarRelatorioVigilanciaCommand request, CancellationToken cancellationToken)
    {
        var vigilancia = await Vigilancias.ObterAsync(context, request.VigilanciaId, cancellationToken);
        var data = request.Data ?? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        var execucao = vigilancia.Execucoes.Where(e => e.DataRealizada is null).OrderBy(e => e.DataPrevista).FirstOrDefault();
        if (execucao is null)
        {
            execucao = new ExecucaoVigilancia { Vigilancia = vigilancia, DataPrevista = data };
            vigilancia.Execucoes.Add(execucao);
        }

        var nome = Path.GetFileName(request.NomeFicheiro);
        execucao.ChaveArmazenamento = await armazenamento.GuardarAsync(request.Conteudo, $"vigilancias/{vigilancia.Codigo}", nome, cancellationToken);
        execucao.NomeFicheiro = nome;
        execucao.TipoConteudo = string.IsNullOrWhiteSpace(request.TipoConteudo) ? "application/octet-stream" : request.TipoConteudo;
        execucao.TamanhoBytes = request.TamanhoBytes;
        await context.SaveChangesAsync(cancellationToken);

        // Já com o número da execução, para o link do ficheiro.
        var resultado = string.IsNullOrWhiteSpace(request.Descricao) ? $"Relatório {nome}" : request.Descricao.Trim();
        execucao.RegistarResultado(resultado, data, Vigilancias.LinkFicheiro(vigilancia.Id, execucao.Id));
        if (Vigilancias.Seguinte(execucao.DataPrevista, vigilancia.Periodicidade) is { } seguinte
            && !vigilancia.Execucoes.Any(e => e != execucao && e.DataRealizada is null))
        {
            vigilancia.Execucoes.Add(new ExecucaoVigilancia { Vigilancia = vigilancia, DataPrevista = seguinte });
        }

        await context.SaveChangesAsync(cancellationToken);
        return execucao.Id;
    }
}

/// <summary>O relatório carregado numa execução (para descarregar).</summary>
[Authorize]
public record ObterRelatorioVigilanciaQuery(int VigilanciaId, int ExecucaoId) : IRequest<FicheiroDto>;

public class ObterRelatorioVigilanciaQueryHandler(IApplicationDbContext context, IArmazenamentoFicheiros armazenamento)
    : IRequestHandler<ObterRelatorioVigilanciaQuery, FicheiroDto>
{
    public async Task<FicheiroDto> Handle(ObterRelatorioVigilanciaQuery request, CancellationToken cancellationToken)
    {
        var ficheiro = await context.Iniciativas.OfType<Vigilancia>().AsNoTracking()
            .Where(v => v.Id == request.VigilanciaId)
            .SelectMany(v => v.Execucoes)
            .Where(e => e.Id == request.ExecucaoId && e.ChaveArmazenamento != null)
            .Select(e => new { e.ChaveArmazenamento, e.NomeFicheiro, e.TipoConteudo })
            .FirstOrDefaultAsync(cancellationToken);
        Guard.Against.NotFound(request.ExecucaoId, ficheiro);

        var conteudo = await armazenamento.AbrirAsync(ficheiro.ChaveArmazenamento!, cancellationToken);
        Guard.Against.NotFound(ficheiro.ChaveArmazenamento!, conteudo);
        return new FicheiroDto(conteudo, ficheiro.NomeFicheiro ?? "relatorio", ficheiro.TipoConteudo ?? "application/octet-stream");
    }
}

/// <summary>Retira uma vigilância da página (fica terminada, com o histórico) ou volta a pô-la ativa.</summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record AlterarVigilanciaAtivaCommand(int VigilanciaId, bool Ativa) : IRequest;

public class AlterarVigilanciaAtivaCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<AlterarVigilanciaAtivaCommand>
{
    public async Task Handle(AlterarVigilanciaAtivaCommand request, CancellationToken cancellationToken)
    {
        var vigilancia = await Vigilancias.ObterAsync(context, request.VigilanciaId, cancellationToken);
        vigilancia.Estado = request.Ativa ? EstadoIniciativa.EmCurso : EstadoIniciativa.Concluida;
        if (!request.Ativa)
        {
            vigilancia.DataFim ??= DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}

