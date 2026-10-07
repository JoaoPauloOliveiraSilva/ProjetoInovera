namespace Innovera.Application.Ideias.Queries;

[Authorize]
public record ObterIdeiaQuery(int Id) : IRequest<IdeiaDetalheDto>;

public class ObterIdeiaQueryHandler : IRequestHandler<ObterIdeiaQuery, IdeiaDetalheDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUtilizadores _utilizadores;

    public ObterIdeiaQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    {
        _context = context;
        _utilizadores = utilizadores;
    }

    public async Task<IdeiaDetalheDto> Handle(ObterIdeiaQuery request, CancellationToken cancellationToken)
    {
        var eu = await _utilizadores.AtualAsync(cancellationToken);
        var admin = _utilizadores.EAdministrador;

        var ideia = await _context.Ideias
            .AsNoTracking()
            .Include(i => i.Autor)
            .Include(i => i.Coautores).ThenInclude(c => c.Utilizador)
            .Include(i => i.Anexos)
            .Include(i => i.Comentarios).ThenInclude(c => c.Autor)
            .Include(i => i.Comentarios).ThenInclude(c => c.Gostos)
            .Include(i => i.Likes)
            .Include(i => i.Avaliacao)
            .Include(i => i.IniciativaResultante)
            .FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);

        Guard.Against.NotFound(request.Id, ideia);

        var eAutor = ideia.AutorId == eu.Id || ideia.CreatedBy == eu.IdentityId || ideia.Coautores.Any(c => c.UtilizadorId == eu.Id);
        if (!RegrasVisibilidade.PodeVer(admin, eu.Id, eu.IdentityId, ideia.AutorId, ideia.CreatedBy,
                ideia.Coautores.Select(c => c.UtilizadorId), ideia.Privada, ideia.Estado))
        {
            throw new Common.Exceptions.ForbiddenAccessException();
        }

        var avaliacao = ideia.Avaliacao is null
            ? null
            : new AvaliacaoDto((int?)ideia.Avaliacao.Custo, (int?)ideia.Avaliacao.Enquadramento, (int?)ideia.Avaliacao.Beneficio,
                (int?)ideia.Avaliacao.AdequacaoTecnica, (int?)ideia.Avaliacao.Incerteza, ideia.Avaliacao.Nota, ideia.Avaliacao.Aprovada,
                ideia.Avaliacao.Observacoes);

        var nomeAutor = ideia.Anonima ? null : ideia.Autor?.Nome;

        return new IdeiaDetalheDto(
            ideia.Id, ideia.Codigo, ideia.Titulo, ideia.Tipo, ideia.Descricao, ideia.Vantagens, ideia.Requisitos,
            ideia.ComoEFeitoAtualmente, ideia.ModeloNegocio, ideia.Competidores, ideia.Custos, ideia.Privada, ideia.Anonima,
            nomeAutor,
            RegrasVisibilidade.Autores(ideia.Anonima, nomeAutor, ideia.Coautores.Select(c => c.Utilizador.Nome)),
            ideia.Coautores.Select(c => new CoautorDto(c.UtilizadorId, c.Utilizador.Nome, c.Confirmado)).ToList(),
            ideia.DataSubmissao, ideia.Estado, ideia.FimDiscussao, ideia.Responsabilidade, ideia.Classificacao, ideia.Classe, ideia.Status,
            ideia.Likes.Count,
            ideia.Likes.Any(l => l.UtilizadorId == eu.Id),
            ideia.Anexos.OrderBy(a => a.Id).Select(a => new AnexoDto(a.Id, a.NomeFicheiro, a.TamanhoBytes)).ToList(),
            ideia.Comentarios.OrderBy(c => c.Data)
                .Select(c => new ComentarioDto(c.Id, c.Autor.Nome, c.Autor.Empresa, c.Texto, c.Data, c.Gostos.Count,
                    c.Gostos.Any(g => g.UtilizadorId == eu.Id)))
                .ToList(),
            avaliacao,
            ideia.Estado == EstadoIdeia.ValidacaoAutores && ideia.Coautores.Any(c => c.UtilizadorId == eu.Id && !c.Confirmado),
            admin || eAutor,
            ideia.IniciativaResultante?.Id);
    }
}
