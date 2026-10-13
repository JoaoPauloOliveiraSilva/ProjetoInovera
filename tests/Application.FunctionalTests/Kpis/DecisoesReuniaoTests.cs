using System.Text;
using Innovera.Application.Charters;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Exportacoes;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;
using Innovera.Infrastructure.Services;

namespace Innovera.Application.FunctionalTests.Kpis;

/// <summary>O que a dstelecom pediu na reunião de 13/10: relatórios das vigilâncias, KPIs por semestre, objetivos e planos de ação, Excel.</summary>
public class DecisoesReuniaoTests : TestBase
{
    [Test]
    public async Task RelatorioCarregadoNumaVigilanciaEntraNoConhecimentoCodificado()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var vig = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Vigilancia, new DadosIniciativa
        {
            Titulo = "Fibra ótica",
            Estado = EstadoIniciativa.EmCurso,
            Campos = new CamposTipoDto(Periodicidade: Periodicidade.Mensal),
        }));

        var conteudo = Encoding.UTF8.GetBytes("relatório de vigilância");
        var execucao = await TestApp.SendAsync(new CarregarRelatorioVigilanciaCommand(vig, "relatorio-marco.pdf", "application/pdf", conteudo.Length,
            new MemoryStream(conteudo), "Novos cabos de alta densidade", new DateOnly(2026, 3, 10)));

        var painel = (await TestApp.SendAsync(new ListarVigilanciasQuery())).ShouldHaveSingleItem();
        painel.ProximaExecucao.ShouldBe(new DateOnly(2026, 4, 10));
        var feita = painel.Execucoes.Single(e => e.Id == execucao);
        feita.NomeFicheiro.ShouldBe("relatorio-marco.pdf");
        feita.ConhecimentoId.ShouldNotBeNull();

        var entrada = (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(OrigemConhecimento.Vigilancia, null))).ShouldHaveSingleItem();
        entrada.Descricao.ShouldBe("Novos cabos de alta densidade");
        entrada.Url.ShouldBe($"/api/Iniciativas/{vig}/execucoes/{execucao}/ficheiro");

        var ficheiro = await TestApp.SendAsync(new ObterRelatorioVigilanciaQuery(vig, execucao));
        using (var leitor = new StreamReader(ficheiro.Conteudo))
        {
            (await leitor.ReadToEndAsync()).ShouldBe("relatório de vigilância");
        }

        // Retirar a vigilância tira-a da página (o histórico fica).
        await TestApp.SendAsync(new AlterarVigilanciaAtivaCommand(vig, false));
        (await TestApp.SendAsync(new ListarVigilanciasQuery())).ShouldBeEmpty();
        (await TestApp.SendAsync(new ListarVigilanciasQuery(IncluirRetiradas: true))).ShouldHaveSingleItem().Ativa.ShouldBeFalse();

        await TestApp.RunAsDefaultUserAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new CarregarRelatorioVigilanciaCommand(vig, "x.pdf", "application/pdf",
            1, new MemoryStream([1]), null, null)));
    }

    [Test]
    public async Task KpisPorSemestreComObjetivoEPlanoDeAcao()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa
            { Titulo = "A", Estado = EstadoIniciativa.EmCurso, DataInicio = new DateOnly(2025, 3, 1) }));
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa
            { Titulo = "B", Estado = EstadoIniciativa.EmCurso, DataInicio = new DateOnly(2025, 9, 1) }));

        static decimal? Novos(IndicadoresDto d) => d.Kpis.Single(k => k.Numero == 13).Valor;
        Novos(await TestApp.SendAsync(new ObterIndicadoresQuery(2025, PeriodoKpi.PrimeiroSemestre))).ShouldBe(1m);
        Novos(await TestApp.SendAsync(new ObterIndicadoresQuery(2025, PeriodoKpi.SegundoSemestre))).ShouldBe(1m);
        Novos(await TestApp.SendAsync(new ObterIndicadoresQuery(2025))).ShouldBe(2m);

        await TestApp.SendAsync(new GuardarObjetivoKpiCommand(13, 2025, "≥ 3"));
        var eu = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());
        var acao = await TestApp.SendAsync(new GuardarAcaoKpiCommand(13, 2025, null, new AcaoKpiDados(TipoAcaoKpi.PlanoInicial, null,
            "Lançar 1 projeto por trimestre", null, "Equipa de inovação", eu.Id, new DateOnly(2025, 12, 31), null, null, null)));

        var kpi = (await TestApp.SendAsync(new ObterIndicadoresQuery(2025))).Kpis.Single(k => k.Numero == 13);
        kpi.Objetivo.ShouldBe("≥ 3");
        kpi.Atingido.ShouldBe(false);
        kpi.AcoesAbertas.ShouldBe(1);
        (await TestApp.SendAsync(new ObterIndicadoresQuery(2025, PeriodoKpi.PrimeiroSemestre))).Kpis.Single(k => k.Numero == 13).Atingido.ShouldBeNull();

        var plano = await TestApp.SendAsync(new ObterPlanoKpiQuery(13, 2025));
        plano.PodeEditar.ShouldBeTrue();
        plano.Acoes.ShouldHaveSingleItem().Responsavel.ShouldBe(eu.Nome);

        await TestApp.SendAsync(new GuardarObjetivoKpiCommand(13, 2025, "2"));
        (await TestApp.SendAsync(new ObterIndicadoresQuery(2025))).Kpis.Single(k => k.Numero == 13).Atingido.ShouldBe(true);

        await TestApp.SendAsync(new ApagarAcaoKpiCommand(13, 2025, acao));
        (await TestApp.SendAsync(new ObterPlanoKpiQuery(13, 2025))).Acoes.ShouldBeEmpty();

        await TestApp.RunAsDefaultUserAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GuardarObjetivoKpiCommand(13, 2025, "≥ 1")));
    }

    [Test]
    public void ObjetivoEscritoComoNoMod239()
    {
        ObjetivoKpi.Atingido("≥ 30", 30m).ShouldBe(true);
        ObjetivoKpi.Atingido("≥ 80%", 79.9m).ShouldBe(false);
        ObjetivoKpi.Atingido("> 1 000 000€", 1_200_000m).ShouldBe(true);
        ObjetivoKpi.Atingido("> 1.000.000 €", 900_000m).ShouldBe(false);
        ObjetivoKpi.Atingido("≤ 2,5", 2.5m).ShouldBe(true);
        ObjetivoKpi.Atingido("muitos", 10m).ShouldBeNull();
        ObjetivoKpi.Atingido("≥ 30", null).ShouldBeNull();
    }

    [Test]
    public async Task ExportaOsModelosEmExcel()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "AI Poles" }));

        var ficheiro = await TestApp.SendAsync(new ExportarModeloQuery("mod246"));
        ficheiro.NomeFicheiro.ShouldEndWith(".xlsx");
        var folhas = new LeitorXlsx().Ler(ficheiro.Conteudo);
        folhas.Select(f => f.Nome).ShouldContain("Projetos");
        folhas.Single(f => f.Nome == "Projetos").Linhas.ShouldContain(l => l.Contains("AI Poles"));

        foreach (var modelo in ModelosExportacao.Nomes.Keys)
        {
            (await TestApp.SendAsync(new ExportarModeloQuery(modelo, 2026))).Conteudo.Length.ShouldBeGreaterThan(0);
        }

        await Should.ThrowAsync<Domain.Exceptions.RegraDeNegocioException>(() => TestApp.SendAsync(new ExportarModeloQuery("mod999")));
    }
}
