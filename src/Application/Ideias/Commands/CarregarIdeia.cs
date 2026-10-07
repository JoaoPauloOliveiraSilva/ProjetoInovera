namespace Innovera.Application.Ideias.Commands;

internal static class CarregarIdeia
{
    public static async Task<Ideia> ComTudoAsync(IApplicationDbContext context, int id, CancellationToken cancellationToken)
    {
        var ideia = await context.Ideias
            .Include(i => i.Coautores)
            .Include(i => i.Likes)
            .Include(i => i.Comentarios)
            .Include(i => i.Avaliacao)
            .Include(i => i.IniciativaResultante)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

        Guard.Against.NotFound(id, ideia);
        return ideia;
    }
}
