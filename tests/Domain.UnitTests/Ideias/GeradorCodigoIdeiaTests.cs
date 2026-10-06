using Innovera.Domain.Services;
using NUnit.Framework;
using Shouldly;

namespace Innovera.Domain.UnitTests.Ideias;

public class GeradorCodigoIdeiaTests
{
    [TestCase(1, "AA01")]
    [TestCase(2, "AA02")]
    [TestCase(99, "AA99")]
    [TestCase(100, "AB01")]
    [TestCase(26 * 99, "AZ99")]
    [TestCase(26 * 99 + 1, "BA01")]
    [TestCase(GeradorCodigoIdeia.Maximo, "ZZ99")]
    public void DeveGerarCodigoSequencial(int numero, string esperado)
    {
        GeradorCodigoIdeia.Gerar(numero).ShouldBe(esperado);
    }

    [TestCase(0)]
    [TestCase(GeradorCodigoIdeia.Maximo + 1)]
    public void DeveRejeitarNumerosForaDoIntervalo(int numero)
    {
        Should.Throw<ArgumentOutOfRangeException>(() => GeradorCodigoIdeia.Gerar(numero));
    }
}
