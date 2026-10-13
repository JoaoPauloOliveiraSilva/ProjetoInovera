using Innovera.Application.Common.Interfaces;
using Innovera.Infrastructure.Services;
using NUnit.Framework;

namespace Innovera.Application.UnitTests.Common.Excel;

public class GeradorExcelTests
{
    [Test]
    public void OExcelGeradoLeSeOutraVezComOsMesmosValores()
    {
        var folhas = new[]
        {
            new FolhaExcel("Projetos", "Mod.246 · Projetos", ["Código", "Título", "Início", "Horas", "Ativo"],
            [
                ["PRJ-001", "Manutenção preditiva <OLT> & \"IA\"", new DateOnly(2026, 9, 1), 12.5m, true],
                ["PRJ-002", null, null, 3, false],
            ]),
            new FolhaExcel("Projetos", "Repetido: o nome tem de mudar", ["A"], []),
        };

        var bytes = new GeradorExcel().Gerar(folhas);
        var lidas = new LeitorXlsx().Ler(new MemoryStream(bytes));

        Assert.That(lidas.Select(f => f.Nome), Is.EqualTo(new[] { "Projetos", "Projetos (2)" }));
        var linhas = lidas[0].Linhas.Where(l => l.Any(v => v is not null)).ToList();
        Assert.That(linhas[0][0], Is.EqualTo("Mod.246 · Projetos"));
        Assert.That(linhas[1][1], Is.EqualTo("Título"));
        Assert.That(linhas[2][0], Is.EqualTo("PRJ-001"));
        Assert.That(linhas[2][1], Is.EqualTo("Manutenção preditiva <OLT> & \"IA\""));
        Assert.That(Convert.ToDecimal(linhas[2][3], System.Globalization.CultureInfo.InvariantCulture), Is.EqualTo(12.5m));
        Assert.That(linhas[3][0], Is.EqualTo("PRJ-002"));
    }
}
