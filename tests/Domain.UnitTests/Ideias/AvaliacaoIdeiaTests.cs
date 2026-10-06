using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;
using Innovera.Domain.ValueObjects;
using NUnit.Framework;
using Shouldly;

namespace Innovera.Domain.UnitTests.Ideias;

public class AvaliacaoIdeiaTests
{
    private static AvaliacaoIdeia Avaliacao(int custo, int enquadramento, int beneficio, int adequacao, int incerteza) => new()
    {
        Custo = (EscalaDeAvaliacao)custo,
        Enquadramento = (EscalaDeAvaliacao)enquadramento,
        Beneficio = (EscalaDeAvaliacao)beneficio,
        AdequacaoTecnica = (EscalaDeAvaliacao)adequacao,
        Incerteza = (EscalaDeAvaliacao)incerteza,
    };

    [Test]
    public void DeveCalcularNotaPonderadaDoMod246()
    {
        // 0.15*4 + 0.25*3 + 0.30*2 + 0.20*1 + 0.10*0 = 2.15
        var avaliacao = Avaliacao(4, 3, 2, 1, 0);

        avaliacao.CalcularNota(PesosAvaliacao.Mod246).ShouldBe(2.15m);
        avaliacao.Aprovada.ShouldBe(true);
    }

    [Test]
    public void DeveCalcularMediaSimples()
    {
        var avaliacao = Avaliacao(4, 3, 2, 1, 0);

        avaliacao.CalcularNota(PesosAvaliacao.MediaSimples).ShouldBe(2.00m);
        avaliacao.Aprovada.ShouldBe(true);
    }

    [Test]
    public void NotaAbaixoDeDoisNaoAprova()
    {
        var avaliacao = Avaliacao(1, 2, 1, 2, 3);

        avaliacao.CalcularNota(PesosAvaliacao.Mod246).ShouldBe(1.65m);
        avaliacao.Aprovada.ShouldBe(false);
    }

    [Test]
    public void VariavelNaoAplicavelDeixaNotaVazia()
    {
        var avaliacao = Avaliacao(4, 4, 4, 4, 4);
        avaliacao.Incerteza = null;

        avaliacao.CalcularNota(PesosAvaliacao.Mod246).ShouldBeNull();
        avaliacao.Aprovada.ShouldBeNull();
    }

    [Test]
    public void PesosTemDeSomarUm()
    {
        Should.Throw<RegraDeNegocioException>(() => new PesosAvaliacao(0.5m, 0.5m, 0.5m, 0m, 0m));
    }
}
