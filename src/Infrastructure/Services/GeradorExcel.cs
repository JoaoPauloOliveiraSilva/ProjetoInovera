using System.Globalization;
using System.IO.Compression;
using System.Text;
using System.Xml;
using Innovera.Application.Common.Interfaces;

namespace Innovera.Infrastructure.Services;

/// <summary>
/// Escreve um .xlsx (Office Open XML) sem bibliotecas externas: uma folha por tabela, com título, cabeçalho a negrito
/// fixo, filtro e larguras de coluna ajustadas ao conteúdo.
/// </summary>
public class GeradorExcel : IGeradorExcel
{
    private const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string NsRel = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private const int LinhaCabecalho = 3;

    // Estilos (styles.xml): 0 normal, 1 cabeçalho, 2 data, 3 título, 4 texto com quebra de linha
    private const int EstiloCabecalho = 1;
    private const int EstiloData = 2;
    private const int EstiloTitulo = 3;
    private const int EstiloTexto = 4;

    public byte[] Gerar(IReadOnlyList<FolhaExcel> folhas)
    {
        ArgumentNullException.ThrowIfNull(folhas);
        if (folhas.Count == 0)
        {
            throw new ArgumentException("É preciso pelo menos uma folha.", nameof(folhas));
        }

        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            var nomes = NomesUnicos(folhas.Select(f => f.Nome).ToList());
            Escrever(zip, "[Content_Types].xml", w => ContentTypes(w, folhas.Count));
            Escrever(zip, "_rels/.rels", Rels);
            Escrever(zip, "xl/workbook.xml", w => Workbook(w, nomes));
            Escrever(zip, "xl/_rels/workbook.xml.rels", w => WorkbookRels(w, folhas.Count));
            Escrever(zip, "xl/styles.xml", Estilos);
            for (var i = 0; i < folhas.Count; i++)
            {
                var folha = folhas[i];
                Escrever(zip, $"xl/worksheets/sheet{i + 1}.xml", w => Folha(w, folha));
            }
        }

