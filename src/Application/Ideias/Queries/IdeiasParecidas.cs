using Innovera.Application.Iniciativas;

namespace Innovera.Application.Ideias.Queries;

public record IdeiaParecidaDto(int Id, string Codigo, string Titulo, EstadoIdeia Estado, ClasseIdeia? Classe, int Semelhanca);

/// <summary>
/// Ideias com título (e descrição) parecidos com os de uma ideia nova, para avisar antes de submeter uma ideia repetida.
/// Compara as palavras que contam (sem acentos, maiúsculas, plurais e palavras como "de", "para", "com").
/// </summary>
[Authorize]
public record ProcurarIdeiasParecidasQuery(string Titulo, string? Descricao = null) : IRequest<IReadOnlyList<IdeiaParecidaDto>>;

public class ProcurarIdeiasParecidasQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    : IRequestHandler<ProcurarIdeiasParecidasQuery, IReadOnlyList<IdeiaParecidaDto>>
{
    /// <summary>A partir desta semelhança (0 a 1) a ideia aparece no aviso.</summary>
    public const double Limiar = 0.5;

    public const int Maximo = 5;

    public async Task<IReadOnlyList<IdeiaParecidaDto>> Handle(ProcurarIdeiasParecidasQuery request, CancellationToken cancellationToken)
    {
        var titulo = SemelhancaTexto.Palavras(request.Titulo);
        if (titulo.Count < 2)
        {
            return [];
        }

        var descricao = SemelhancaTexto.Palavras(request.Descricao);
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var veTodas = await utilizadores.TemPermissaoAsync(Permissoes.Ideias.VerTodas, cancellationToken);

        var ideias = await context.Ideias.AsNoTracking()
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                i.Titulo,
                i.Descricao,
                i.Estado,
                i.Classe,
                i.Privada,
                i.AutorId,
                i.CreatedBy,
                Coautores = i.Coautores.Select(c => c.UtilizadorId).ToList(),
            })
            .ToListAsync(cancellationToken);

        return ideias
            .Where(i => RegrasVisibilidade.PodeVer(veTodas, eu.Id, eu.IdentityId, i.AutorId, i.CreatedBy, i.Coautores, i.Privada, i.Estado))
            .Select(i =>
            {
                var outroTitulo = SemelhancaTexto.Palavras(i.Titulo);
                var pelosTitulos = SemelhancaTexto.Dice(titulo, outroTitulo);

                // A descrição ajuda, mas conta menos: duas ideias da mesma área partilham muitas palavras.
                var pelosTextos = descricao.Count == 0
                    ? 0
                    : 0.8 * SemelhancaTexto.Dice(new HashSet<string>(titulo.Concat(descricao)),
                        new HashSet<string>(outroTitulo.Concat(SemelhancaTexto.Palavras(i.Descricao))));
                return (Ideia: i, Semelhanca: Math.Max(pelosTitulos, pelosTextos));
            })
            .Where(x => x.Semelhanca >= Limiar)
            .OrderByDescending(x => x.Semelhanca)
            .Take(Maximo)
            .Select(x => new IdeiaParecidaDto(x.Ideia.Id, x.Ideia.Codigo, x.Ideia.Titulo, x.Ideia.Estado, x.Ideia.Classe,
                (int)Math.Round(100 * x.Semelhanca)))
            .ToList();
    }
}

/// <summary>Semelhança entre textos curtos pelas palavras que contam.</summary>
public static class SemelhancaTexto
{
    private static readonly HashSet<string> Vazias =
    [
        "a", "o", "as", "os", "um", "uma", "uns", "umas", "de", "da", "do", "das", "dos", "em", "na", "no", "nas", "nos", "ao", "aos",
        "para", "pela", "pelo", "pelas", "pelos", "por", "com", "sem", "sobre", "entre", "e", "ou", "que", "se", "mais", "menos",
        "como", "ser", "ter", "nova", "novo", "novas", "novos", "ideia", "ideias", "melhoria", "melhorar", "criar", "utilizar",
        "uso", "dstelecom", "dst", "the", "of", "and",
    ];

    /// <summary>Palavras sem acentos, em minúsculas, sem as palavras vazias e sem o plural ("postes" = "poste").</summary>
    public static HashSet<string> Palavras(string? texto)
    {
        var palavras = new HashSet<string>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(texto))
        {
            return palavras;
        }

        var normalizado = ImportarPortefolioCommandHandler.Normalizar(texto);
        foreach (var bruta in normalizado.Split(Separadores, StringSplitOptions.RemoveEmptyEntries))
        {
            var palavra = new string(bruta.Where(char.IsLetterOrDigit).ToArray());
            if (palavra.Length < 2 || Vazias.Contains(palavra))
            {
                continue;
            }

            palavras.Add(Singular(palavra));
        }

        return palavras;
    }

    /// <summary>Coeficiente de Dice: 2 × palavras em comum / total de palavras (0 = nada em comum, 1 = iguais).</summary>
    public static double Dice(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 || b.Count == 0)
        {
            return 0;
        }

        var comuns = a.Count(b.Contains);
        return 2.0 * comuns / (a.Count + b.Count);
    }

    private static readonly char[] Separadores = [' ', '-', '/', '.', ',', ';', ':', '(', ')', '"', '\'', '\n', '\r', '\t', '&', '+'];

    private static string Singular(string palavra)
    {
        if (palavra.Length > 5 && palavra.EndsWith("oes", StringComparison.Ordinal))
        {
            return palavra[..^3] + "ao";
        }

        if (palavra.Length > 4 && palavra.EndsWith("is", StringComparison.Ordinal))
        {
            return palavra[..^2] + "l";
        }

        return palavra.Length > 3 && palavra.EndsWith('s') ? palavra[..^1] : palavra;
    }
}
