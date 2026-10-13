using Innovera.Application.Importacoes;

namespace Innovera.Application.FunctionalTests.Importacoes;

/// <summary>
/// Importa os Excel reais da dstelecom quando a variável INNOVERA_MODELOS_DST aponta para a pasta deles (só no PC de quem os tem:
/// os ficheiros nunca vão para o repositório). Sem a variável o teste é ignorado.
/// </summary>
public class ImportarModelosReaisTests : TestBase
{
    // O Mod.246 primeiro: as lições (Mod.252) e a estratégia (Mod.52) ligam-se às iniciativas.
    private static readonly string[] Ordem = ["Mod.246", "Mod.235", "Mod.236", "Mod.237", "Mod.238", "Mod.239", "Mod.241", "Mod.252", "Mod.52.", "Mod.69"];

    [Test]
    public async Task ImportaOsModelosReaisDaDst()
    {
        var pasta = Environment.GetEnvironmentVariable("INNOVERA_MODELOS_DST");
        if (string.IsNullOrWhiteSpace(pasta) || !Directory.Exists(pasta))
        {
            Assert.Ignore("Sem INNOVERA_MODELOS_DST: os modelos reais da dst não estão neste PC.");
        }

        await TestApp.RunAsAdministratorAsync();
        var ficheiros = Directory.GetFiles(pasta, "*.xlsx");
        foreach (var prefixo in Ordem)
        {
            var caminho = ficheiros.FirstOrDefault(f => Path.GetFileName(f).StartsWith(prefixo, StringComparison.OrdinalIgnoreCase));
            if (caminho is null)
            {
                TestContext.Out.WriteLine($"{prefixo}: ficheiro não encontrado.");
                continue;
            }

            await using var conteudo = File.OpenRead(caminho);
            var resultado = await TestApp.SendAsync(new ImportarModeloCommand(conteudo));
            TestContext.Out.WriteLine($"== {Path.GetFileName(caminho)} → {resultado.Modelo}");
            foreach (var f in resultado.Folhas)
            {
                TestContext.Out.WriteLine($"   {f.Folha}: {f.Importadas} importadas, {f.JaExistiam} já existiam, {f.Ignoradas} ignoradas");
            }

            foreach (var aviso in resultado.Avisos)
            {
                TestContext.Out.WriteLine($"   aviso: {aviso}");
            }

            resultado.Modelo.ShouldStartWith(prefixo.TrimEnd('.'));
            resultado.Folhas.Sum(f => f.Importadas).ShouldBeGreaterThan(0, $"{prefixo} não importou nada.");
        }
    }
}
