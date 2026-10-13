using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;
using Innovera.Application.Common.Interfaces;

namespace Innovera.Infrastructure.Services;

/// <summary>
/// Lê os valores das folhas de um .xlsx diretamente do XML (um .xlsx é um zip com um XML por folha).
/// Chega para importar os modelos Excel da dst sem depender de bibliotecas externas.
/// </summary>
public class LeitorXlsx : ILeitorFolhas
{
    private static readonly XNamespace Ss = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Pr = "http://schemas.openxmlformats.org/package/2006/relationships";

    public IReadOnlyList<FolhaLida> Ler(Stream conteudo)
    {
        if (!conteudo.CanSeek)
        {
            var copia = new MemoryStream();
            conteudo.CopyTo(copia);
            copia.Position = 0;
            conteudo = copia;
        }

        using var zip = new ZipArchive(conteudo, ZipArchiveMode.Read, leaveOpen: true);
        var livro = Carregar(zip, "xl/workbook.xml") ?? throw new InvalidDataException("Falta xl/workbook.xml.");
        var partilhados = TextosPartilhados(zip);
        var estilos = CoresDosEstilos(zip);
        var alvos = Carregar(zip, "xl/_rels/workbook.xml.rels")?.Root?
            .Elements(Pr + "Relationship")
            .ToDictionary(r => (string)r.Attribute("Id")!, r => (string)r.Attribute("Target")!)
            ?? new Dictionary<string, string>();

        var folhas = new List<FolhaLida>();
        foreach (var folha in livro.Root?.Element(Ss + "sheets")?.Elements(Ss + "sheet") ?? [])
        {
            var nome = (string?)folha.Attribute("name") ?? string.Empty;
            var id = (string?)folha.Attribute(R + "id");
            if (id is null || !alvos.TryGetValue(id, out var alvo))
            {
                continue;
            }

            var caminho = alvo.StartsWith('/') ? alvo.TrimStart('/') : "xl/" + alvo;
            var xml = Carregar(zip, caminho);
            if (xml is not null)
            {
                var cores = new Dictionary<(int Linha, int Coluna), IReadOnlyList<string>>();
                folhas.Add(new FolhaLida(nome, Linhas(xml, partilhados, estilos, cores), cores));
            }
        }

        return folhas;
    }

    private static List<object?[]> Linhas(XDocument folha, IReadOnlyList<string> partilhados, IReadOnlyList<IReadOnlyList<string>> estilos,
        Dictionary<(int Linha, int Coluna), IReadOnlyList<string>> cores)
    {
        var linhas = new List<object?[]>();
        var dados = folha.Root?.Element(Ss + "sheetData");
        if (dados is null)
        {
            return linhas;
        }

        var proximaLinha = 1;
        foreach (var linha in dados.Elements(Ss + "row"))
        {
            // As linhas vazias não aparecem no XML: mantêm-se como linhas vazias para os números baterem certo.
            var numero = (int?)linha.Attribute("r") ?? proximaLinha;
            while (proximaLinha < numero)
            {
                linhas.Add([]);
                proximaLinha++;
            }

            var valores = new List<object?>();
            var proximaColuna = 0;
            foreach (var celula in linha.Elements(Ss + "c"))
            {
                var referencia = (string?)celula.Attribute("r");
                var coluna = referencia is null ? proximaColuna : Coluna(referencia);
                while (valores.Count < coluna)
                {
                    valores.Add(null);
                }

                valores.Add(Valor(celula, partilhados));
                if ((int?)celula.Attribute("s") is int estilo && estilo >= 0 && estilo < estilos.Count && estilos[estilo].Count > 0)
                {
                    cores[(numero - 1, coluna)] = estilos[estilo];
                }

                proximaColuna = coluna + 1;
            }

            linhas.Add([.. valores]);
            proximaLinha = numero + 1;
        }

        return linhas;
    }

