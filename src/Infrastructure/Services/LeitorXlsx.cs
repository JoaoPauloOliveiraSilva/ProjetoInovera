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
                folhas.Add(new FolhaLida(nome, Linhas(xml, partilhados)));
            }
        }

        return folhas;
    }

    private static List<object?[]> Linhas(XDocument folha, IReadOnlyList<string> partilhados)
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
