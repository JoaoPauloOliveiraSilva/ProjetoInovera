namespace Innovera.Application.Ideias.Queries;

/// <summary>Lista (e portefólio) de ideias visíveis para o utilizador atual, com filtros.</summary>
[Authorize]
public record ListarIdeiasQuery : IRequest<IReadOnlyList<IdeiaResumoDto>>
{
    /// <summary>Filtra por estado (ex.: EmDiscussao para "ideias em discussão", EmAvaliacao para a página de avaliação).</summary>
    public EstadoIdeia? Estado { get; init; }

    /// <summary>Só as ideias de que o utilizador é autor ("a minha caixa").</summary>
    public bool SoMinhas { get; init; }

    /// <summary>Só as ideias já decididas (aprovadas ou não aprovadas).</summary>
    public bool SoDecididas { get; init; }

    /// <summary>Procura no título, no código e no n.º da Caixa de Inovação.</summary>
    public string? Texto { get; init; }

    public ClasseIdeia? Classe { get; init; }

    public Responsabilidade? Responsabilidade { get; init; }

    /// <summary>Ano de submissão.</summary>
    public int? Ano { get; init; }
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
        var admin = await _utilizadores.TemPermissaoAsync(Permissoes.Ideias.VerTodas, cancellationToken);

        var consulta = _context.Ideias.AsNoTracking();
        if (request.Estado is not null)
        {
            consulta = consulta.Where(i => i.Estado == request.Estado);
        }

        if (request.SoDecididas)
        {
            consulta = consulta.Where(i => i.Estado == EstadoIdeia.Aprovada || i.Estado == EstadoIdeia.NaoAprovada);
        }

        if (request.Classe is not null)
        {
            consulta = consulta.Where(i => i.Classe == request.Classe);
        }

        if (request.Responsabilidade is not null)
        {
            consulta = consulta.Where(i => i.Responsabilidade == request.Responsabilidade);
        }

        if (request.Ano is not null)
        {
            var inicio = new DateTimeOffset(request.Ano.Value, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var fim = inicio.AddYears(1);
            consulta = consulta.Where(i => i.DataSubmissao >= inicio && i.DataSubmissao < fim);
        }

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(i => i.Titulo.ToLower().Contains(texto) || i.Codigo.ToLower().Contains(texto)
                || (i.IdCaixaInovacao != null && i.IdCaixaInovacao.ToString()!.Contains(texto)));
        }

        var linhas = await consulta
            .OrderByDescending(i => i.DataSubmissao)
            .ThenByDescending(i => i.Numero)
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                i.Titulo,
                i.Tipo,
                i.Anonima,
                Autor = i.Autor != null ? i.Autor.Nome : i.AutorNome,
                i.AutorId,
                i.CreatedBy,
                Coautores = i.Coautores.Select(c => new { c.UtilizadorId, c.Utilizador.Nome, c.Confirmado }).ToList(),
                i.DataSubmissao,
                i.Estado,
                i.Privada,
                Likes = i.Likes.Count,
                Comentarios = i.Comentarios.Count,
                i.Responsabilidade,
                i.Classificacao,
                i.Classe,
                Nota = i.Avaliacao != null ? i.Avaliacao.Nota : null,
                i.IdCaixaInovacao,
            })
            .ToListAsync(cancellationToken);

        return linhas
            .Where(l => RegrasVisibilidade.PodeVer(admin, eu.Id, eu.IdentityId, l.AutorId, l.CreatedBy,
                l.Coautores.Select(c => c.UtilizadorId), l.Privada, l.Estado))
            .Where(l => !request.SoMinhas || l.AutorId == eu.Id || l.CreatedBy == eu.IdentityId || l.Coautores.Any(c => c.UtilizadorId == eu.Id))
            .Select(l => new IdeiaResumoDto(
                l.Id, l.Codigo, l.Titulo, l.Tipo,
                RegrasVisibilidade.Autores(l.Anonima, l.Autor, l.Coautores.Select(c => c.Nome)),
                l.DataSubmissao, l.Estado, l.Privada, l.Likes, l.Comentarios, l.Responsabilidade, l.Classificacao, l.Classe, l.Nota,
                l.IdCaixaInovacao,
                l.Estado == EstadoIdeia.ValidacaoAutores && l.Coautores.Any(c => c.UtilizadorId == eu.Id && !c.Confirmado)))
            .ToList();
    }
}
