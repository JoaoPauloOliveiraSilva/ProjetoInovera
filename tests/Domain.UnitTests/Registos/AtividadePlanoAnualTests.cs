using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace Innovera.Domain.UnitTests.Registos;

public class AtividadePlanoAnualTests
{
    [Test]
    public void SemRecorrenciaTemUmaSoOcorrencia()
    {
        var a = new AtividadePlanoAnual { DataInicio = new DateOnly(2026, 3, 10) };

        a.Ocorrencias(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).ShouldBe(new[] { new DateOnly(2026, 3, 10) });
        a.Ocorrencias(new DateOnly(2027, 1, 1), new DateOnly(2027, 12, 31)).ShouldBeEmpty();
    }

    [Test]
    public void MensalNaoAcumulaDesviosNosMesesCurtos()
    {
        var a = new AtividadePlanoAnual { DataInicio = new DateOnly(2026, 1, 31), Recorrencia = Periodicidade.Mensal };

        var ocorrencias = a.Ocorrencias(new DateOnly(2026, 1, 1), new DateOnly(2026, 4, 30)).ToList();

        ocorrencias.ShouldBe(new[] { new DateOnly(2026, 1, 31), new DateOnly(2026, 2, 28), new DateOnly(2026, 3, 31), new DateOnly(2026, 4, 30) });
    }

    [Test]
    public void TrimestralSoDentroDoIntervalo()
    {
        var a = new AtividadePlanoAnual { DataInicio = new DateOnly(2025, 11, 15), Recorrencia = Periodicidade.Trimestral };

        a.Ocorrencias(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)).ShouldBe(
            new[] { new DateOnly(2026, 2, 15), new DateOnly(2026, 5, 15), new DateOnly(2026, 8, 15), new DateOnly(2026, 11, 15) });
    }

    [Test]
    public void AvisoComAntecedenciaEDuracao()
    {
        var a = new AtividadePlanoAnual
        {
            DataInicio = new DateOnly(2026, 6, 10), DataFim = new DateOnly(2026, 6, 12), DiasAntecedenciaAviso = 7,
        };

        a.DataAviso(a.DataInicio).ShouldBe(new DateOnly(2026, 6, 3));
        a.DuracaoDias.ShouldBe(2);
    }
}
