using System.Globalization;
using System.Text.RegularExpressions;
using Innovera.Application.Iniciativas;

namespace Innovera.Application.Importacoes;

/// <summary>Leitura dos valores das células dos modelos Excel da dstelecom (textos, números, datas em vários formatos).</summary>
internal static partial class Celulas
{
    /// <summary>Minúsculas, sem acentos e com os espaços normalizados.</summary>
    public static string Normalizar(string texto) => ImportarPortefolioCommandHandler.Normalizar(texto);

    /// <summary>Só letras e algarismos, sem acentos ("AR & VR" → "arvr"), para comparar nomes escritos de formas diferentes.</summary>
    public static string Chave(string? texto)
        => texto is null ? string.Empty : new string(Normalizar(texto).Where(char.IsLetterOrDigit).ToArray());

    public static string? Texto(object? valor) => valor switch
    {
        null => null,
        string s => string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
        double d => d.ToString(CultureInfo.InvariantCulture),
        bool b => b ? "Sim" : "Não",
        _ => valor.ToString(),
    };

    public static string? Cortar(string? texto, int maximo) => texto is null || texto.Length <= maximo ? texto : texto[..maximo];

    /// <summary>Vazio, "N.A.", "n.a.", "-" contam como sem valor.</summary>
    public static bool SemValor(string? texto)
        => texto is null || Normalizar(texto) is "" or "n.a." or "n.a" or "na" or "-" or "—" or "n/a";

    public static int? Inteiro(object? valor) => valor switch
    {
        double d when d == Math.Floor(d) && d is >= int.MinValue and <= int.MaxValue => (int)d,
        string s when int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) => n,
        _ => null,
    };

    /// <summary>Número (aceita "1 200,50", "1.200,50", "30k€"…); null se não for um número.</summary>
    public static decimal? Numero(object? valor)
    {
        switch (valor)
        {
            case double d when !double.IsNaN(d) && !double.IsInfinity(d):
                return (decimal)d;
            case string s:
                var t = new string(s.Where(c => char.IsDigit(c) || c is ',' or '.' or '-').ToArray());
                if (t.Length == 0 || SemValor(s))
                {
                    return null;
                }

                if (t.Contains(','))
                {
                    t = t.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.');
                }
                else if (t.Count(c => c == '.') > 1)
                {
                    t = t.Replace(".", string.Empty, StringComparison.Ordinal);
                }

                return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out var n) ? n : null;
            default:
                return null;
        }
    }

    private static readonly string[] FormatosData = ["d.M.yyyy", "d/M/yyyy", "d-M-yyyy", "yyyy-MM-dd", "d.M.yy", "d/M/yy"];

    /// <summary>Data a partir de um número do Excel ou de texto ("31.01.2026", "16/09/2026"…).</summary>
    public static DateOnly? Data(object? valor)
    {
        switch (valor)
        {
            case double d when d is > 1 and < 2958466:
                return DateOnly.FromDateTime(DateTime.FromOADate(d));
            case string s:
                var t = s.Trim();
                return DateTime.TryParseExact(t, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data)
                    ? DateOnly.FromDateTime(data)
                    : null;
            default:
                return null;
        }
    }

    /// <summary>Primeira data dentro de um texto ("Data da atualização: 16/09/2026").</summary>
    public static DateOnly? DataNoTexto(string? texto)
    {
        if (texto is null)
        {
            return null;
        }

        var m = DataRegex().Match(texto);
        return m.Success ? Data(m.Value.Replace('/', '.').Replace('-', '.')) : null;
    }

    /// <summary>"01.07.2020 - 30.06.2023", "2020 - 2024", "13.05.2026 - Sem fim" → início e fim.</summary>
    public static (DateOnly? Inicio, DateOnly? Fim) Intervalo(object? valor)
    {
        if (valor is double)
        {
            return (Data(valor), null);
        }

        var texto = Texto(valor);
        if (texto is null)
        {
            return (null, null);
        }

        var partes = texto.Split([" - ", " – ", " a ", "–"], 2, StringSplitOptions.TrimEntries);
        DateOnly? Parte(string p, bool fim)
        {
            if (Data(p) is { } d)
            {
                return d;
            }

            return int.TryParse(p, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ano) && ano is >= 1990 and <= 2100
                ? fim ? new DateOnly(ano, 12, 31) : new DateOnly(ano, 1, 1)
                : null;
        }

        return partes.Length == 2 ? (Parte(partes[0], false), Parte(partes[1], true)) : (Parte(partes[0], false), null);
    }

    /// <summary>Números de KPI de um texto ("14 e 36", "11, 35", 23).</summary>
    public static List<int> Numeros(object? valor)
    {
        if (Inteiro(valor) is int n)
        {
            return [n];
        }

        var texto = Texto(valor);
        return texto is null
            ? []
            : NumeroRegex().Matches(texto).Select(m => int.Parse(m.Value, CultureInfo.InvariantCulture)).Distinct().ToList();
    }

    /// <summary>Ano de um texto ("Ano: 2026", "2025 - 1ºS").</summary>
    public static int? Ano(string? texto)
    {
        if (texto is null)
        {
            return null;
        }

        var m = AnoRegex().Match(texto);
        return m.Success ? int.Parse(m.Value, CultureInfo.InvariantCulture) : null;
    }

    [GeneratedRegex(@"\d{1,2}[./-]\d{1,2}[./-]\d{2,4}")]
    private static partial Regex DataRegex();

    [GeneratedRegex(@"\d+")]
    private static partial Regex NumeroRegex();

    [GeneratedRegex(@"\b(19|20)\d{2}\b")]
    private static partial Regex AnoRegex();
}