    private static object? Valor(XElement celula, IReadOnlyList<string> partilhados)
    {
        var tipo = (string?)celula.Attribute("t");
        var v = celula.Element(Ss + "v")?.Value;
        switch (tipo)
        {
            case "s":
                return int.TryParse(v, out var i) && i >= 0 && i < partilhados.Count ? partilhados[i] : null;
            case "inlineStr":
                return string.Concat(celula.Element(Ss + "is")?.Descendants(Ss + "t").Select(t => t.Value) ?? []);
            case "str":
                return v;
            case "b":
                return v == "1";
            case "e":
                return null;
            default:
                if (v is null)
                {
                    return null;
                }

                return double.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out var numero) ? numero : v;
        }
    }

    /// <summary>"B7" → 1 (coluna A = 0).</summary>
    private static int Coluna(string referencia)
    {
        var coluna = 0;
        foreach (var c in referencia)
        {
            if (!char.IsLetter(c))
            {
                break;
            }

            coluna = (coluna * 26) + (char.ToUpperInvariant(c) - 'A' + 1);
        }

        return coluna - 1;
    }

    // ---------- Cores de fundo (styles.xml + tema) ----------

    private static readonly XNamespace A = "http://schemas.openxmlformats.org/drawingml/2006/main";

    /// <summary>Para cada estilo de célula (cellXfs), as cores do fundo em "RRGGBB" (vazio se não tiver fundo).</summary>
    private static List<IReadOnlyList<string>> CoresDosEstilos(ZipArchive zip)
    {
        var estilos = Carregar(zip, "xl/styles.xml")?.Root;
        if (estilos is null)
        {
            return [];
        }

        var tema = CoresDoTema(zip);
        var fundos = (estilos.Element(Ss + "fills")?.Elements(Ss + "fill") ?? [])
            .Select(f => CoresDoFundo(f, tema))
            .ToList();

        return (estilos.Element(Ss + "cellXfs")?.Elements(Ss + "xf") ?? [])
            .Select(xf => (int?)xf.Attribute("fillId") is int id && id >= 0 && id < fundos.Count ? fundos[id] : Array.Empty<string>())
            .ToList();
    }

    private static IReadOnlyList<string> CoresDoFundo(XElement fundo, IReadOnlyList<string> tema)
    {
        var padrao = fundo.Element(Ss + "patternFill");
        if (padrao is not null)
        {
            var tipo = (string?)padrao.Attribute("patternType");
            if (tipo is null or "none" or "gray125")
            {
                return Array.Empty<string>();
            }

            return Cor(padrao.Element(Ss + "fgColor"), tema) is { } cor ? [cor] : Array.Empty<string>();
        }

        var gradiente = fundo.Element(Ss + "gradientFill");
        if (gradiente is not null)
        {
            return gradiente.Elements(Ss + "stop")
                .Select(p => Cor(p.Element(Ss + "color"), tema))
                .OfType<string>()
                .Distinct()
                .ToList();
        }

        return Array.Empty<string>();
    }

    /// <summary>Cor "RRGGBB" de um elemento color/fgColor (rgb direto ou cor do tema com tint).</summary>
    private static string? Cor(XElement? cor, IReadOnlyList<string> tema)
    {
        if (cor is null)
        {
            return null;
        }

        string? rgb = null;
        var direto = (string?)cor.Attribute("rgb");
        if (direto is { Length: >= 6 })
        {
            rgb = direto[^6..];
        }
        else if ((int?)cor.Attribute("theme") is int indice && indice >= 0 && indice < tema.Count)
        {
            rgb = tema[indice];
        }

        if (rgb is null)
        {
            return null;
        }

        var tint = (double?)cor.Attribute("tint") ?? 0;
        return (tint == 0 ? rgb : AplicarTint(rgb, tint)).ToUpperInvariant();
    }

    /// <summary>
    /// Cores do tema pela ordem dos índices do Excel: 0 = lt1, 1 = dk1, 2 = lt2, 3 = dk2, 4–9 = accent1–6, 10 = hlink, 11 = folHlink
    /// (no XML a ordem é dk1, lt1, dk2, lt2…).
    /// </summary>
    private static List<string> CoresDoTema(ZipArchive zip)
    {
        var esquema = Carregar(zip, "xl/theme/theme1.xml")?.Descendants(A + "clrScheme").FirstOrDefault();
        if (esquema is null)
        {
            return [];
        }

        string CorDe(string nome)
        {
            var e = esquema.Element(A + nome);
            return (string?)e?.Element(A + "srgbClr")?.Attribute("val")
                ?? (string?)e?.Element(A + "sysClr")?.Attribute("lastClr")
                ?? "000000";
        }

        return
        [
            CorDe("lt1"), CorDe("dk1"), CorDe("lt2"), CorDe("dk2"),
            CorDe("accent1"), CorDe("accent2"), CorDe("accent3"), CorDe("accent4"), CorDe("accent5"), CorDe("accent6"),
            CorDe("hlink"), CorDe("folHlink"),
        ];
    }

    /// <summary>Clareia (tint &gt; 0) ou escurece (tint &lt; 0) a cor como o Excel: muda a luminosidade em HLS.</summary>
    private static string AplicarTint(string rgb, double tint)
    {
        var r = Convert.ToInt32(rgb[..2], 16) / 255.0;
        var g = Convert.ToInt32(rgb[2..4], 16) / 255.0;
        var b = Convert.ToInt32(rgb[4..6], 16) / 255.0;
        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var l = (max + min) / 2;
        double h = 0, s = 0;
        if (max != min)
        {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            h = max == r ? ((g - b) / d) + (g < b ? 6 : 0) : max == g ? ((b - r) / d) + 2 : ((r - g) / d) + 4;
            h /= 6;
        }

        l = tint < 0 ? l * (1 + tint) : (l * (1 - tint)) + tint;

        static double Canal(double p, double q, double t)
        {
            if (t < 0)
            {
                t += 1;
            }

            if (t > 1)
            {
                t -= 1;
            }

            return t < 1.0 / 6 ? p + ((q - p) * 6 * t) : t < 0.5 ? q : t < 2.0 / 3 ? p + ((q - p) * ((2.0 / 3) - t) * 6) : p;
        }

        double nr = l, ng = l, nb = l;
        if (s != 0)
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - (l * s);
            var p = (2 * l) - q;
            nr = Canal(p, q, h + (1.0 / 3));
            ng = Canal(p, q, h);
            nb = Canal(p, q, h - (1.0 / 3));
        }

        static int Byte(double v) => (int)Math.Round(Math.Clamp(v, 0, 1) * 255);
        return $"{Byte(nr):X2}{Byte(ng):X2}{Byte(nb):X2}";
    }

    private static List<string> TextosPartilhados(ZipArchive zip)
        => Carregar(zip, "xl/sharedStrings.xml")?.Root?
            .Elements(Ss + "si")
            .Select(si => string.Concat(si.Descendants(Ss + "t").Select(t => t.Value)))
            .ToList() ?? [];

    private static XDocument? Carregar(ZipArchive zip, string caminho)
    {
        var entrada = zip.GetEntry(caminho)
            ?? zip.Entries.FirstOrDefault(e => string.Equals(e.FullName, caminho, StringComparison.OrdinalIgnoreCase));
        if (entrada is null)
        {
            return null;
        }

        using var stream = entrada.Open();
        return XDocument.Load(stream);
    }
}
