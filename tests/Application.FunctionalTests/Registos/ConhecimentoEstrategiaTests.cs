using Innovera.Application.Charters;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;
using Innovera.Application.Notificacoes;
using Innovera.Application.Registos;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;

namespace Innovera.Application.FunctionalTests.Registos;

public class ConhecimentoEstrategiaTests : TestBase
{
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);

    [Test]
    public async Task ResultadoDaVigilanciaEntraNoConhecimentoCodificadoEPlaneiaASeguinte()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var vig = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Vigilancia, new DadosIniciativa
        {
            Titulo = "Fibra ótica",
            Campos = new CamposTipoDto(Periodicidade: Periodicidade.Mensal),
        }));

        var prevista = new DateOnly(2026, 3, 1);
        var id = await TestApp.SendAsync(new GuardarExecucaoCommand(vig, null, new ExecucaoDados(prevista, null, null, null)));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(
            new GuardarExecucaoCommand(vig, id, new ExecucaoDados(prevista, null, "Sem data", null))));

        await TestApp.SendAsync(new GuardarExecucaoCommand(vig, id,
            new ExecucaoDados(prevista, prevista, "Novos cabos de 6912 fibras", "https://dstelecom.sharepoint.com/vigilancia.pdf")));

        var execucoes = await TestApp.SendAsync(new ListarExecucoesQuery(vig));
        execucoes.Count.ShouldBe(2);
        var feita = execucoes.Single(e => e.Id == id);
        feita.NoPrazo.ShouldBeTrue();
        feita.ConhecimentoId.ShouldNotBeNull();
        execucoes.Single(e => e.Id != id).DataPrevista.ShouldBe(new DateOnly(2026, 4, 1));

        var entrada = (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(OrigemConhecimento.Vigilancia, null))).ShouldHaveSingleItem();
        entrada.Descricao.ShouldBe("Novos cabos de 6912 fibras");
        entrada.IniciativaId.ShouldBe(vig);

        // Corrigir o resultado atualiza a mesma entrada e não planeia outra execução.
        await TestApp.SendAsync(new GuardarExecucaoCommand(vig, id, new ExecucaoDados(prevista, prevista, "Cabos de 6912 fibras (2 fabricantes)", null)));
        (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(OrigemConhecimento.Vigilancia, null)))
            .ShouldHaveSingleItem().Descricao.ShouldBe("Cabos de 6912 fibras (2 fabricantes)");
        (await TestApp.SendAsync(new ListarExecucoesQuery(vig))).Count.ShouldBe(2);

        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new ApagarExecucaoCommand(vig, id)));
        await TestApp.RunAsAdministratorAsync();
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new ApagarConhecimentoCodificadoCommand(entrada.Id)));

        // Só as vigilâncias têm execuções.
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "AI Poles" }));
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(
            new GuardarExecucaoCommand(projeto, null, new ExecucaoDados(prevista, null, null, null))));
    }

    [Test]
    public async Task ParceirosLigadosAsIniciativas()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var uminho = await TestApp.SendAsync(new GuardarParceiroCommand(null, new ParceiroDados("Universidade do Minho", null, TipoParceiro.Academico, null)));
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "AI Poles" }));

        await TestApp.SendAsync(new DefinirParceirosIniciativaCommand(projeto, [uminho, uminho]));
        (await TestApp.SendAsync(new ObterIniciativaQuery(projeto))).Parceiros.ShouldHaveSingleItem().Nome.ShouldBe("Universidade do Minho");
        (await TestApp.SendAsync(new ListarParceirosQuery())).ShouldHaveSingleItem().Iniciativas.ShouldHaveSingleItem().Titulo.ShouldBe("AI Poles");

        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new DefinirParceirosIniciativaCommand(projeto, [999])));
        await TestApp.SendAsync(new DefinirParceirosIniciativaCommand(projeto, []));
        (await TestApp.SendAsync(new ObterIniciativaQuery(projeto))).Parceiros.ShouldBeEmpty();

        await TestApp.RunAsDefaultUserAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new DefinirParceirosIniciativaCommand(projeto, [uminho])));
    }

    [Test]
    public async Task ConhecimentoManualComAtividadesDePartilhaEAvisos()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ObterUtilizadorAtualQuery());
        var id = await TestApp.SendAsync(new GuardarConhecimentoCodificadoCommand(null, new ConhecimentoCodificadoDados(null,
            "Estudo de definição do desafio de postes", "https://dstelecom.sharepoint.com/estudo.pdf", "OneDrive dstelecom inov",
            "Requisitos dos sensores", null, null, null, [new AtividadeGestaoDto("Partilha com a CE", "Sofia", Hoje.AddDays(3), false)])));

        var entrada = (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(OrigemConhecimento.Manual, null))).ShouldHaveSingleItem();
        entrada.Atividades.ShouldNotBeNull().ShouldHaveSingleItem().Responsavel.ShouldBe("Sofia");

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarConhecimentoCodificadoCommand(id,
            new ConhecimentoCodificadoDados(null, "Estudo", null, null, null, null, null, null, [new AtividadeGestaoDto(" ", null, null, false)]))));

        // Ativos e conhecimento tácito também têm Atividades · Quem · Quando.
        await TestApp.SendAsync(new GuardarAtivoCommand(null, new AtivoDados(null, TipoAtivoIntangivel.Patente, "INPI", "Sensor", null, null,
            null, null, null, null, null, null, null, null, null, [new AtividadeGestaoDto("Converter a patente provisória", "Jurídico", Hoje.AddDays(200), false)])));
        (await TestApp.SendAsync(new ListarAtivosQuery())).ShouldHaveSingleItem().Atividades.ShouldHaveSingleItem();
        await TestApp.SendAsync(new GuardarConhecimentoTacitoCommand(null, new ConhecimentoTacitoDados(null, "dstelecom", "Ana", "Fusão de fibra",
            NivelConfidencialidade.Baixo, [new AtividadeGestaoDto("Vídeo de formação", "Ana", null, true)])));
        (await TestApp.SendAsync(new ListarConhecimentoTacitoQuery())).ShouldHaveSingleItem().Atividades.ShouldHaveSingleItem().Concluida.ShouldBeTrue();

        // Só a atividade dos próximos 7 dias avisa, e uma só vez.
        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBeGreaterThanOrEqualTo(1);
        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBe(0);
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.ShouldContain(n => n.Titulo.Contains("Partilha com a CE"));

        await TestApp.SendAsync(new ApagarConhecimentoCodificadoCommand(id));
        (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(null, null))).ShouldBeEmpty();
    }

    [Test]
    public async Task EstrategiaComKpisECopiaParaOAnoSeguinte()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "AI Poles" }));
        var dados = new LinhaEstrategiaDados(2025, "Automatização", "Postes inteligentes", "Desenvolver o projeto AI Poles", [36, 14, 14], 10, 3, 50m,
            "Consultoria INESC - 30k€", Horizonte.H2, projeto);
        await TestApp.SendAsync(new GuardarLinhaEstrategiaCommand(null, dados));

        var linha = (await TestApp.SendAsync(new ListarEstrategiaQuery(2025))).ShouldHaveSingleItem();
        linha.Kpis.Select(k => k.Numero).ToList().ShouldBe(new List<int> { 14, 36 });
        linha.Kpis[0].Nome.ShouldBe("Projetos em curso");
        linha.DedicacaoTotalHoras.ShouldBe(1500m);
        linha.IniciativaTitulo.ShouldBe("AI Poles");

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarLinhaEstrategiaCommand(null, dados with { Kpis = [42] })));

        (await TestApp.SendAsync(new CopiarEstrategiaCommand(2025, 2026))).ShouldBe(1);
        (await TestApp.SendAsync(new ListarEstrategiaQuery(2026))).ShouldHaveSingleItem().AreaAtuacao.ShouldBe("Automatização");
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new CopiarEstrategiaCommand(2025, 2026)));

        // Ferramentas: uma análise por ano.
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarFerramentaCommand(null,
            new FerramentaDados(null, "PDCA", null, null, null, null, [new AnaliseAnualDto(2025, "a"), new AnaliseAnualDto(2025, "b")]))));
        await TestApp.SendAsync(new GuardarFerramentaCommand(null,
            new FerramentaDados(null, "PDCA", null, null, null, null, [new AnaliseAnualDto(2024, "Pouco usado"), new AnaliseAnualDto(2025, "Usado em 4 melhorias")])));
        (await TestApp.SendAsync(new ListarFerramentasQuery())).ShouldHaveSingleItem().Analises.First().Ano.ShouldBe(2025);

        await TestApp.RunAsDefaultUserAsync();
        (await TestApp.SendAsync(new ListarEstrategiaQuery(2025))).ShouldHaveSingleItem();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GuardarLinhaEstrategiaCommand(null, dados)));
    }
}
