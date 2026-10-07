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

    private static Ideia IdeiaSubmetida()
    {
        var ideia = new Ideia { Titulo = "Informação de Recrutamento" };
        ideia.AceitarTermos(Hoje);
        ideia.Submeter(Hoje);
        return ideia;
    }

    private static Ideia IdeiaEmDiscussao()
    {
        var ideia = IdeiaSubmetida();
        ideia.IniciarDiscussao(Hoje);
        return ideia;
    }

    private static Ideia IdeiaEmAvaliacao()
    {
        var ideia = IdeiaEmDiscussao();
        ideia.FecharDiscussao();
        return ideia;
    }

    [Test]
    public void NaoPodeSerSubmetidaSemAceitarOsTermos()
    {
        var ideia = new Ideia { Titulo = "Ideia" };

        Should.Throw<RegraDeNegocioException>(() => ideia.Submeter(Hoje));
    }

    [Test]
    public void SemCoautoresPassaLogoParaAValidacaoDaEquipa()
    {
        IdeiaSubmetida().Estado.ShouldBe(EstadoIdeia.ValidacaoEquipa);
    }

    [Test]
    public void CoautoresTemDeConfirmarAAutoria()
    {
        var ideia = new Ideia { Titulo = "Ideia" };
        ideia.Coautores.Add(new AutorIdeia { UtilizadorId = 5 });
        ideia.Coautores.Add(new AutorIdeia { UtilizadorId = 6 });
        ideia.AceitarTermos(Hoje);
        ideia.Submeter(Hoje);

        ideia.Estado.ShouldBe(EstadoIdeia.ValidacaoAutores);

        ideia.ConfirmarAutoria(5, Hoje);
        ideia.Estado.ShouldBe(EstadoIdeia.ValidacaoAutores);

        ideia.ConfirmarAutoria(6, Hoje);
        ideia.Estado.ShouldBe(EstadoIdeia.ValidacaoEquipa);
    }

    [Test]
    public void AnexoNaoPodePassarDe25MB()
    {
        var ideia = new Ideia();

        Should.Throw<RegraDeNegocioException>(
            () => ideia.AdicionarAnexo(new AnexoIdeia { NomeFicheiro = "video.mp4", TamanhoBytes = AnexoIdeia.TamanhoMaximoBytes + 1 }));
    }

    [Test]
    public void DiscussaoDuraTrintaDias()
    {
        var ideia = IdeiaEmDiscussao();

        ideia.Estado.ShouldBe(EstadoIdeia.EmDiscussao);
        ideia.FimDiscussao.ShouldBe(Hoje.AddDays(30));
        ideia.DiscussaoTerminou(Hoje.AddDays(29)).ShouldBeFalse();
        ideia.DiscussaoTerminou(Hoje.AddDays(30)).ShouldBeTrue();
    }

    [Test]
    public void FecharDiscussaoPassaParaEmAvaliacaoPeloManager()
    {
        var ideia = IdeiaEmAvaliacao();

        ideia.Estado.ShouldBe(EstadoIdeia.EmAvaliacao);
        ideia.Classificacao.ShouldBe(ClassificacaoIdeia.EmAvaliacaoPeloManager);
    }

    [Test]
    public void SoAceitaUmLikePorPessoa()
    {
        var ideia = IdeiaEmDiscussao();

        ideia.AdicionarLike(7, Hoje);
        ideia.AdicionarLike(7, Hoje);
        ideia.AdicionarLike(8, Hoje);

        ideia.Likes.Count.ShouldBe(2);
    }

    [Test]
    public void ComentarioSoAceitaUmGostoPorPessoa()
    {
        var ideia = IdeiaEmDiscussao();
        ideia.AdicionarComentario(3, "Excelente ideia", Hoje);
        var comentario = ideia.Comentarios.Single();

        comentario.AdicionarGosto(4, Hoje);
        comentario.AdicionarGosto(4, Hoje);

        comentario.Gostos.Count.ShouldBe(1);
    }

    [Test]
    public void NaoPodeSerAvaliadaDuranteADiscussao()
    {
        var ideia = IdeiaEmDiscussao();

        Should.Throw<RegraDeNegocioException>(
            () => ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel3), PesosAvaliacao.Mod246));
    }

    [Test]
    public void AvaliacaoComNotaDoisOuMaisAprovaEGeraEvento()
    {
        var ideia = IdeiaEmAvaliacao();

        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel2), PesosAvaliacao.Mod246);

        ideia.Estado.ShouldBe(EstadoIdeia.Aprovada);
        ideia.Classificacao.ShouldBe(ClassificacaoIdeia.Aprovada);
        ideia.DomainEvents.OfType<IdeiaAvaliadaEvent>().ShouldHaveSingleItem();
    }

    [Test]
    public void AvaliacaoComNotaAbaixoDeDoisNaoAprova()
    {
        var ideia = IdeiaEmAvaliacao();

        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel1), PesosAvaliacao.Mod246);

        ideia.Estado.ShouldBe(EstadoIdeia.NaoAprovada);
        ideia.Classificacao.ShouldBe(ClassificacaoIdeia.NaoAprovada);
    }

    [Test]
    public void IdeiaDstSoRegistaSeFoiAprovada()
    {
        var ideia = IdeiaEmAvaliacao();
        ideia.Responsabilidade = Responsabilidade.Dst;

        ideia.RegistarDecisaoSemNotas(true);

        ideia.Estado.ShouldBe(EstadoIdeia.Aprovada);
        ideia.Avaliacao.ShouldBeNull();
    }

    [Test]
    public void IdeiaDstelecomPrecisaDasNotas()
    {
        var ideia = IdeiaEmAvaliacao();

        Should.Throw<RegraDeNegocioException>(() => ideia.RegistarDecisaoSemNotas(true));
    }

    [Test]
    public void IdeiaAprovadaSoPodeSerMelhoriaProjetoDesafioOuArquivada()
    {
        var ideia = IdeiaEmAvaliacao();
        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel4), PesosAvaliacao.Mod246);

        Should.Throw<RegraDeNegocioException>(() => ideia.DefinirClasse(ClasseIdeia.IdeiaDuplicada));

        ideia.DefinirClasse(ClasseIdeia.Projeto);
        ideia.Classe.ShouldBe(ClasseIdeia.Projeto);
    }

    [Test]
    public void IdeiaNaoAprovadaSoPodeSerDuplicadaOuSemMerito()
    {
        var ideia = IdeiaEmAvaliacao();
        ideia.RegistarAvaliacao(AvaliacaoComTodas(EscalaDeAvaliacao.Nivel0), PesosAvaliacao.Mod246);

        Should.Throw<RegraDeNegocioException>(() => ideia.DefinirClasse(ClasseIdeia.Projeto));

        ideia.DefinirClasse(ClasseIdeia.IdeiaDuplicada);
        ideia.Classe.ShouldBe(ClasseIdeia.IdeiaDuplicada);
    }

    [Test]
    public void AtribuirNumeroDeveGerarOCodigo()
    {
        var ideia = IdeiaSubmetida();

        ideia.AtribuirNumero(100);

        ideia.Numero.ShouldBe(100);
        ideia.Codigo.ShouldBe("AB01");
    }

    [Test]
    public void DeveRetirarOLikeDuranteADiscussao()
    {
        var ideia = IdeiaEmDiscussao();
        ideia.AdicionarLike(7, Hoje);

        ideia.RemoverLike(7);

        ideia.Likes.ShouldBeEmpty();
    }

    [Test]
    public void NaoDeveRetirarLikesForaDaDiscussao()
    {
        var ideia = IdeiaSubmetida();

        Should.Throw<RegraDeNegocioException>(() => ideia.RemoverLike(7));
    }
}
