using Innovera.Application.Common.Exceptions;
using Innovera.Application.Ideias.Commands;
using Innovera.Application.Ideias.Queries;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;

namespace Innovera.Application.FunctionalTests.Ideias;

public class FluxoIdeiaTests : TestBase
{
    private static CriarIdeiaCommand NovaIdeia(string titulo = "Informação de Recrutamento") => new()
    {
        Tipo = TipoIdeia.Melhoria,
        Titulo = titulo,
        Descricao = "Divulgar as vagas de emprego do grupo de outra forma.",
        TermosAceites = true,
    };

    [Test]
    public async Task DeveCriarIdeiaComCodigoEAguardarValidacaoDaEquipa()
    {
        await TestApp.RunAsDefaultUserAsync();

        var primeira = await TestApp.SendAsync(NovaIdeia());
        var segunda = await TestApp.SendAsync(NovaIdeia("Academia SAP"));

        primeira.Codigo.ShouldMatch("^[A-Z]{2}[0-9]{2}$");
        segunda.Codigo.ShouldNotBe(primeira.Codigo);
        primeira.Estado.ShouldBe(EstadoIdeia.ValidacaoEquipa);
    }

    [Test]
    public async Task DeveExigirOsTermosECondicoes()
    {
        await TestApp.RunAsDefaultUserAsync();

        var comando = NovaIdeia() with { TermosAceites = false };

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(comando));
    }

    [Test]
    public async Task SoAEquipaDeInovacaoPodeValidar()
    {
        await TestApp.RunAsDefaultUserAsync();
        var ideia = await TestApp.SendAsync(NovaIdeia());

        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ValidarIdeiaCommand(ideia.Id)));
    }

    [Test]
    public async Task FluxoCompletoAteProjeto()
    {
        // Um trabalhador submete a ideia
        await TestApp.RunAsDefaultUserAsync();
        var criada = await TestApp.SendAsync(NovaIdeia());

        // A equipa valida e abre a discussão
        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(new ValidarIdeiaCommand(criada.Id));

        // Likes e comentários durante a discussão
        (await TestApp.SendAsync(new AlternarGostoIdeiaCommand(criada.Id))).ShouldBeTrue();
        await TestApp.SendAsync(new ComentarIdeiaCommand(criada.Id, "Excelente ideia!"));

        var emDiscussao = await TestApp.SendAsync(new ObterIdeiaQuery(criada.Id));
        emDiscussao.Estado.ShouldBe(EstadoIdeia.EmDiscussao);
        emDiscussao.Likes.ShouldBe(1);
        emDiscussao.Comentarios.Count.ShouldBe(1);

        // Fim da discussão, avaliação (nota 2,15) e classe Projeto
        await TestApp.SendAsync(new FecharDiscussaoCommand(criada.Id));
        var nota = await TestApp.SendAsync(new RegistarAvaliacaoCommand
        {
            IdeiaId = criada.Id, Custo = 4, Enquadramento = 3, Beneficio = 2, AdequacaoTecnica = 1, Incerteza = 0,
        });
        nota.Nota.ShouldBe(2.15m);
        nota.Estado.ShouldBe(EstadoIdeia.Aprovada);

        var iniciativaId = await TestApp.SendAsync(new DefinirClasseCommand(criada.Id, ClasseIdeia.Projeto, null));

        iniciativaId.ShouldNotBeNull();
        var projeto = await TestApp.FindAsync<Projeto>(iniciativaId.Value);
        projeto.ShouldNotBeNull();
        projeto.IdeiaOrigemId.ShouldBe(criada.Id);
        projeto.Codigo.ShouldBe(criada.Codigo);
    }

    [Test]
    public async Task IdeiaPorValidarNaoApareceAosOutrosTrabalhadores()
    {
        await TestApp.RunAsUserAsync("autor@local", "Testing1234!", []);
        await TestApp.SendAsync(NovaIdeia());

        await TestApp.RunAsUserAsync("colega@local", "Testing1234!", []);
        var visiveis = await TestApp.SendAsync(new ListarIdeiasQuery());

        visiveis.ShouldBeEmpty();
    }
}
