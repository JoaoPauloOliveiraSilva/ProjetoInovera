namespace Innovera.Application.Ideias.Queries;

public record FicheiroDto(Stream Conteudo, string NomeFicheiro, string TipoConteudo);

[Authorize]
public record ObterAnexoQuery(int AnexoId) : IRequest<FicheiroDto>;

public class ObterAnexoQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores, IArmazenamentoFicheiros armazenamento)
    : IRequestHandler<ObterAnexoQuery, FicheiroDto>
{
    public async Task<FicheiroDto> Handle(ObterAnexoQuery request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var anexo = await context.AnexosIdeia
            .AsNoTracking()
            .Include(a => a.Ideia).ThenInclude(i => i.Coautores)
            .FirstOrDefaultAsync(a => a.Id == request.AnexoId, cancellationToken);
        Guard.Against.NotFound(request.AnexoId, anexo);

        var ideia = anexo.Ideia;
        if (!RegrasVisibilidade.PodeVer(utilizadores.EAdministrador, eu.Id, eu.IdentityId, ideia.AutorId, ideia.CreatedBy,
                ideia.Coautores.Select(c => c.UtilizadorId), ideia.Privada, ideia.Estado))
        {
            throw new Common.Exceptions.ForbiddenAccessException();
        }

        var conteudo = await armazenamento.AbrirAsync(anexo.ChaveArmazenamento, cancellationToken);
        Guard.Against.NotFound(anexo.ChaveArmazenamento, conteudo);

        return new FicheiroDto(conteudo, anexo.NomeFicheiro, anexo.TipoConteudo);
    }
}
