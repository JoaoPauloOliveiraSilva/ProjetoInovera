using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace Innovera.Domain.UnitTests.Charter;

public class RiscoTests
{
    [Test]
    public void SeveridadeEImpactoVezesProbabilidade()
    {
        var risco = new Risco { Impacto = NivelImpacto.Alto, Probabilidade = NivelProbabilidade.PoucoProvavel };

        risco.Severidade.ShouldBe(9);
        risco.ExigeMitigacao.ShouldBeTrue();
        risco.FaltaPlanoMitigacao.ShouldBeTrue();
    }

    [Test]
    public void SeveridadeAteQuatroNaoExigeMitigacao()
    {
        var risco = new Risco { Impacto = NivelImpacto.Medio, Probabilidade = NivelProbabilidade.Improvavel };

        risco.Severidade.ShouldBe(4);
        risco.ExigeMitigacao.ShouldBeFalse();
    }

    [Test]
    public void ReavaliacaoCalculaNovaSeveridade()
    {
        var risco = new Risco
        {
            Impacto = NivelImpacto.Extremo,
            Probabilidade = NivelProbabilidade.MuitoProvavel,
            ImpactoReavaliado = NivelImpacto.Medio,
            ProbabilidadeReavaliada = NivelProbabilidade.Raro,
        };

        risco.Severidade.ShouldBe(16);
        risco.SeveridadeReavaliada.ShouldBe(2);
    }
}
