using System.Globalization;
using System.IO.Compression;
using System.Xml.Linq;

namespace Innovera.Application.FunctionalTests.Infrastructure;

/// <summary>Cria um .xlsx mínimo em memória (texto em inlineStr, números e datas como números) para testar importações.</summary>
public static class XlsxDeTeste
{
    private static readonly XNamespace Ss = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly XNamespace R = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    private static readonly XNamespace Pr = "http://schemas.openxmlformats.org/package/2006/relationships";
    private static readonly XNamespace Ct = "http://schemas.openxmlformats.org/package/2006/content-types";

    public static MemoryStream Criar(params (string Nome, object?[][] Linhas)[] folhas)
    {
        var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            Escrever(zip, "[Content_Types].xml", new XElement(Ct + "Types",
                new XElement(Ct + "Default", new XAttribute("Extension", "rels"), new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                new XElement(Ct + "Default", new XAttribute("Extension", "xml"), new XAttribute("ContentType", "application/xml")),
                new XElement(Ct + "Override", new XAttribute("PartName", "/xl/workbook.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                folhas.Select((_, i) => new XElement(Ct + "Override", new XAttribute("PartName", $"/xl/worksheets/sheet{i + 1}.xml"),
                    new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")))));

            Escrever(zip, "_rels/.rels", new XElement(Pr + "Relationships",
                new XElement(Pr + "Relationship", new XAttribute("Id", "rId1"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                    new XAttribute("Target", "xl/workbook.xml"))));

            Escrever(zip, "xl/workbook.xml", new XElement(Ss + "workbook", new XAttribute(XNamespace.Xmlns + "r", R),
                new XElement(Ss + "sheets", folhas.Select((f, i) => new XElement(Ss + "sheet",
                    new XAttribute("name", f.Nome), new XAttribute("sheetId", i + 1), new XAttribute(R + "id", $"rId{i + 1}"))))));

            Escrever(zip, "xl/_rels/workbook.xml.rels", new XElement(Pr + "Relationships",
                folhas.Select((_, i) => new XElement(Pr + "Relationship", new XAttribute("Id", $"rId{i + 1}"),
                    new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                    new XAttribute("Target", $"worksheets/sheet{i + 1}.xml")))));

            for (var i = 0; i < folhas.Length; i++)
            {
                Escrever(zip, $"xl/worksheets/sheet{i + 1}.xml", new XElement(Ss + "worksheet",
                    new XElement(Ss + "sheetData", folhas[i].Linhas.Select((linha, l) => new XElement(Ss + "row",
                        new XAttribute("r", l + 1),
                        linha.Select((valor, c) => Celula(valor, $"{(char)('A' + c)}{l + 1}")).Where(x => x is not null))))));
            }
        }

        memoria.Position = 0;
        return memoria;
    }

    private static XElement? Celula(object? valor, string referencia) => valor switch
    {
        null => null,
        string s => new XElement(Ss + "c", new XAttribute("r", referencia), new XAttribute("t", "inlineStr"),
            new XElement(Ss + "is", new XElement(Ss + "t", s))),
        DateTime d => new XElement(Ss + "c", new XAttribute("r", referencia), new XElement(Ss + "v", d.ToOADate().ToString(CultureInfo.InvariantCulture))),
        IConvertible n => new XElement(Ss + "c", new XAttribute("r", referencia),
            new XElement(Ss + "v", Convert.ToDouble(n, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture))),
        _ => null,
    };

    private static void Escrever(ZipArchive zip, string caminho, XElement raiz)
    {
        var entrada = zip.CreateEntry(caminho);
        using var stream = entrada.Open();
        new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), raiz).Save(stream);
    }
}
