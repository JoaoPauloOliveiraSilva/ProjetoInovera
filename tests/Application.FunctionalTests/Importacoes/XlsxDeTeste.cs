using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace Innovera.Application.FunctionalTests.Importacoes;

/// <summary>
/// Monta um .xlsx pequeno para os testes da importação (valores e cores de fundo), sem dados reais da dstelecom.
/// Cores: "C00000" (rgb), "tema:4:0.4" (cor 4 do tema com tint 0,4) ou duas cores para um gradiente.
/// </summary>
public sealed class XlsxDeTeste
{
    private readonly List<(string Nome, SortedDictionary<(int Linha, int Coluna), (object? Valor, string[] Cores)> Celulas)> _folhas = [];

    public XlsxDeTeste Folha(string nome)
    {
        _folhas.Add((nome, new SortedDictionary<(int, int), (object?, string[])>()));
        return this;
    }

    /// <summary>Uma célula ("B7") com valor (texto, número, DateOnly ou null) e, opcionalmente, cores de fundo.</summary>
    public XlsxDeTeste Celula(string referencia, object? valor, params string[] cores)
    {
        _folhas[^1].Celulas[Posicao(referencia)] = (valor, cores);
        return this;
    }

    /// <summary>Os valores de uma linha a partir da coluna A (null = célula vazia).</summary>
    public XlsxDeTeste Linha(int linha, params object?[] valores)
    {
        for (var c = 0; c < valores.Length; c++)
        {
            if (valores[c] is not null)
            {
                _folhas[^1].Celulas[(linha, c)] = (valores[c], []);
            }
        }

        return this;
    }

