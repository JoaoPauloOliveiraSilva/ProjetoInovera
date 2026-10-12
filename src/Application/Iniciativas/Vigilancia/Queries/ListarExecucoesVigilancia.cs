namespace Innovera.Application.Iniciativas.Vigilancias;

// DTO devolvido ao frontend para cada execução.
public record ExecucaoVigilanciaDto(
    int Id,
    int VigilanciaId,
    DateOnly DataPrevista,
    DateOnly? DataRealizada,
    string? Resultado,
    string? LinkResultado,
    bool NoPrazo
);

[Authorize]
public record ListarExecucoesVigilanciaQuery(
    int VigilanciaId,
    DateOnly? DataPrevista = null,
    DateOnly? DataRealizada = null,
    string? Resultado = null,
    string? LinkResultado = null
) : IRequest<IReadOnlyList<ExecucaoVigilanciaDto>>;

public class ListarExecucoesVigilanciaQueryValidator
    : AbstractValidator<ListarExecucoesVigilanciaQuery>
{
    public ListarExecucoesVigilanciaQueryValidator()
    {
        RuleFor(x => x.VigilanciaId)
            .GreaterThan(0);

        RuleFor(x => x.Resultado)
            .MaximumLength(4000);

        RuleFor(x => x.LinkResultado)
            .MaximumLength(2000)
            .Must(ValidacaoLinks.EValido)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkResultado));
    }
}

public class ListarExecucoesVigilanciaQueryHandler(
    IApplicationDbContext context
) : IRequestHandler<
    ListarExecucoesVigilanciaQuery,
    IReadOnlyList<ExecucaoVigilanciaDto>>
{
    public async Task<IReadOnlyList<ExecucaoVigilanciaDto>> Handle(
        ListarExecucoesVigilanciaQuery request,
        CancellationToken cancellationToken)
    {
        var vigilancia = await context.Iniciativas
            .OfType<Vigilancia>()
            .AsNoTracking()
            .FirstOrDefaultAsync(
                v => v.Id == request.VigilanciaId,
                cancellationToken);

        Guard.Against.NotFound(request.VigilanciaId, vigilancia);

        var consulta = context.Iniciativas
            .OfType<Vigilancia>()
            .Where(v => v.Id == request.VigilanciaId)
            .SelectMany(v => v.Execucoes)
            .AsNoTracking();

        if (request.DataPrevista.HasValue)
        {
            consulta = consulta.Where(
                e => e.DataPrevista == request.DataPrevista.Value);
        }

        if (request.DataRealizada.HasValue)
        {
            consulta = consulta.Where(
                e => e.DataRealizada == request.DataRealizada.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Resultado))
        {
            var resultado = request.Resultado.Trim();

            consulta = consulta.Where(
                e => e.Resultado != null &&
                     e.Resultado.Contains(resultado));
        }

        if (!string.IsNullOrWhiteSpace(request.LinkResultado))
        {
            var link = request.LinkResultado.Trim();

            consulta = consulta.Where(
                e => e.LinkResultado == link);
        }

        return await consulta
            .OrderBy(e => e.DataPrevista)
            .Select(e => new ExecucaoVigilanciaDto(
                e.Id,
                e.VigilanciaId,
                e.DataPrevista,
                e.DataRealizada,
                e.Resultado,
                e.LinkResultado,
                e.DataRealizada.HasValue &&
                    e.DataRealizada.Value <= e.DataPrevista
            ))
            .ToListAsync(cancellationToken);
    }
}