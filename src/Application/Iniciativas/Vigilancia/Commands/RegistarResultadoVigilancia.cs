
namespace Innovera.Application.Iniciativas.Vigilancias;

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record RegistarResultadoVigilanciaCommand(
    int VigilanciaId,
    int ExecucaoId,
    string Resultado,
    DateOnly DataRealizada,
    string? LinkResultado
) : IRequest;

public class RegistarResultadoVigilanciaCommandValidator
    : AbstractValidator<RegistarResultadoVigilanciaCommand>
{
    public RegistarResultadoVigilanciaCommandValidator()
    {
        RuleFor(x => x.VigilanciaId)
            .GreaterThan(0);

        RuleFor(x => x.ExecucaoId)
            .GreaterThan(0);

        RuleFor(x => x.Resultado)
            .NotEmpty()
            .MaximumLength(4000);

        RuleFor(x => x.LinkResultado)
            .MaximumLength(2000)
            .Must(ValidacaoLinks.EValido)
            .When(x => !string.IsNullOrWhiteSpace(x.LinkResultado));
    }
}

public class RegistarResultadoVigilanciaCommandHandler(
    IApplicationDbContext context
) : IRequestHandler<RegistarResultadoVigilanciaCommand>
{
    public async Task Handle(
        RegistarResultadoVigilanciaCommand request,
        CancellationToken cancellationToken)
    {
        var vigilancia = await context.Iniciativas
            .OfType<Vigilancia>()
            .Include(v => v.Execucoes)
            .FirstOrDefaultAsync(
                v => v.Id == request.VigilanciaId,
                cancellationToken);

        Guard.Against.NotFound(request.VigilanciaId, vigilancia);

        var execucao = vigilancia.Execucoes
            .FirstOrDefault(e => e.Id == request.ExecucaoId);

        Guard.Against.NotFound(request.ExecucaoId, execucao);

        execucao.RegistarResultado(
            request.Resultado,
            request.DataRealizada,
            request.LinkResultado);

        await context.SaveChangesAsync(cancellationToken);
    }
}
// Ter de alterar caso nao seja permitido substituir valores ja preenchifos anteriormente.