    public MemoryStream Gerar()
    {
        // Um estilo por combinação de cores (0 = sem fundo).
        var combinacoes = _folhas.SelectMany(f => f.Celulas.Values)
            .Where(c => c.Cores.Length > 0)
            .Select(c => string.Join("+", c.Cores))
            .Distinct()
            .ToList();

        var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            Escrever(zip, "[Content_Types].xml",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\">"
                + "<Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/>"
                + "<Default Extension=\"xml\" ContentType=\"application/xml\"/></Types>");
            Escrever(zip, "_rels/.rels",
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">"
                + "<Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/>"
                + "</Relationships>");

            var livro = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" "
                + "xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets>");
            var relacoes = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\">");
            for (var i = 0; i < _folhas.Count; i++)
            {
                livro.Append(CultureInfo.InvariantCulture, $"<sheet name=\"{SecurityElement.Escape(_folhas[i].Nome)}\" sheetId=\"{i + 1}\" r:id=\"rId{i + 1}\"/>");
                relacoes.Append(CultureInfo.InvariantCulture,
                    $"<Relationship Id=\"rId{i + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet{i + 1}.xml\"/>");
                Escrever(zip, $"xl/worksheets/sheet{i + 1}.xml", FolhaXml(_folhas[i].Celulas, combinacoes));
            }

            livro.Append("</sheets></workbook>");
            relacoes.Append(CultureInfo.InvariantCulture,
                $"<Relationship Id=\"rId{_folhas.Count + 1}\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
            relacoes.Append("</Relationships>");
            Escrever(zip, "xl/workbook.xml", livro.ToString());
            Escrever(zip, "xl/_rels/workbook.xml.rels", relacoes.ToString());
            Escrever(zip, "xl/styles.xml", Estilos(combinacoes));
            Escrever(zip, "xl/theme/theme1.xml", Tema);
        }

        memoria.Position = 0;
        return memoria;
    }

    private static string FolhaXml(SortedDictionary<(int Linha, int Coluna), (object? Valor, string[] Cores)> celulas, List<string> combinacoes)
    {
        var sb = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        foreach (var linha in celulas.GroupBy(c => c.Key.Linha))
        {
            sb.Append(CultureInfo.InvariantCulture, $"<row r=\"{linha.Key}\">");
            foreach (var ((l, c), (valor, cores)) in linha.Select(x => (x.Key, x.Value)))
            {
                var referencia = $"{Letras(c)}{l}";
                var estilo = cores.Length == 0 ? string.Empty : $" s=\"{combinacoes.IndexOf(string.Join("+", cores)) + 1}\"";
                switch (valor)
                {
                    case null:
                        sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\"{estilo}/>");
                        break;
                    case string texto:
                        sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\"{estilo} t=\"inlineStr\"><is><t xml:space=\"preserve\">{SecurityElement.Escape(texto)}</t></is></c>");
                        break;
                    case DateOnly data:
                        sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\"{estilo}><v>{data.ToDateTime(TimeOnly.MinValue).ToOADate()}</v></c>");
                        break;
                    default:
                        sb.Append(CultureInfo.InvariantCulture, $"<c r=\"{referencia}\"{estilo}><v>{Convert.ToString(valor, CultureInfo.InvariantCulture)}</v></c>");
                        break;
                }
            }

            sb.Append("</row>");
        }

        return sb.Append("</sheetData></worksheet>").ToString();
    }

    private static string Estilos(List<string> combinacoes)
    {
        var fundos = new StringBuilder("<fill><patternFill patternType=\"none\"/></fill><fill><patternFill patternType=\"gray125\"/></fill>");
        foreach (var combinacao in combinacoes)
        {
            var cores = combinacao.Split('+');
            if (cores.Length == 1)
            {
                fundos.Append(CultureInfo.InvariantCulture, $"<fill><patternFill patternType=\"solid\">{CorXml("fgColor", cores[0])}<bgColor indexed=\"64\"/></patternFill></fill>");
            }
            else
            {
                fundos.Append("<fill><gradientFill degree=\"90\">");
                for (var i = 0; i < cores.Length; i++)
                {
                    fundos.Append(CultureInfo.InvariantCulture, $"<stop position=\"{i}\">{CorXml("color", cores[i])}</stop>");
                }

                fundos.Append("</gradientFill></fill>");
            }
        }

        var xfs = new StringBuilder("<xf numFmtId=\"0\" fontId=\"0\" fillId=\"0\" borderId=\"0\"/>");
        for (var i = 0; i < combinacoes.Count; i++)
        {
            xfs.Append(CultureInfo.InvariantCulture, $"<xf numFmtId=\"0\" fontId=\"0\" fillId=\"{i + 2}\" borderId=\"0\" applyFill=\"1\"/>");
        }

        return "<?xml version=\"1.0\" encoding=\"UTF-8\"?><styleSheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\">"
            + "<fonts count=\"1\"><font><sz val=\"11\"/></font></fonts>"
            + $"<fills count=\"{combinacoes.Count + 2}\">{fundos}</fills>"
            + "<borders count=\"1\"><border/></borders>"
            + $"<cellXfs count=\"{combinacoes.Count + 1}\">{xfs}</cellXfs></styleSheet>";
    }

    private static string CorXml(string elemento, string cor)
    {
        if (cor.StartsWith("tema:", StringComparison.Ordinal))
        {
            var partes = cor.Split(':');
            return $"<{elemento} theme=\"{partes[1]}\" tint=\"{partes[2]}\"/>";
        }

        return $"<{elemento} rgb=\"FF{cor}\"/>";
    }

    // Tema do Office: lt1 branco, dk1 preto, accent1 4472C4…
    private const string Tema = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><a:theme xmlns:a=\"http://schemas.openxmlformats.org/drawingml/2006/main\" name=\"Office\">"
        + "<a:themeElements><a:clrScheme name=\"Office\">"
        + "<a:dk1><a:sysClr val=\"windowText\" lastClr=\"000000\"/></a:dk1><a:lt1><a:sysClr val=\"window\" lastClr=\"FFFFFF\"/></a:lt1>"
        + "<a:dk2><a:srgbClr val=\"44546A\"/></a:dk2><a:lt2><a:srgbClr val=\"E7E6E6\"/></a:lt2>"
        + "<a:accent1><a:srgbClr val=\"4472C4\"/></a:accent1><a:accent2><a:srgbClr val=\"ED7D31\"/></a:accent2>"
        + "<a:accent3><a:srgbClr val=\"A5A5A5\"/></a:accent3><a:accent4><a:srgbClr val=\"FFC000\"/></a:accent4>"
        + "<a:accent5><a:srgbClr val=\"5B9BD5\"/></a:accent5><a:accent6><a:srgbClr val=\"70AD47\"/></a:accent6>"
        + "<a:hlink><a:srgbClr val=\"0563C1\"/></a:hlink><a:folHlink><a:srgbClr val=\"954F72\"/></a:folHlink>"
        + "</a:clrScheme></a:themeElements></a:theme>";

    private static void Escrever(ZipArchive zip, string caminho, string conteudo)
    {
        using var escritor = new StreamWriter(zip.CreateEntry(caminho).Open(), new UTF8Encoding(false));
        escritor.Write(conteudo);
    }

    private static (int Linha, int Coluna) Posicao(string referencia)
    {
        var coluna = 0;
        var i = 0;
        for (; i < referencia.Length && char.IsLetter(referencia[i]); i++)
        {
            coluna = (coluna * 26) + (char.ToUpperInvariant(referencia[i]) - 'A' + 1);
        }

        return (int.Parse(referencia[i..], CultureInfo.InvariantCulture), coluna - 1);
    }

    private static string Letras(int coluna)
    {
        var texto = string.Empty;
        for (var n = coluna + 1; n > 0; n = (n - 1) / 26)
        {
            texto = (char)('A' + ((n - 1) % 26)) + texto;
        }

        return texto;
    }
}
