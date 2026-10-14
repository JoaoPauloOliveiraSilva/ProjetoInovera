using System.Text.RegularExpressions;
using Innovera.Application.Administracao;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Common.Interfaces;
using Innovera.Application.Contas;
using Innovera.Application.Ideias.Commands;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Constants;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Innovera.Application.FunctionalTests.Administracao;

public class AdministracaoTests : TestBase
{
    private static AdicionarUtilizadorCommand NovoUtilizador(string email = "rita@local", params string[] perfis)
        => new("Rita Gomes", email, "dstelecom, s.a.", perfis.Length == 0 ? [Roles.Trabalhador] : perfis);

    /// <summary>Tira do email o id da conta e o token do link "definir password".</summary>
    private static (string UtilizadorId, string Token) LinkDoEmail(MensagemEmail email)
    {
        var m = Regex.Match(email.Texto, @"[?&]u=([^&\s]+)&t=([^\s&]+)");
        m.Success.ShouldBeTrue("O email não tem o link esperado.");
        return (Uri.UnescapeDataString(m.Groups[1].Value), m.Groups[2].Value);
    }

    private static async Task<UtilizadorAdminDto> UtilizadorNaLista(string email)
        => (await TestApp.SendAsync(new ListarUtilizadoresQuery(null))).Single(u => u.Email == email);

