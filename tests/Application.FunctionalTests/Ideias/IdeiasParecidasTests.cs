using Innovera.Application.Ideias.Commands;
using Innovera.Application.Ideias.Queries;
using Innovera.Domain.Enums;

namespace Innovera.Application.FunctionalTests.Ideias;

/// <summary>Aviso de ideia parecida ao submeter (para não haver ideias repetidas).</summary>
public class IdeiasParecidasTests : TestBase
{
    private static CriarIdeiaCommand Ideia(string titulo, string descricao = "Descrição da ideia.") => new()
    {
        Tipo = TipoIdeia.Melhoria,
        Titulo = titulo,
        Descricao = descricao,
        TermosAceites = true,
    };

    [Test]
    public async Task EncontraAsIdeiasComTituloParecidoMesmoComPluraisEAcentos()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var postes = await TestApp.SendAsync(Ideia("Inspeção de postes com drones"));
        await TestApp.SendAsync(Ideia("Academia SAP para novos colaboradores"));
        await TestApp.SendAsync(Ideia("Bebedouros nas obras"));

        var parecidas = await TestApp.SendAsync(new ProcurarIdeiasParecidasQuery("Drone para inspecao de poste"));

        var encontrada = parecidas.ShouldHaveSingleItem();
        encontrada.Id.ShouldBe(postes.Id);
        encontrada.Semelhanca.ShouldBeGreaterThanOrEqualTo(80);

        (await TestApp.SendAsync(new ProcurarIdeiasParecidasQuery("Formação em segurança"))).ShouldBeEmpty();
        (await TestApp.SendAsync(new ProcurarIdeiasParecidasQuery("drones"))).ShouldBeEmpty();
    }

    [Test]
    public async Task UmTrabalhadorSoVeAsIdeiasQueJaPodiaVer()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(Ideia("Inspeção de postes com drones"));

        // A ideia ainda está à espera da validação da equipa: um trabalhador que não é autor não a vê.
        await TestApp.RunAsDefaultUserAsync();
        (await TestApp.SendAsync(new ProcurarIdeiasParecidasQuery("Inspeção de postes com drones"))).ShouldBeEmpty();
    }

    [Test]
    public void PalavrasSemAcentosPluraisNemPalavrasVazias()
    {
        SemelhancaTexto.Palavras("Inspeções dos Postes com Drones").ShouldBe(new[] { "inspecao", "poste", "drone" }, ignoreOrder: true);
        SemelhancaTexto.Dice(SemelhancaTexto.Palavras("a b c"), SemelhancaTexto.Palavras("")).ShouldBe(0);
    }
}
