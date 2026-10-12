
namespace Innovera.Application.Iniciativas.Vigilancias;

// no momento cria um execucao de cada vez, implementar geracao automatica depois.
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record CriarExecucaoVigilanciaCommand(
    int VigilanciaId,
    DateOnly DataPrevista
) : IRequest<int>;

public class CriarExecucaoVigilanciaCommandValidator
    : AbstractValidator<CriarExecucaoVigilanciaCommand>
{
    public CriarExecucaoVigilanciaCommandValidator()
    {
        RuleFor(x => x.VigilanciaId)
            .GreaterThan(0);

        RuleFor(x => x.DataPrevista)
            .Must(data => data != default)
            .WithMessage("A data prevista é obrigatória.");
    }
}

public class CriarExecucaoVigilanciaCommandHandler(
    IApplicationDbContext context
) : IRequestHandler<CriarExecucaoVigilanciaCommand, int>
{
    public async Task<int> Handle(
        CriarExecucaoVigilanciaCommand request,
        CancellationToken cancellationToken)
    {
        var vigilancia = await context.Iniciativas
            .OfType<Vigilancia>()
            .Include(v => v.Execucoes)
            .FirstOrDefaultAsync(
                v => v.Id == request.VigilanciaId,
                cancellationToken);

        Guard.Against.NotFound(request.VigilanciaId, vigilancia);

        var execucao = new ExecucaoVigilancia
        {
            VigilanciaId = vigilancia.Id,
            Vigilancia = vigilancia,
            DataPrevista = request.DataPrevista
        };

        vigilancia.Execucoes.Add(execucao);

        await context.SaveChangesAsync(cancellationToken);

        return execucao.Id;
    }
}