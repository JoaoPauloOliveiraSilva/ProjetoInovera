using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Application.Notificacoes;
using Innovera.Application.Registos;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;

namespace Innovera.Application.FunctionalTests.Registos;

public class RegistosTests : TestBase
{
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);

    [Test]
    public async Task EquipaRegistaParceriaEAtivoETrabalhadorSoConsulta()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var uminho = await TestApp.SendAsync(new GuardarParceiroCommand(null, new ParceiroDados("Universidade do Minho", null, TipoParceiro.Academico, null)));
        await TestApp.SendAsync(new GuardarAcordoCommand(null, new AcordoDados(uminho, "Protocolo de estágios de mestrado", true,
            TipoProtocolo.Investigacao, new DateOnly(2025, 9, 1), null, true, EstadoAcordo.EmCurso, null, "https://dstelecom.sharepoint.com/protocolo")));
        await TestApp.SendAsync(new GuardarAtivoCommand(null, new AtivoDados(null, TipoAtivoIntangivel.Marca, "INPI", "Innovera", null, "123",
            null, new DateOnly(2024, 1, 10), Hoje.AddDays(30), "Nacional", "Registada", 250m, null, null, null)));
        await TestApp.SendAsync(new GuardarConhecimentoTacitoCommand(null, new ConhecimentoTacitoDados(null, "dstelecom", "Ana", "Fusão de fibra em postes",
            NivelConfidencialidade.Baixo)));
        await TestApp.SendAsync(new GuardarConhecimentoTacitoCommand(null, new ConhecimentoTacitoDados(null, "dstelecom", "Rui", "Preçário confidencial",
            NivelConfidencialidade.Elevado)));
        await TestApp.SendAsync(new GuardarFerramentaCommand(null, new FerramentaDados(null, "Design Thinking", "Método de ideação", null, 0m, null)));

        var parceiros = await TestApp.SendAsync(new ListarParceirosQuery());
        parceiros.ShouldHaveSingleItem().AcordosEmCurso.ShouldBe(1);
        (await TestApp.SendAsync(new ListarAtivosQuery())).ShouldHaveSingleItem().DiasParaValidade.ShouldBe(30);
        (await TestApp.SendAsync(new ListarConhecimentoTacitoQuery())).Count.ShouldBe(2);

        await TestApp.RunAsDefaultUserAsync();
        (await TestApp.SendAsync(new ListarAcordosQuery("minho"))).ShouldHaveSingleItem().Parceiro.ShouldBe("Universidade do Minho");
        (await TestApp.SendAsync(new ListarConhecimentoTacitoQuery())).ShouldHaveSingleItem().Detentor.ShouldBe("Ana");
        (await TestApp.SendAsync(new ListarFerramentasQuery())).ShouldHaveSingleItem();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(
            new GuardarParceiroCommand(null, new ParceiroDados("X", null, TipoParceiro.Empresarial, null))));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ApagarRegistoCommand(TipoRegisto.Parceiro, uminho)));

        // Apagar o parceiro apaga os acordos.
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ApagarRegistoCommand(TipoRegisto.Parceiro, uminho));
        (await TestApp.SendAsync(new ListarAcordosQuery(null))).ShouldBeEmpty();
    }

    [Test]
    public async Task AcordoComLinkInvalidoEhRecusado()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var p = await TestApp.SendAsync(new GuardarParceiroCommand(null, new ParceiroDados("INESC TEC", null, TipoParceiro.Academico, null)));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarAcordoCommand(null, new AcordoDados(p, "NDA", true,
            TipoProtocolo.Confidencialidade, new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), false, EstadoAcordo.EmCurso, null, "ficheiro.pdf"))));
    }

    [Test]
    public async Task PlanoAnualAvisaOsResponsaveisUmaSoVez()
    {
        await TestApp.RunAsUserAsync("ricardo@local", "Ricardo1234!", []);
        var ricardo = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await TestApp.SendAsync(new GuardarAtividadePlanoCommand(null, new AtividadePlanoDados("Avaliação de ideias com a CE", null,
            "#C8102E", Hoje.AddDays(3), null, Periodicidade.Mensal, 7, [ricardo.Id])));

        var plano = await TestApp.SendAsync(new ListarPlanoAnualQuery(Hoje.AddDays(3).Year));
        var atividade = plano.Single(a => a.Id == id);
        atividade.Responsaveis.ShouldHaveSingleItem().Nome.ShouldBe(ricardo.Nome);
        atividade.ProximaOcorrencia.ShouldBe(Hoje.AddDays(3));

        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBe(1);
        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBe(0);

        await TestApp.RunAsUserAsync("ricardo@local", "Ricardo1234!", []);
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.ShouldContain(n => n.Titulo.Contains("Avaliação de ideias com a CE"));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ApagarAtividadePlanoCommand(id)));
    }

    [Test]
    public async Task ValidadeDoAtivoAvisaQuemGereOsRegistos()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ObterUtilizadorAtualQuery());
        await TestApp.SendAsync(new GuardarAtivoCommand(null, new AtivoDados(null, TipoAtivoIntangivel.Patente, "EPO", "Sensor de fibra", null, null,
            null, null, Hoje.AddDays(60), null, null, null, null, null, null)));
        await TestApp.SendAsync(new GuardarAtivoCommand(null, new AtivoDados(null, TipoAtivoIntangivel.Marca, "INPI", "Marca antiga", null, null,
            null, null, Hoje.AddDays(400), null, null, null, null, null, null)));

        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBeGreaterThanOrEqualTo(1);
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.ShouldContain(n => n.Titulo.Contains("Sensor de fibra"));
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.ShouldNotContain(n => n.Titulo.Contains("Marca antiga"));
    }

    [Test]
    public async Task ValoresExternosSoParaOAdministradorESemRepetidos()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ListarValoresExternosQuery()));

        await TestApp.RunAsAdministratorAsync();
        var dados = new ValorExternoDados(TipoValorExterno.NumeroTrabalhadores, 2026, PeriodoKpi.Anual, 277m, null);
        var id = await TestApp.SendAsync(new GuardarValorExternoCommand(null, dados));
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new GuardarValorExternoCommand(null, dados)));
        await TestApp.SendAsync(new GuardarValorExternoCommand(id, dados with { Valor = 280m }));
        (await TestApp.SendAsync(new ListarValoresExternosQuery())).ShouldHaveSingleItem().Valor.ShouldBe(280m);
    }

    [Test]
    public async Task CamposProprioDeCadaTipo()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var mestrado = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Mestrado, new DadosIniciativa
        {
            Titulo = "Escada segura",
            Campos = new CamposTipoDto(NomeAluno: "Joana Silva", Curso: "Engenharia Mecânica"),
        }));
        var vigilancia = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Vigilancia, new DadosIniciativa
        {
            Titulo = "Fibra ótica",
            Campos = new CamposTipoDto(Descritivo: "Novos cabos", Periodicidade: Periodicidade.Trimestral),
        }));

        (await TestApp.SendAsync(new ObterIniciativaQuery(mestrado))).Campos.NomeAluno.ShouldBe("Joana Silva");
        var v = await TestApp.SendAsync(new ObterIniciativaQuery(vigilancia));
        v.Campos.Periodicidade.ShouldBe(Periodicidade.Trimestral);

        // Editar sem os campos não os apaga.
        await TestApp.SendAsync(new AtualizarIniciativaCommand(vigilancia, new DadosIniciativa { Titulo = "Fibra ótica 2" }));
        (await TestApp.SendAsync(new ObterIniciativaQuery(vigilancia))).Campos.Descritivo.ShouldBe("Novos cabos");
    }
}