        return memoria.ToArray();
    }

    private static void Escrever(ZipArchive zip, string caminho, Action<XmlWriter> conteudo)
    {
        var entrada = zip.CreateEntry(caminho, CompressionLevel.Optimal);
        using var stream = entrada.Open();
        using var w = XmlWriter.Create(stream, new XmlWriterSettings { Encoding = new UTF8Encoding(false), Indent = false });
        w.WriteStartDocument(true);
        conteudo(w);
        w.WriteEndDocument();
    }

    private static void ContentTypes(XmlWriter w, int folhas)
    {
        const string ct = "http://schemas.openxmlformats.org/package/2006/content-types";
        w.WriteStartElement("Types", ct);
        Default(w, ct, "rels", "application/vnd.openxmlformats-package.relationships+xml");
        Default(w, ct, "xml", "application/xml");
        Override(w, ct, "/xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");
        Override(w, ct, "/xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml");
        for (var i = 1; i <= folhas; i++)
        {
            Override(w, ct, $"/xl/worksheets/sheet{i}.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");
        }

        w.WriteEndElement();

        static void Default(XmlWriter w, string ns, string extensao, string tipo)
        {
            w.WriteStartElement("Default", ns);
            w.WriteAttributeString("Extension", extensao);
            w.WriteAttributeString("ContentType", tipo);
            w.WriteEndElement();
        }

        static void Override(XmlWriter w, string ns, string parte, string tipo)
        {
            w.WriteStartElement("Override", ns);
            w.WriteAttributeString("PartName", parte);
            w.WriteAttributeString("ContentType", tipo);
            w.WriteEndElement();
        }
    }

    private static void Rels(XmlWriter w)
    {
        const string ns = "http://schemas.openxmlformats.org/package/2006/relationships";
        w.WriteStartElement("Relationships", ns);
        w.WriteStartElement("Relationship", ns);
        w.WriteAttributeString("Id", "rId1");
        w.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument");
        w.WriteAttributeString("Target", "xl/workbook.xml");
        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static void Workbook(XmlWriter w, IReadOnlyList<string> nomes)
    {
        w.WriteStartElement("workbook", Ns);
        w.WriteAttributeString("xmlns", "r", null, NsRel);
        w.WriteStartElement("sheets", Ns);
        for (var i = 0; i < nomes.Count; i++)
        {
            w.WriteStartElement("sheet", Ns);
            w.WriteAttributeString("name", nomes[i]);
            w.WriteAttributeString("sheetId", (i + 1).ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("id", NsRel, $"rId{i + 1}");
            w.WriteEndElement();
        }

        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static void WorkbookRels(XmlWriter w, int folhas)
    {
        const string ns = "http://schemas.openxmlformats.org/package/2006/relationships";
        w.WriteStartElement("Relationships", ns);
        for (var i = 1; i <= folhas; i++)
        {
            w.WriteStartElement("Relationship", ns);
            w.WriteAttributeString("Id", $"rId{i}");
            w.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet");
            w.WriteAttributeString("Target", $"worksheets/sheet{i}.xml");
            w.WriteEndElement();
        }

        w.WriteStartElement("Relationship", ns);
        w.WriteAttributeString("Id", $"rId{folhas + 1}");
        w.WriteAttributeString("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles");
        w.WriteAttributeString("Target", "styles.xml");
        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static void Estilos(XmlWriter w)
    {
        w.WriteStartElement("styleSheet", Ns);
        w.WriteRaw("""
            <fonts count="3"><font><sz val="10"/><name val="Arial"/></font><font><b/><sz val="10"/><color rgb="FFFFFFFF"/><name val="Arial"/></font><font><b/><sz val="13"/><name val="Arial"/></font></fonts>
            <fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FF1F3A5F"/><bgColor indexed="64"/></patternFill></fill></fills>
            <borders count="2"><border><left/><right/><top/><bottom/><diagonal/></border><border><left/><right/><top/><bottom style="thin"><color rgb="FFDDDDDD"/></bottom><diagonal/></border></borders>
            <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
            <cellXfs count="5">
            <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1"><alignment vertical="top"/></xf>
            <xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"><alignment vertical="center" wrapText="1"/></xf>
            <xf numFmtId="14" fontId="0" fillId="0" borderId="1" xfId="0" applyNumberFormat="1" applyBorder="1"><alignment horizontal="left" vertical="top"/></xf>
            <xf numFmtId="0" fontId="2" fillId="0" borderId="0" xfId="0" applyFont="1"/>
            <xf numFmtId="0" fontId="0" fillId="0" borderId="1" xfId="0" applyBorder="1" applyAlignment="1"><alignment vertical="top" wrapText="1"/></xf>
            </cellXfs>
            <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
            """);
        w.WriteEndElement();
    }

    private static void Folha(XmlWriter w, FolhaExcel folha)
    {
        var colunas = Math.Max(1, folha.Cabecalhos.Count);
        var larguras = Enumerable.Range(0, colunas)
            .Select(c => Math.Clamp(
                Math.Max(folha.Cabecalhos.ElementAtOrDefault(c)?.Length ?? 0,
                    folha.Linhas.Select(l => Texto(l.ElementAtOrDefault(c)).Length).DefaultIfEmpty(0).Max()) + 2, 8, 60))
            .ToList();
        var ultimaColuna = Coluna(colunas - 1);
        var ultimaLinha = LinhaCabecalho + Math.Max(folha.Linhas.Count, 0);

        w.WriteStartElement("worksheet", Ns);
        w.WriteStartElement("sheetViews", Ns);
        w.WriteStartElement("sheetView", Ns);
        w.WriteAttributeString("workbookViewId", "0");
        w.WriteStartElement("pane", Ns);
        w.WriteAttributeString("ySplit", LinhaCabecalho.ToString(CultureInfo.InvariantCulture));
        w.WriteAttributeString("topLeftCell", $"A{LinhaCabecalho + 1}");
        w.WriteAttributeString("activePane", "bottomLeft");
        w.WriteAttributeString("state", "frozen");
        w.WriteEndElement();
        w.WriteEndElement();
        w.WriteEndElement();

        w.WriteStartElement("cols", Ns);
        for (var c = 0; c < colunas; c++)
        {
            w.WriteStartElement("col", Ns);
            var n = (c + 1).ToString(CultureInfo.InvariantCulture);
            w.WriteAttributeString("min", n);
            w.WriteAttributeString("max", n);
            w.WriteAttributeString("width", larguras[c].ToString(CultureInfo.InvariantCulture));
            w.WriteAttributeString("customWidth", "1");
            w.WriteEndElement();
        }

        w.WriteEndElement();

        w.WriteStartElement("sheetData", Ns);
        EscreverLinha(w, 1, [folha.Titulo], EstiloTitulo);
        EscreverLinha(w, LinhaCabecalho, folha.Cabecalhos.Cast<object?>().ToList(), EstiloCabecalho);
        for (var i = 0; i < folha.Linhas.Count; i++)
        {
            EscreverLinha(w, LinhaCabecalho + 1 + i, folha.Linhas[i], null);
        }

        w.WriteEndElement();

        w.WriteStartElement("autoFilter", Ns);
        w.WriteAttributeString("ref", $"A{LinhaCabecalho}:{ultimaColuna}{ultimaLinha}");
        w.WriteEndElement();
        w.WriteEndElement();
    }

    private static void EscreverLinha(XmlWriter w, int linha, IReadOnlyList<object?> valores, int? estilo)
    {
        w.WriteStartElement("row", Ns);
        w.WriteAttributeString("r", linha.ToString(CultureInfo.InvariantCulture));
        for (var c = 0; c < valores.Count; c++)
        {
            var valor = valores[c];
            if (valor is null)
            {
                continue;
            }

            w.WriteStartElement("c", Ns);
            w.WriteAttributeString("r", $"{Coluna(c)}{linha}");
            switch (valor)
            {
                case DateOnly data:
                    w.WriteAttributeString("s", (estilo ?? EstiloData).ToString(CultureInfo.InvariantCulture));
                    w.WriteElementString("v", Ns, (data.ToDateTime(TimeOnly.MinValue) - new DateTime(1899, 12, 30)).TotalDays.ToString(CultureInfo.InvariantCulture));
                    break;
                case DateTimeOffset dataHora:
                    w.WriteAttributeString("s", (estilo ?? EstiloData).ToString(CultureInfo.InvariantCulture));
                    w.WriteElementString("v", Ns, ((int)(dataHora.UtcDateTime.Date - new DateTime(1899, 12, 30)).TotalDays).ToString(CultureInfo.InvariantCulture));
                    break;
                case int or long or decimal or double or float:
                    if (estilo is not null)
                    {
                        w.WriteAttributeString("s", estilo.Value.ToString(CultureInfo.InvariantCulture));
                    }

                    w.WriteElementString("v", Ns, Convert.ToString(valor, CultureInfo.InvariantCulture));
                    break;
                default:
                    var texto = Texto(valor);
                    w.WriteAttributeString("t", "inlineStr");
                    w.WriteAttributeString("s", (estilo ?? (texto.Length > 60 || texto.Contains('\n') ? EstiloTexto : 0)).ToString(CultureInfo.InvariantCulture));
                    w.WriteStartElement("is", Ns);
                    w.WriteStartElement("t", Ns);
                    w.WriteAttributeString("xml", "space", null, "preserve");
                    w.WriteString(texto);
                    w.WriteEndElement();
                    w.WriteEndElement();
                    break;
            }

            w.WriteEndElement();
        }

        w.WriteEndElement();
    }

    private static string Texto(object? valor)
    {
        var texto = valor switch
        {
            null => string.Empty,
            bool b => b ? "sim" : "não",
            DateOnly d => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            DateTimeOffset d => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString() ?? string.Empty,
        };

        // Caracteres de controlo não são XML válido.
        return new string(texto.Where(c => c is '\n' or '\t' || !char.IsControl(c)).ToArray());
    }

    private static string Coluna(int indice)
    {
        var nome = string.Empty;
        for (var n = indice + 1; n > 0; n = (n - 1) / 26)
        {
            nome = (char)('A' + ((n - 1) % 26)) + nome;
        }

        return nome;
    }

    /// <summary>Nomes de folha válidos no Excel: até 31 caracteres, sem []:*?/\ e sem repetidos.</summary>
    private static List<string> NomesUnicos(IReadOnlyList<string> nomes)
    {
        var usados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var resultado = new List<string>();
        foreach (var original in nomes)
        {
            var limpo = new string(original.Where(c => !"[]:*?/\\".Contains(c)).ToArray()).Trim();
            if (limpo.Length == 0)
            {
                limpo = "Folha";
            }

            limpo = limpo.Length > 31 ? limpo[..31] : limpo;
            var nome = limpo;
            for (var i = 2; !usados.Add(nome); i++)
            {
                var sufixo = $" ({i})";
                nome = (limpo.Length + sufixo.Length > 31 ? limpo[..(31 - sufixo.Length)] : limpo) + sufixo;
            }

            resultado.Add(nome);
        }

        return resultado;
    }
}
