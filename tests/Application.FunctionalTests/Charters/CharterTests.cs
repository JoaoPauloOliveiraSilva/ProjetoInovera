using Innovera.Application.Administracao;
using Innovera.Application.Charters;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;
using Innovera.Application.Notificacoes;
using Innovera.Application.Utilizadores;

namespace Innovera.Application.FunctionalTests.Charters;

public class CharterTests : TestBase
{
    private static Task<int> CriarProjetoAsync() =>
        TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa
        {
            Titulo = "AI Poles",
            Responsavel = "Sérgio Lopes",
            DataInicio = new DateOnly(2026, 1, 1),
        }));

    [Test]
    public async Task PreencheOsSeisSeparadores()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarProjetoAsync();

        // Ainda sem charter: vem vazio, mas editável.
        var vazio = await TestApp.SendAsync(new ObterCharterQuery(id));
        vazio.Tarefas.ShouldBeEmpty();
        vazio.PodeEditar.ShouldBeTrue();

        await TestApp.SendAsync(new GuardarCharterGeralCommand(id,
            new CharterGeralDto("Rui Dias", "Desenvolvimento", "Inovação interna / Projeto", null, null, "Aprovado pela CE", "Objetivo 1", null)));
        var m1 = await TestApp.SendAsync(new GuardarEntregaCommand(id, null,
            new EntregaDados(TipoEntrega.Milestone, "M1", "Protótipo", new DateOnly(2026, 6, 30), EstadoTrabalho.Planeado, "Relatório", null, null, null)));
        var tarefa = await TestApp.SendAsync(new GuardarTarefaCommand(id, null,
            new TarefaDados("1.1", "Levantamento", TipoTarefa.Analise, EstadoTrabalho.EmCurso, new DateOnly(2026, 1, 5), new DateOnly(2026, 2, 1),
                null, null, "Ana", m1)));
        var risco = await TestApp.SendAsync(new GuardarRiscoCommand(id, null,
            new RiscoDados(null, "Atraso do fornecedor", NivelImpacto.Alto, NivelProbabilidade.PoucoProvavel, null, null, "Ana", null, null, null)));
        await TestApp.SendAsync(new GuardarLinhaOrcamentoCommand(id, null, new LinhaOrcamentoDados("RH", null, 2026, 10000m, 8000m, null)));
        await TestApp.SendAsync(new GuardarLinhaOrcamentoCommand(id, null, new LinhaOrcamentoDados("Serviços", null, 2026, 3250m, null, null)));
        await TestApp.SendAsync(new GuardarRequisitoCommand(id, null,
            new RequisitoDados(TipoRequisitoConformidade.Legal, "RGPD", null, "CNPD", null, null, null, null, EstadoRequisito.Pendente, null)));

        var charter = await TestApp.SendAsync(new ObterCharterQuery(id));
        charter.Geral.SupervisorNome.ShouldBe("Rui Dias");
        charter.Tarefas.ShouldHaveSingleItem().MilestoneId.ShouldBe(m1);
        charter.Entregas.ShouldHaveSingleItem().Codigo.ShouldBe("M1");
        var r = charter.Riscos.ShouldHaveSingleItem();
        r.Numero.ShouldBe(1);
        r.Severidade.ShouldBe(9);
        r.FaltaPlanoMitigacao.ShouldBeTrue();
        charter.TotaisOrcamento.ShouldHaveSingleItem().Previsto.ShouldBe(13250m);
        charter.Conformidade.ShouldHaveSingleItem().Requisito.ShouldBe("RGPD");

        // Editar o risco com plano de ação tira o alerta; apagar a tarefa tira-a do charter.
        await TestApp.SendAsync(new GuardarRiscoCommand(id, risco,
            new RiscoDados(null, "Atraso do fornecedor", NivelImpacto.Alto, NivelProbabilidade.PoucoProvavel, RespostaRisco.Mitigar,
                "Segundo fornecedor", "Ana", null, null, null)));
        await TestApp.SendAsync(new ApagarItemCharterCommand(id, SeccaoCharter.Tarefa, tarefa));

        charter = await TestApp.SendAsync(new ObterCharterQuery(id));
        charter.Riscos.Single().FaltaPlanoMitigacao.ShouldBeFalse();
        charter.Tarefas.ShouldBeEmpty();
    }

    [Test]
    public async Task RelatorioDoMilestoneEntraNoConhecimentoCodificado()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarProjetoAsync();
        var m1 = await TestApp.SendAsync(new GuardarEntregaCommand(id, null,
            new EntregaDados(TipoEntrega.Milestone, "M1", "Protótipo", null, EstadoTrabalho.EmCurso, null, null, null, null)));

        await TestApp.SendAsync(new GuardarEntregaCommand(id, m1,
            new EntregaDados(TipoEntrega.Milestone, "M1", "Protótipo", null, EstadoTrabalho.EmCurso, null, null, new DateOnly(2026, 7, 1),
                "https://dstelecom.sharepoint.com/relatorio-m1")));

        var entrega = (await TestApp.SendAsync(new ObterCharterQuery(id))).Entregas.Single();
        entrega.Estado.ShouldBe(EstadoTrabalho.Concluido);

        var conhecimento = (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(null, null))).ShouldHaveSingleItem();
        conhecimento.Origem.ShouldBe(OrigemConhecimento.RelatorioIniciativa);
        conhecimento.IniciativaId.ShouldBe(id);
        conhecimento.EntregaCodigo.ShouldBe("M1");
        conhecimento.Url.ShouldBe("https://dstelecom.sharepoint.com/relatorio-m1");
        conhecimento.Data.ShouldBe(new DateOnly(2026, 7, 1));

        // Trocar o link atualiza a mesma entrada (não duplica).
        await TestApp.SendAsync(new GuardarEntregaCommand(id, m1,
            new EntregaDados(TipoEntrega.Milestone, "M1", "Protótipo", null, EstadoTrabalho.Concluido, null, null, new DateOnly(2026, 7, 1),
                "https://dstelecom.sharepoint.com/relatorio-m1-v2")));
        (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(null, null))).ShouldHaveSingleItem()
            .Url.ShouldBe("https://dstelecom.sharepoint.com/relatorio-m1-v2");
    }

    [Test]
    public async Task LicaoEntraNoRepositorioEAvisaAEquipa()
    {
        // O administrador entra uma vez (fica registado como utilizador) para poder receber o aviso.
        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(new ObterUtilizadorAtualQuery());
        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarProjetoAsync();

        await TestApp.SendAsync(new GuardarLicaoCommand(id, null,
            new LicaoDados(CategoriaLicao.Gestao, "Envolver a área de negócio desde o arranque", null, "Menos retrabalho")));

        var licao = (await TestApp.SendAsync(new ListarLicoesQuery(null, "negócio"))).ShouldHaveSingleItem();
        licao.IniciativaId.ShouldBe(id);
        licao.Categoria.ShouldBe(CategoriaLicao.Gestao);

        await TestApp.RunAsAdministratorAsync();
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens
            .ShouldContain(n => n.Titulo.StartsWith("Nova lição aprendida") && n.Link == "/inovacao/licoes");
    }

    [Test]
    public async Task SoProjetosEDesafiosTemCharterETrabalhadorNaoEdita()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var melhoria = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.MelhoriaContinua, new DadosIniciativa { Titulo = "5S" }));
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new ObterCharterQuery(melhoria)));
        var id = await CriarProjetoAsync();

        await TestApp.RunAsDefaultUserAsync();
        (await TestApp.SendAsync(new ObterCharterQuery(id))).PodeEditar.ShouldBeFalse();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GuardarLicaoCommand(id, null,
            new LicaoDados(CategoriaLicao.Tecnica, "x", null, null))));
    }

    [Test]
    public async Task LinksDoOneDriveNaFichaDaIniciativa()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarProjetoAsync();

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new AdicionarLinkCommand(id, "Proposta", "C:\\proposta.docx")));
        var link = await TestApp.SendAsync(new AdicionarLinkCommand(id, "Proposta", "https://dstelecom.sharepoint.com/proposta"));

        var detalhe = await TestApp.SendAsync(new ObterIniciativaQuery(id));
        detalhe.TemCharter.ShouldBeTrue();
        detalhe.Links.ShouldHaveSingleItem().Titulo.ShouldBe("Proposta");

        await TestApp.SendAsync(new RemoverLinkCommand(id, link));
        (await TestApp.SendAsync(new ObterIniciativaQuery(id))).Links.ShouldBeEmpty();
    }

    [Test]
    public async Task AlteracoesFicamNoRegistoDeAuditoria()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarProjetoAsync();
        await TestApp.SendAsync(new AtualizarIniciativaCommand(id, new DadosIniciativa { Titulo = "AI Poles 2", Responsavel = "Sérgio Lopes" }));

        // A equipa de inovação não vê a auditoria; o administrador vê.
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ListarAuditoriaQuery(null, null, null)));
        await TestApp.RunAsAdministratorAsync();

        var registos = await TestApp.SendAsync(new ListarAuditoriaQuery("Projeto", id.ToString(System.Globalization.CultureInfo.InvariantCulture), null));
        registos.ShouldContain(r => r.Acao == AcaoAuditoria.Criar);
        var alteracao = registos.Single(r => r.Acao == AcaoAuditoria.Alterar && r.ValoresDepois!.Contains("AI Poles 2"));
        using (var antes = System.Text.Json.JsonDocument.Parse(alteracao.ValoresAntes!))
        {
            antes.RootElement.GetProperty("Titulo").GetString().ShouldBe("AI Poles");
        }
        alteracao.Utilizador.ShouldNotBeNull();

        (await TestApp.SendAsync(new ListarAuditoriaQuery(null, null, "poles 2"))).ShouldNotBeEmpty();
    }
}
