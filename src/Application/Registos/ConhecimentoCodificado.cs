namespace Innovera.Application.Registos;

// Conhecimento codificado (Mod.237): além das entradas automáticas (relatórios de entregas e resultados de
// vigilâncias), a equipa regista à mão outros documentos (estudos, relatórios SIFIDE…) e, em todas, as
// atividades de partilha (com a CE, os trabalhadores, o CTO…), quem e quando.

public record ConhecimentoCodificadoDados(DateOnly? Data, string Documento, string? Url, string? ModoConsulta, string? Descricao,
    string? Resultado, DateOnly? ArquivadoEm, int? IniciativaId, IReadOnlyList<AtividadeGestaoDto>? Atividades = null);

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarConhecimentoCodificadoCommand(int? Id, ConhecimentoCodificadoDados Dados) : IRequest<int>;

public class GuardarConhecimentoCodificadoCommandValidator : AbstractValidator<GuardarConhecimentoCodificadoCommand>
{
    public GuardarConhecimentoCodificadoCommandValidator()
    {
        RuleFor(c => c.Dados.Documento).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Dados.ModoConsulta).MaximumLength(300);
        RuleFor(c => c.Dados.Descricao).MaximumLength(4000);
        RuleFor(c => c.Dados.Resultado).MaximumLength(1000);
        RuleFor(c => c.Dados.Url).MaximumLength(2000).Must(ValidacaoLinks.EValido)
            .When(c => !string.IsNullOrWhiteSpace(c.Dados.Url)).WithMessage("O link tem de ser um endereço https://… (ex.: OneDrive).");
        RuleForEach(c => c.Dados.Atividades).SetValidator(new AtividadeGestaoValidator());
    }
}

public class GuardarConhecimentoCodificadoCommandHandler(IApplicationDbContext context, TimeProvider relogio)
    : IRequestHandler<GuardarConhecimentoCodificadoCommand, int>
{
    public async Task<int> Handle(GuardarConhecimentoCodificadoCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dados;
        if (d.IniciativaId is { } iniciativaId && !await context.Iniciativas.AnyAsync(i => i.Id == iniciativaId, cancellationToken))
        {
            throw new RegraDeNegocioException("A iniciativa escolhida já não existe.");
        }

        var c = await Registo.ObterOuNovoAsync(context.ConhecimentosCodificados, request.Id, cancellationToken);
        c.Data = d.Data ?? (c.Data == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : c.Data);
        c.Documento = d.Documento.Trim();
        c.Descricao = Registo.Limpar(d.Descricao);
        c.Resultado = Registo.Limpar(d.Resultado);
        c.ModoConsulta = Registo.Limpar(d.ModoConsulta);
        c.ArquivadoEm = d.ArquivadoEm;
        if (c.Origem == OrigemConhecimento.Manual)
        {
            // O link e a iniciativa das entradas automáticas vêm da entrega ou da vigilância.
            c.Url = Registo.Limpar(d.Url);
            c.IniciativaId = d.IniciativaId;
        }

        Registo.DefinirAtividades(c.Atividades, d.Atividades);
        await context.SaveChangesAsync(cancellationToken);
        return c.Id;
    }
}

/// <summary>Só as entradas manuais se apagam (as outras voltariam a ser criadas pela entrega ou pela vigilância).</summary>
[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record ApagarConhecimentoCodificadoCommand(int Id) : IRequest;

public class ApagarConhecimentoCodificadoCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarConhecimentoCodificadoCommand>
{
    public async Task Handle(ApagarConhecimentoCodificadoCommand request, CancellationToken cancellationToken)
    {
        var c = await context.ConhecimentosCodificados.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, c);
        if (c.Origem != OrigemConhecimento.Manual)
        {
            throw new RegraDeNegocioException("Esta entrada foi criada automaticamente (relatório ou vigilância): não se pode apagar.");
        }

        context.ConhecimentosCodificados.Remove(c);
        await context.SaveChangesAsync(cancellationToken);
    }
}
