using Innovera.Domain.ValueObjects;

namespace Innovera.Application.Ideias.Commands;

/// <summary>Regista as 5 notas de uma ideia da dstelecom; a nota e a decisão são calculadas pelo domínio.</summary>
[Authorize(Roles = Roles.Administrador)]
public record RegistarAvaliacaoCommand : IRequest<IdeiaResumoNotaDto>
{
    public int IdeiaId { get; init; }
    public int Custo { get; init; }
    public int Enquadramento { get; init; }
    public int Beneficio { get; init; }
    public int AdequacaoTecnica { get; init; }
    public int Incerteza { get; init; }
    public DateOnly? DataReuniaoCE { get; init; }
    public string? Observacoes { get; init; }
}

public record IdeiaResumoNotaDto(decimal? Nota, EstadoIdeia Estado, ClassificacaoIdeia? Classificacao);

public class RegistarAvaliacaoCommandValidator : AbstractValidator<RegistarAvaliacaoCommand>
{
    public RegistarAvaliacaoCommandValidator()
    {
        RuleFor(c => c.Custo).InclusiveBetween(0, 4);
        RuleFor(c => c.Enquadramento).InclusiveBetween(0, 4);
        RuleFor(c => c.Beneficio).InclusiveBetween(0, 4);
        RuleFor(c => c.AdequacaoTecnica).InclusiveBetween(0, 4);
        RuleFor(c => c.Incerteza).InclusiveBetween(0, 4);
        RuleFor(c => c.Observacoes).MaximumLength(2000);
    }
}

public class RegistarAvaliacaoCommandHandler(IApplicationDbContext context) : IRequestHandler<RegistarAvaliacaoCommand, IdeiaResumoNotaDto>
{
    public async Task<IdeiaResumoNotaDto> Handle(RegistarAvaliacaoCommand request, CancellationToken cancellationToken)
    {
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.Responsabilidade = Responsabilidade.Dstelecom;
        ideia.RegistarAvaliacao(new AvaliacaoIdeia
        {
            Custo = (EscalaDeAvaliacao)request.Custo,
            Enquadramento = (EscalaDeAvaliacao)request.Enquadramento,
            Beneficio = (EscalaDeAvaliacao)request.Beneficio,
            AdequacaoTecnica = (EscalaDeAvaliacao)request.AdequacaoTecnica,
            Incerteza = (EscalaDeAvaliacao)request.Incerteza,
            DataReuniaoCE = request.DataReuniaoCE,
            Observacoes = request.Observacoes,
        }, PesosAvaliacao.Mod246);

        await context.SaveChangesAsync(cancellationToken);
        return new IdeiaResumoNotaDto(ideia.Avaliacao?.Nota, ideia.Estado, ideia.Classificacao);
    }
}

/// <summary>Ideias da dst (grupo): só se regista se foram aprovadas.</summary>
[Authorize(Roles = Roles.Administrador)]
public record RegistarDecisaoDstCommand(int IdeiaId, bool Aprovada) : IRequest;

public class RegistarDecisaoDstCommandHandler(IApplicationDbContext context) : IRequestHandler<RegistarDecisaoDstCommand>
{
    public async Task Handle(RegistarDecisaoDstCommand request, CancellationToken cancellationToken)
    {
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.Responsabilidade = Responsabilidade.Dst;
        ideia.RegistarDecisaoSemNotas(request.Aprovada);
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>
/// Define a classe. Projeto, Desafio e Melhoria Contínua criam a iniciativa ligada à ideia (devolve o id dela).
/// </summary>
[Authorize(Roles = Roles.Administrador)]
public record DefinirClasseCommand(int IdeiaId, ClasseIdeia Classe, string? Status) : IRequest<int?>;

public class DefinirClasseCommandHandler(IApplicationDbContext context) : IRequestHandler<DefinirClasseCommand, int?>
{
    public async Task<int?> Handle(DefinirClasseCommand request, CancellationToken cancellationToken)
    {
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.DefinirClasse(request.Classe);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            ideia.Status = request.Status.Trim();
        }

        Iniciativa? iniciativa = ideia.IniciativaResultante;
        if (iniciativa is null)
        {
            iniciativa = request.Classe switch
            {
                ClasseIdeia.Projeto => new Projeto(),
                ClasseIdeia.Desafio => new Desafio(),
                ClasseIdeia.MelhoriaContinua => new MelhoriaContinua(),
                _ => null,
            };

            if (iniciativa is not null)
            {
                iniciativa.Codigo = ideia.Codigo;
                iniciativa.Titulo = ideia.Titulo;
                iniciativa.Descricao = ideia.Descricao;
                iniciativa.ResponsavelId = ideia.AutorId;
                iniciativa.Origem = $"Ideia {ideia.Codigo}";
                iniciativa.IdeiaOrigemId = ideia.Id;
                context.Iniciativas.Add(iniciativa);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return iniciativa?.Id;
    }
}