    [Test]
    public async Task AdministradorAdicionaUtilizadorQueDefineAPasswordPeloConvite()
    {
        await TestApp.RunAsAdministratorAsync();

        await TestApp.SendAsync(NovoUtilizador());

        var convite = EmailFalso.Enviados.ShouldHaveSingleItem();
        convite.Para.ShouldBe("rita@local");
        (await UtilizadorNaLista("rita@local")).Estado.ShouldBe(EstadoConta.ConvitePendente);

        var (id, token) = LinkDoEmail(convite);
        var email = await TestApp.SendAsync(new AceitarConviteCommand(id, token, "Segura2026x"));

        email.ShouldBe("rita@local");
        var lista = await UtilizadorNaLista("rita@local");
        lista.Estado.ShouldBe(EstadoConta.Ativa);
        lista.Perfis.ShouldBe(new[] { Roles.Trabalhador });

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var conta = await userManager.FindByIdAsync(id);
        (await userManager.CheckPasswordAsync(conta!, "Segura2026x")).ShouldBeTrue();

        // O convite só se usa uma vez.
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new AceitarConviteCommand(id, token, "Outra2026xx")));
    }

    [Test]
    public async Task PasswordRecusadaNaoGastaOConvite()
    {
        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(NovoUtilizador());
        var (id, token) = LinkDoEmail(EmailFalso.Enviados.Single());

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new AceitarConviteCommand(id, token, "curta")));
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new AceitarConviteCommand(id, token, "semmaiusculas1")));

        (await TestApp.SendAsync(new AceitarConviteCommand(id, token, "Segura2026x"))).ShouldBe("rita@local");
    }

    [Test]
    public async Task TrabalhadorNaoGereUtilizadores()
    {
        await TestApp.RunAsDefaultUserAsync();

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ListarUtilizadoresQuery(null)));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(NovoUtilizador()));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new CriarPerfilCommand("Auditor")));
    }

    [Test]
    public async Task EquipaDeInovacaoValidaIdeiasMasNaoGereUtilizadores()
    {
        await TestApp.RunAsDefaultUserAsync();
        var ideia = await TestApp.SendAsync(new CriarIdeiaCommand
        {
            Tipo = TipoIdeia.Melhoria,
            Titulo = "Academia SAP",
            Descricao = "Formação interna em SAP.",
            TermosAceites = true,
        });

        await TestApp.RunAsEquipaInovacaoAsync();
        await TestApp.SendAsync(new ValidarIdeiaCommand(ideia.Id));

        (await TestApp.FindAsync<Ideia>(ideia.Id))!.Estado.ShouldBe(EstadoIdeia.EmDiscussao);
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ListarUtilizadoresQuery(null)));
    }

    [Test]
    public async Task UmPerfilNovoGanhaPermissoesSemMexerNoCodigo()
    {
        await TestApp.RunAsDefaultUserAsync();
        var ideia = await TestApp.SendAsync(new CriarIdeiaCommand
        {
            Tipo = TipoIdeia.Melhoria,
            Titulo = "Boleias partilhadas",
            Descricao = "Lugares reservados para quem partilha carro.",
            TermosAceites = true,
        });

        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(new CriarPerfilCommand("Avaliador externo"));
        await TestApp.SendAsync(new DefinirPermissoesCommand("Avaliador externo", [Permissoes.Ideias.Validar]));

        var perfis = await TestApp.SendAsync(new ListarPerfisQuery());
        perfis.Perfis.Single(p => p.Nome == "Avaliador externo").Permissoes.ShouldBe(new[] { Permissoes.Ideias.Validar });
        perfis.Perfis.Single(p => p.Nome == Roles.Administrador).TodasAsPermissoes.ShouldBeTrue();

        await TestApp.RunAsUserAsync("externo@local", "Externo1234!", ["Avaliador externo"]);
        await TestApp.SendAsync(new ValidarIdeiaCommand(ideia.Id));
        (await TestApp.FindAsync<Ideia>(ideia.Id))!.Estado.ShouldBe(EstadoIdeia.EmDiscussao);

        await TestApp.RunAsAdministratorAsync();
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new ApagarPerfilCommand("Avaliador externo")));
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new ApagarPerfilCommand(Roles.Trabalhador)));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new DefinirPermissoesCommand(Roles.Administrador, [])));
    }

    [Test]
    public async Task AdministradorNaoSeDesativaNemTiraOProprioPerfil()
    {
        await TestApp.RunAsAdministratorAsync();
        var eu = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        eu.Permissoes.ShouldContain(Permissoes.Administracao.GerirPerfis);
        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new AlterarEstadoUtilizadorCommand(eu.Id, false)));
        await Should.ThrowAsync<RegraDeNegocioException>(() =>
            TestApp.SendAsync(new EditarUtilizadorCommand(eu.Id, eu.Nome, null, [Roles.Trabalhador])));
    }

    [Test]
    public async Task ContaDesativadaNaoRecebeLinkDeRecuperacao()
    {
        await TestApp.RunAsAdministratorAsync();
        var id = await TestApp.SendAsync(NovoUtilizador());
        var (identityId, token) = LinkDoEmail(EmailFalso.Enviados.Single());
        await TestApp.SendAsync(new AceitarConviteCommand(identityId, token, "Segura2026x"));
        EmailFalso.Enviados.Clear();

        await TestApp.SendAsync(new RecuperarPasswordCommand("rita@local"));
        EmailFalso.Enviados.ShouldHaveSingleItem().Assunto.ShouldBe("Recuperar a password");

        await TestApp.SendAsync(new AlterarEstadoUtilizadorCommand(id, false));
        (await UtilizadorNaLista("rita@local")).Estado.ShouldBe(EstadoConta.Desativada);
        (await TestApp.FindAsync<Utilizador>(id))!.Ativo.ShouldBeFalse();

        EmailFalso.Enviados.Clear();
        await TestApp.SendAsync(new RecuperarPasswordCommand("rita@local"));
        EmailFalso.Enviados.ShouldBeEmpty();
    }

    [Test]
    public async Task CadaPessoaTemUmSoPerfil()
    {
        await TestApp.RunAsAdministratorAsync();
        await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(NovoUtilizador("dois@local", Roles.EquipaInovacao, Roles.Trabalhador)));

        var id = await TestApp.SendAsync(NovoUtilizador("um@local", Roles.Trabalhador));
        await Should.ThrowAsync<ValidationException>(() =>
            TestApp.SendAsync(new EditarUtilizadorCommand(id, "Rita Gomes", null, [Roles.Administrador, Roles.EquipaInovacao])));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new EditarUtilizadorCommand(id, "Rita Gomes", null, [])));

        await TestApp.SendAsync(new EditarUtilizadorCommand(id, "Rita Gomes", null, [Roles.EquipaInovacao]));
        var rita = (await TestApp.SendAsync(new ListarUtilizadoresQuery(null))).Single(u => u.Id == id);
        rita.Perfis.ShouldBe(new[] { Roles.EquipaInovacao });
    }
}
