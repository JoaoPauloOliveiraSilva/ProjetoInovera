using Innovera.Application.Ideias.Commands;
using Innovera.Application.Notificacoes;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;

namespace Innovera.Application.FunctionalTests.Notificacoes;

public class NotificacoesTests : TestBase
{
    private static CriarIdeiaCommand NovaIdeia(params int[] coautores) => new()
    {
        Tipo = TipoIdeia.Melhoria,
        Titulo = "Boleias partilhadas",
        Descricao = "Lugares reservados para quem partilha carro.",
        CoautoresIds = coautores,
        TermosAceites = true,
    };

    [Test]
    public async Task CoautorRecebeAvisoEEmailParaConfirmar()
    {
        await TestApp.RunAsUserAsync("coautor@local", "Coautor1234!", []);
        var coautor = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        await TestApp.RunAsDefaultUserAsync();
        await TestApp.SendAsync(NovaIdeia(coautor.Id));

        EmailFalso.Enviados.ShouldHaveSingleItem().Para.ShouldBe("coautor@local");

        await TestApp.RunAsUserAsync("coautor@local", "Coautor1234!", []);
        var avisos = await TestApp.SendAsync(new ListarNotificacoesQuery());
        avisos.NaoLidas.ShouldBe(1);
        avisos.Itens.ShouldHaveSingleItem().Titulo.ShouldContain("coautoria");
        avisos.Itens[0].Link.ShouldNotBeNull().ShouldStartWith("/inovacao/ideias/");

        await TestApp.SendAsync(new MarcarNotificacoesLidasCommand(null));
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).NaoLidas.ShouldBe(0);
    }

    [Test]
    public async Task AutorEAvisadoEmCadaPassoAteADecisao()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        await TestApp.RunAsDefaultUserAsync();
        var ideia = await TestApp.SendAsync(NovaIdeia());

        await TestApp.RunAsEquipaInovacaoAsync();
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).Itens.ShouldContain(n => n.Titulo.Contains("para validar"));
        await TestApp.SendAsync(new ValidarIdeiaCommand(ideia.Id));
        await TestApp.SendAsync(new FecharDiscussaoCommand(ideia.Id));
        await TestApp.SendAsync(new RegistarAvaliacaoCommand
        {
            IdeiaId = ideia.Id, Custo = 3, Enquadramento = 3, Beneficio = 3, AdequacaoTecnica = 3, Incerteza = 3,
        });
        await TestApp.SendAsync(new DefinirClasseCommand(ideia.Id, ClasseIdeia.MelhoriaContinua, null));

        var assuntos = EmailFalso.Enviados.Where(e => e.Para == "test@local").Select(e => e.Assunto).ToList();
        assuntos.ShouldContain(a => a.Contains("está em discussão"));
        assuntos.ShouldContain(a => a.Contains("Terminou a discussão"));
        assuntos.ShouldContain(a => a.Contains("foi aprovada"));

        await TestApp.RunAsDefaultUserAsync();
        (await TestApp.SendAsync(new ListarNotificacoesQuery())).NaoLidas.ShouldBe(3);
    }
}
