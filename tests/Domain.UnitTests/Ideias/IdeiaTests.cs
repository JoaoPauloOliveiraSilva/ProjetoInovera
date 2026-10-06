using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Domain.Events;
using Innovera.Domain.Exceptions;
using Innovera.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace Innovera.Domain.UnitTests.Ideias;

public class IdeiaTests
{
    private static readonly DateTimeOffset Hoje = new(2026, 10, 12, 9, 0, 0, TimeSpan.Zero);

    private static AvaliacaoIdeia AvaliacaoComTodas(EscalaDeAvaliacao nota) => new()
    {
        Custo = nota,
        Enquadramento = nota,
        Beneficio = nota,
        AdequacaoTecnica = nota,
        Incerteza = nota,
    };

    [Test]
    public void DiscussaoDuraTrintaDias()
    {
        var ideia = new Ideia { Titulo = "Ideia" };

        ideia.IniciarDiscussao(Hoje);

        ideia.Estado.ShouldBe(EstadoIdeia.EmDiscussao);
        ideia.FimDiscussao.ShouldBe(Hoje.AddDays(30));
        ideia.DiscussaoTerminou(Hoje.AddDays(29)).ShouldBeFalse();
        ideia.DiscussaoTerminou(Hoje.AddDays(30)).ShouldBeTrue();
    }

    [Test]
    public void SoAceitaUmLikePorPessoa()
    {
        var ideia = new Ideia();
        ideia.IniciarDiscussao(Hoje);

        ideia.AdicionarLike(7, Hoje);
        ideia.AdicionarLike(7, Hoje);
        ideia.AdicionarLike(8, Hoje);

        ideia.Likes.Count.ShouldBe(2);
    }

    [Test]
    public void NaoPodeSerAvaliadaDuranteADiscussao()
    {
        var ideia = new Ideia();
        ideia.IniciarDiscussao(Hoje);

        Should.Throw<RegraDeNegocioException>(
            () => ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel3), PesosAvaliacao.Mod246));
    }

    [Test]
    public void AvaliacaoComNotaDoisOuMaisAprovaEGeraEvento()
    {
        var ideia = new Ideia();
        ideia.IniciarDiscussao(Hoje);
        ideia.FecharDiscussao();

        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel2), PesosAvaliacao.Mod246);

        ideia.Estado.ShouldBe(EstadoIdeia.Aprovada);
        ideia.Classificacao.ShouldBe(ClassificacaoIdeia.Aprovada);
        ideia.DomainEvents.OfType<IdeiaAvaliadaEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public void AvaliacaoComNotaAbaixoDeDoisNaoAprova()
    {
        var ideia = new Ideia();
        ideia.IniciarDiscussao(Hoje);
        ideia.FecharDiscussao();

        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel1), PesosAvaliacao.Mod246);

        ideia.Estado.ShouldBe(EstadoIdeia.NaoAprovada);
        ideia.Classificacao.ShouldBe(ClassificacaoIdeia.NaoAprovada);
    }

    [Test]
    public void DestinoSoPodeSerProjetoDesafioMelhoriaOuArquivo()
    {
        var ideia = new Ideia();
        ideia.IniciarDiscussao(Hoje);
        ideia.FecharDiscussao();
        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel4), PesosAvaliacao.Mod246);

        Should.Throw<RegraDeNegocioException>(() => ideia.DefinirDestino(ClasseIdeia.IdeiaDuplicada));

        ideia.DefinirDestino(ClasseIdeia.Projeto);
        ideia.Classe.ShouldBe(ClasseIdeia.Projeto);
    }
}
