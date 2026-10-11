namespace Innovera.Application.Iniciativas;

/// <summary>Acrescenta um link para um documento no OneDrive (RF15). A plataforma não guarda o ficheiro.</summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record AdicionarLinkCommand(int IniciativaId, string Titulo, string Url) : IRequest<int>;

public class AdicionarLinkCommandValidator : AbstractValidator<AdicionarLinkCommand>
{
    public AdicionarLinkCommandValidator()
    {
        RuleFor(c => c.Titulo).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Url).NotEmpty().MaximumLength(2000).Must(ValidacaoLinks.EValido)
            .WithMessage("O link tem de ser um endereço https://… (ex.: OneDrive).");
    }
}

public class AdicionarLinkCommandHandler(IApplicationDbContext context) : IRequestHandler<AdicionarLinkCommand, int>
{
    public async Task<int> Handle(AdicionarLinkCommand request, CancellationToken cancellationToken)
    {
        var existe = await context.Iniciativas.AnyAsync(i => i.Id == request.IniciativaId, cancellationToken);
        if (!existe)
        {
            throw new NotFoundException(request.IniciativaId.ToString(System.Globalization.CultureInfo.InvariantCulture), nameof(Iniciativa));
        }

        var link = new LinkDocumento { IniciativaId = request.IniciativaId, Titulo = request.Titulo.Trim(), Url = request.Url.Trim() };
        context.LinksDocumento.Add(link);
        await context.SaveChangesAsync(cancellationToken);
        return link.Id;
    }
}

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record RemoverLinkCommand(int IniciativaId, int LinkId) : IRequest;

public class RemoverLinkCommandHandler(IApplicationDbContext context) : IRequestHandler<RemoverLinkCommand>
{
    public async Task Handle(RemoverLinkCommand request, CancellationToken cancellationToken)
    {
        var link = await context.LinksDocumento
            .FirstOrDefaultAsync(l => l.Id == request.LinkId && l.IniciativaId == request.IniciativaId, cancellationToken);
        Guard.Against.NotFound(request.LinkId, link);
        context.LinksDocumento.Remove(link);
        await context.SaveChangesAsync(cancellationToken);
    }
}
