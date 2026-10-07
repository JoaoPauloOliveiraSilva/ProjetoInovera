namespace Innovera.Application.Ideias.Commands;

/// <summary>Um coautor confirma que é autor da ideia (passo "01. validação dos autores").</summary>
[Authorize]
public record ConfirmarAutoriaCommand(int IdeiaId) : IRequest;

public class ConfirmarAutoriaCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<ConfirmarAutoriaCommand>
{
    public async Task Handle(ConfirmarAutoriaCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.ConfirmarAutoria(eu.Id, relogio.GetUtcNow());
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>A equipa de Inovação valida a ideia e abre a discussão pública (30 dias).</summary>
[Authorize(Roles = Roles.Administrador)]
public record ValidarIdeiaCommand(int IdeiaId) : IRequest;

public class ValidarIdeiaCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<ValidarIdeiaCommand>
{
    public async Task Handle(ValidarIdeiaCommand request, CancellationToken cancellationToken)
    {
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.IniciarDiscussao(relogio.GetUtcNow());
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Fecha já a discussão (normalmente é automático aos 30 dias).</summary>
[Authorize(Roles = Roles.Administrador)]
public record FecharDiscussaoCommand(int IdeiaId) : IRequest;

public class FecharDiscussaoCommandHandler(IApplicationDbContext context) : IRequestHandler<FecharDiscussaoCommand>
{
    public async Task Handle(FecharDiscussaoCommand request, CancellationToken cancellationToken)
    {
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.FecharDiscussao();
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Dá ou retira o like (devolve se o utilizador passou a gostar).</summary>
[Authorize]
public record AlternarGostoIdeiaCommand(int IdeiaId) : IRequest<bool>;

public class AlternarGostoIdeiaCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<AlternarGostoIdeiaCommand, bool>
{
    public async Task<bool> Handle(AlternarGostoIdeiaCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        var gostava = ideia.Likes.Any(l => l.UtilizadorId == eu.Id);
        if (gostava)
        {
            ideia.RemoverLike(eu.Id);
        }
        else
        {
            ideia.AdicionarLike(eu.Id, relogio.GetUtcNow());
        }

        await context.SaveChangesAsync(cancellationToken);
        return !gostava;
    }
}

[Authorize]
public record ComentarIdeiaCommand(int IdeiaId, string Texto) : IRequest;

public class ComentarIdeiaCommandValidator : AbstractValidator<ComentarIdeiaCommand>
{
    public ComentarIdeiaCommandValidator()
    {
        RuleFor(c => c.Texto).NotEmpty().MaximumLength(2000);
    }
}

public class ComentarIdeiaCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<ComentarIdeiaCommand>
{
    public async Task Handle(ComentarIdeiaCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        ideia.AdicionarComentario(eu.Id, request.Texto, relogio.GetUtcNow());
        await context.SaveChangesAsync(cancellationToken);
    }
}

[Authorize]
public record AlternarGostoComentarioCommand(int ComentarioId) : IRequest<bool>;

public class AlternarGostoComentarioCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<AlternarGostoComentarioCommand, bool>
{
    public async Task<bool> Handle(AlternarGostoComentarioCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var comentario = await context.ComentariosIdeia
            .Include(c => c.Gostos)
            .FirstOrDefaultAsync(c => c.Id == request.ComentarioId, cancellationToken);
        Guard.Against.NotFound(request.ComentarioId, comentario);

        var gostava = comentario.Gostos.Any(g => g.UtilizadorId == eu.Id);
        if (gostava)
        {
            comentario.RemoverGosto(eu.Id);
        }
        else
        {
            comentario.AdicionarGosto(eu.Id, relogio.GetUtcNow());
        }

        await context.SaveChangesAsync(cancellationToken);
        return !gostava;
    }
}