/// <summary>
/// Uma tabela de um modelo: a linha de cabeçalho é a primeira que tem uma célula a começar pelo texto-chave; quando o cabeçalho
/// ocupa duas linhas (grupos de colunas por cima), as colunas vazias ficam com o nome da linha de cima.
/// </summary>
internal sealed class TabelaModelo
{
    private readonly FolhaLida _folha;
    private readonly List<(string Nome, int Coluna)> _colunas = [];

    public int LinhaCabecalho { get; } = -1;

    public bool Encontrada => LinhaCabecalho >= 0;

    public TabelaModelo(FolhaLida folha, string chave)
    {
        _folha = folha;
        var alvo = Celulas.Normalizar(chave);
        for (var l = 0; l < folha.Linhas.Count && LinhaCabecalho < 0; l++)
        {
            if (folha.Linhas[l].Any(v => v is string s && Celulas.Normalizar(s).StartsWith(alvo, StringComparison.Ordinal)))
            {
                LinhaCabecalho = l;
            }
        }

        if (LinhaCabecalho < 0)
        {
            return;
        }

        var cabecalho = folha.Linhas[LinhaCabecalho];
        var acima = LinhaCabecalho > 0 ? folha.Linhas[LinhaCabecalho - 1] : Array.Empty<object?>();
        var colunas = Math.Max(cabecalho.Length, acima.Length);
        for (var c = 0; c < colunas; c++)
        {
            var nome = Celulas.Texto(c < cabecalho.Length ? cabecalho[c] : null) ?? Celulas.Texto(c < acima.Length ? acima[c] : null);
            if (nome is not null)
            {
                _colunas.Add((Celulas.Normalizar(nome), c));
            }
        }
    }

    /// <summary>Linhas de dados (as que têm algum valor), com o índice da linha (a contar de 0).</summary>
    public IEnumerable<(object?[] Linha, int Indice)> Dados()
    {
        if (!Encontrada)
        {
            yield break;
        }

        for (var l = LinhaCabecalho + 1; l < _folha.Linhas.Count; l++)
        {
            var linha = _folha.Linhas[l];
            if (linha.Any(v => Celulas.Texto(v) is not null))
            {
                yield return (linha, l);
            }
        }
    }

    /// <summary>Índice da primeira coluna cujo cabeçalho começa por um dos nomes (null se não houver).</summary>
    public int? Coluna(params string[] nomes)
    {
        foreach (var nome in nomes.Select(Celulas.Normalizar))
        {
            var coluna = _colunas.FirstOrDefault(c => c.Nome.StartsWith(nome, StringComparison.Ordinal));
            if (coluna.Nome is not null)
            {
                return coluna.Coluna;
            }
        }

        return null;
    }

    public static object? Valor(object?[] linha, int? coluna) => coluna is int c && c < linha.Length ? linha[c] : null;
}
