using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Domain.Enums;

namespace Innovera.Application.FunctionalTests.Kpis;

/// <summary>Evolução anual de um KPI (gráfico na página de cada KPI).</summary>
public class EvolucaoKpiTests : TestBase
{
    [Test]
    public async Task UmPontoPorAnoComOObjetivoEOAnoAtualParcial()
    {
        var ano = DateTime.UtcNow.Year;
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa
            { Titulo = "A", Estado = EstadoIniciativa.EmCurso, DataInicio = new DateOnly(ano - 1, 3, 1) }));
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa
            { Titulo = "B", Estado = EstadoIniciativa.EmCurso, DataInicio = new DateOnly(ano - 1, 9, 1) }));
        await TestApp.SendAsync(new GuardarObjetivoKpiCommand(13, ano - 1, "≥ 3"));

        var evolucao = await TestApp.SendAsync(new ObterEvolucaoKpiQuery(13));

        evolucao.Pontos.Select(p => p.Ano).ShouldBe(Enumerable.Range(ano - 3, 4));
        var passado = evolucao.Pontos.Single(p => p.Ano == ano - 1);
        passado.Valor.ShouldBe(2m);
        passado.Objetivo.ShouldBe("≥ 3");
        passado.Atingido.ShouldBe(false);
        passado.Parcial.ShouldBeFalse();
        passado.Oficial.ShouldBeFalse();
        var atual = evolucao.Pontos.Single(p => p.Ano == ano);
        atual.Parcial.ShouldBeTrue();
        atual.Atingido.ShouldBeNull();
    }

    [Test]
    public async Task OsKpisDoOrcamentoSoAparecemAQuemPodeVerOOrcamento()
    {
        await TestApp.RunAsDefaultUserAsync();
        var evolucao = await TestApp.SendAsync(new ObterEvolucaoKpiQuery(38));
        evolucao.Pontos.ShouldAllBe(p => p.Valor == null);
        evolucao.Nota.ShouldNotBeNull();
    }
}
