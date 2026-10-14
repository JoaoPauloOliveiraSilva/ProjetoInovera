using Innovera.Application.Charters;
using Innovera.Application.Iniciativas;
using Innovera.Application.Notificacoes;
using Innovera.Application.Registos;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;

namespace Innovera.Application.FunctionalTests.Charters;

/// <summary>Avisos (sino + email) das tarefas e entregas do Project Charter a chegar ao prazo ou atrasadas.</summary>
public class PrazosCharterTests : TestBase
{
    private static DateOnly Hoje => DateOnly.FromDateTime(DateTime.UtcNow);

    private static TarefaDados Tarefa(string codigo, string nome, DateOnly fim, string? responsavel = null,
        EstadoTrabalho estado = EstadoTrabalho.EmCurso)
        => new(codigo, nome, TipoTarefa.Tecnica, estado, null, fim, null, null, responsavel, null);

    [Test]
    public async Task AvisaOResponsavelEQuemGereAsIniciativasUmaSoVez()
    {
        await TestApp.RunAsUserAsync("ricardo@local", "Ricardo1234!", []);
        var ricardo = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ObterUtilizadorAtualQuery());
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto,
            new DadosIniciativa { Titulo = "AI Poles", Estado = EstadoIniciativa.EmCurso }));
        await TestApp.SendAsync(new GuardarTarefaCommand(projeto, null, Tarefa("1.1", "Recolher imagens", Hoje.AddDays(2), ricardo.Nome)));
        await TestApp.SendAsync(new GuardarTarefaCommand(projeto, null, Tarefa("1.2", "Anotar dataset", Hoje.AddDays(-3))));
        await TestApp.SendAsync(new GuardarTarefaCommand(projeto, null, Tarefa("1.3", "Tarefa muito antiga", Hoje.AddDays(-90))));
        await TestApp.SendAsync(new GuardarTarefaCommand(projeto, null, Tarefa("1.4", "Tarefa feita", Hoje.AddDays(1), estado: EstadoTrabalho.Concluido)));
        await TestApp.SendAsync(new GuardarEntregaCommand(projeto, null, new EntregaDados(TipoEntrega.Milestone, "M1", "Modelo validado",
            Hoje.AddDays(5), EstadoTrabalho.Planeado, null, null, null, null)));

        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBeGreaterThanOrEqualTo(3);
        (await TestApp.SendAsync(new GerarAlertasCommand())).ShouldBe(0);

        var daEquipa = (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.Select(n => n.Titulo).ToList();
        daEquipa.ShouldContain("Atrasado: 1.2 Anotar dataset");
        daEquipa.ShouldContain("Prazo: M1 Modelo validado");
        daEquipa.ShouldNotContain(t => t.Contains("muito antiga") || t.Contains("feita"));

        await TestApp.RunAsUserAsync("ricardo@local", "Ricardo1234!", []);
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.Select(n => n.Titulo).ShouldBe(new[] { "Prazo: 1.1 Recolher imagens" });
    }
}
