namespace Innovera.Application.Ideias.Queries;

/// <summary>Lista de ideias visíveis para o utilizador atual.</summary>
[Authorize]
public record ListarIdeiasQuery : IRequest<IReadOnlyList<IdeiaResumoDto>>
{
    /// <summary>Filtra por estado (ex.: EmDiscussao para "ideias em discussão", EmAvaliacao para a página de avaliação).</summary>
    public EstadoIdeia? Estado { get; init; }

    /// <summary>Só as ideias de que o utilizador é autor ("a minha caixa").</summary>
    public bool SoMinhas { get; init; }
}

public class ListarIdeiasQueryHandler : IRequestHandler<ListarIdeiasQuery, IReadOnlyList<IdeiaResumoDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUtilizadores _utilizadores;

    public ListarIdeiasQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    {
        _context = context;
        _utilizadores = utilizadores;
    }

    public async Task<IReadOnlyList<IdeiaResumoDto>> Handle(ListarIdeiasQuery request, CancellationToken cancellationToken)
    {
        var eu = await _utilizadores.AtualAsync(cancellationToken);
        var admin = _utilizadores.EAdministrador;

        var consulta = _context.Ideias.AsNoTracking();
        if (request.Estado is not null)
        {
            consulta = consulta.Where(i => i.Estado == request.Estado);
        }

        var linhas = await consulta
            .OrderByDescending(i => i.Numero)
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                i.Titulo,
                i.Tipo,
                i.Anonima,
                Autor = i.Autor != null ? i.Autor.Nome : null,
                i.AutorId,
                i.CreatedBy,
                Coautores = i.Coautores.Select(c => new { c.UtilizadorId, c.Utilizador.Nome }).ToList(),
                i.DataSubmissao,
                i.Estado,
                i.Privada,
                Likes = i.Likes.Count,
                Comentarios = i.Comentarios.Count,
                i.Responsabilidade,
                i.Classificacao,
                i.Classe,
                Nota = i.Avaliacao != null ? i.Avaliacao.Nota : null,
            })
            .ToListAsync(cancellationToken);

        return linhas
            .Where(l => RegrasVisibilidade.PodeVer(admin, eu.Id, eu.IdentityId, l.AutorId, l.CreatedBy,
                l.Coautores.Select(c => c.UtilizadorId), l.Privada, l.Estado))
            .Where(l => !request.SoMinhas || l.AutorId == eu.Id || l.CreatedBy == eu.IdentityId || l.Coautores.Any(c => c.UtilizadorId == eu.Id))
            .Select(l => new IdeiaResumoDto(
                l.Id, l.Codigo, l.Titulo, l.Tipo,
                RegrasVisibilidade.Autores(l.Anonima, l.Autor, l.Coautores.Select(c => c.Nome)),
                l.DataSubmissao, l.Estado, l.Privada, l.Likes, l.Comentarios, l.Responsabilidade, l.Classificacao, l.Classe, l.Nota))
            .ToList();
    }
}
