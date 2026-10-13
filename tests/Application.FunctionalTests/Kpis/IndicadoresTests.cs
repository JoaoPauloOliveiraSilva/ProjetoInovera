using Innovera.Application.Charters;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Application.Utilizadores;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;

namespace Innovera.Application.FunctionalTests.Kpis;

public class IndicadoresTests : TestBase
{
    private static Task<int> CriarAsync(TipoIniciativa tipo, string titulo, EstadoIniciativa estado, DateOnly? inicio, DateOnly? fim, PilarEstrategico? pilar) =>
        TestApp.SendAsync(new CriarIniciativaCommand(tipo, new DadosIniciativa
        {
            Titulo = titulo,
            Estado = estado,
            DataInicio = inicio,
            DataFim = fim,
            Pilar = pilar,
        }));

    private static KpiDto Kpi(IndicadoresDto dados, int numero) => dados.Kpis.Single(k => k.Numero == numero);

    [Test]
    public async Task IndicadoresContamAsIniciativasDoAnoEEscondemOOrcamento()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var a = await CriarAsync(TipoIniciativa.Projeto, "A", EstadoIniciativa.EmCurso, new DateOnly(2025, 3, 1), null, PilarEstrategico.Pilar1);
        await CriarAsync(TipoIniciativa.Projeto, "B", EstadoIniciativa.Concluida, new DateOnly(2025, 5, 1), new DateOnly(2025, 11, 30), PilarEstrategico.Pilar1);
        await CriarAsync(TipoIniciativa.Projeto, "C", EstadoIniciativa.EmCurso, new DateOnly(2024, 2, 1), null, PilarEstrategico.Pilar2);
        await CriarAsync(TipoIniciativa.Desafio, "D", EstadoIniciativa.Cancelada, new DateOnly(2025, 1, 10), null, PilarEstrategico.Pilar3);
        await CriarAsync(TipoIniciativa.Projeto, "E", EstadoIniciativa.Concluida, new DateOnly(2023, 1, 1), new DateOnly(2023, 12, 31), null);

        var d = await TestApp.SendAsync(new ObterIndicadoresQuery(2025));
        d.Kpis.Count.ShouldBe(41);
        Kpi(d, 13).Valor.ShouldBe(2m);              // projetos novos em 2025
        Kpi(d, 13).ValorAnoAnterior.ShouldBe(1m);   // 2024: C
        Kpi(d, 14).Valor.ShouldBe(2m);              // em curso: A e C
        Kpi(d, 15).Valor.ShouldBe(1m);              // terminados em 2025: B
        Kpi(d, 35).Valor.ShouldBe(2m);
        Kpi(d, 36).Valor.ShouldBe(1m);
        Kpi(d, 37).Valor.ShouldBe(0m);              // desafio cancelado não conta
        d.IniciativasPorEstado.ShouldContain(c => c.Tipo == TipoIniciativa.Projeto && c.Estado == EstadoIniciativa.EmCurso && c.Quantidade == 2);

        // O orçamento só aparece a quem o pode ver.
        var investimento = Kpi(d, 38);
        investimento.Disponivel.ShouldBeFalse();
        investimento.Valor.ShouldBeNull();

        // Filtros do portefólio: pilar e ano.
        (await TestApp.SendAsync(new ListarIniciativasQuery(null, null, null, PilarEstrategico.Pilar1))).Count.ShouldBe(2);
        (await TestApp.SendAsync(new ListarIniciativasQuery(TipoIniciativa.Projeto, null, null, null, 2025)))
            .Select(i => i.Titulo).OrderBy(t => t).ToList().ShouldBe(new List<string> { "A", "B", "C" });
        (await TestApp.SendAsync(new ObterIniciativaQuery(a))).Resumo.Pilar.ShouldBe(PilarEstrategico.Pilar1);

        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(new GuardarLinhaOrcamentoCommand(a, null, new LinhaOrcamentoDados("RH", null, 2025, 10000m, 8000m, null)));
        await TestApp.SendAsync(new GuardarValorExternoCommand(null,
            new ValorExternoDados(TipoValorExterno.Receitas, 2025, PeriodoKpi.Anual, 100000m, null)));
        d = await TestApp.SendAsync(new ObterIndicadoresQuery(2025));
        Kpi(d, 38).Valor.ShouldBe(8000m);
        Kpi(d, 40).Valor.ShouldBe(8m);
    }

    [Test]
    public async Task EquipaDeTrabalhoComAlocacaoMensal()
    {
        await TestApp.RunAsUserAsync("ricardo@local", "Ricardo1234!", []);
        var ricardo = await TestApp.SendAsync(new ObterUtilizadorAtualQuery());

        await TestApp.RunAsEquipaInovacaoAsync();
        var id = await CriarAsync(TipoIniciativa.Projeto, "AI Poles", EstadoIniciativa.EmCurso, new DateOnly(2026, 1, 1), null, PilarEstrategico.Pilar2);
        var membro = await TestApp.SendAsync(new GuardarMembroCommand(id, null, new MembroDados(ricardo.Id, null, "Técnico")));
        var externo = await TestApp.SendAsync(new GuardarMembroCommand(id, null, new MembroDados(null, "Prof. Lima", "Consultor")));

        await Should.ThrowAsync<RegraDeNegocioException>(() => TestApp.SendAsync(new GuardarMembroCommand(id, null, new MembroDados(ricardo.Id, null, null))));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarMembroCommand(id, null, new MembroDados(null, " ", null))));

        decimal?[] meses = [null, null, null, null, null, null, null, null, 50m, 50m, 25m, null];
        await TestApp.SendAsync(new GuardarAlocacoesCommand(id, membro, 2026, meses));
        var charter = await TestApp.SendAsync(new ObterCharterQuery(id));
        charter.Pilar.ShouldBe(PilarEstrategico.Pilar2);
        charter.Equipa.Count.ShouldBe(2);
        var m = charter.Equipa.Single(x => x.Id == membro);
        m.Nome.ShouldBe(ricardo.Nome);
        m.Alocacoes.Count.ShouldBe(3);
        m.Alocacoes.Sum(a => a.Horas).ShouldBe(210m);   // 168 h × 125 %
        charter.Equipa.Single(x => x.Id == externo).UtilizadorId.ShouldBeNull();

        // Guardar outra vez substitui o ano.
        await TestApp.SendAsync(new GuardarAlocacoesCommand(id, membro, 2026, [.. Enumerable.Repeat<decimal?>(null, 8), 100m, null, null, null]));
        (await TestApp.SendAsync(new ObterCharterQuery(id))).Equipa.Single(x => x.Id == membro).Alocacoes.ShouldHaveSingleItem().Mes.ShouldBe(9);

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new GuardarAlocacoesCommand(id, membro, 2026, [10m, 20m])));
        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(
            new GuardarAlocacoesCommand(id, membro, 2026, [120m, .. Enumerable.Repeat<decimal?>(null, 11)])));

        await TestApp.SendAsync(new ApagarMembroCommand(id, externo));
        (await TestApp.SendAsync(new ObterCharterQuery(id))).Equipa.ShouldHaveSingleItem();

        await TestApp.RunAsDefaultUserAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new GuardarMembroCommand(id, null, new MembroDados(null, "X", null))));
    }
}
