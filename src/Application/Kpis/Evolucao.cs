namespace Innovera.Application.Kpis;

/// <summary>
/// Valor de um KPI num ano. <paramref name="Oficial"/> = valor importado do Mod.239 (ano fechado);
/// <paramref name="Parcial"/> = ano em curso (valor até hoje).
/// </summary>
public record PontoEvolucaoDto(int Ano, decimal? Valor, string? Objetivo, bool? Atingido, bool Oficial, bool Parcial);

public record EvolucaoKpiDto(int Numero, string Nome, UnidadeKpi Unidade, IReadOnlyList<PontoEvolucaoDto> Pontos, string? Nota);

/// <summary>
/// Evolução anual de um KPI desde o primeiro ano com valores do Mod.239 (ou os últimos 4 anos) até ao ano atual:
/// nos anos fechados vale o valor oficial importado, nos outros o calculado com os registos da plataforma.
/// </summary>
[Authorize]
public record ObterEvolucaoKpiQuery(int Numero) : IRequest<EvolucaoKpiDto>;

public class ObterEvolucaoKpiQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<ObterEvolucaoKpiQuery, EvolucaoKpiDto>
{
    public async Task<EvolucaoKpiDto> Handle(ObterEvolucaoKpiQuery request, CancellationToken cancellationToken)
    {
        var def = PlanosKpi.Definicao(request.Numero);
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        var registos = await context.ValoresKpi.AsNoTracking()
            .Where(v => v.Kpi.Numero == request.Numero && v.Periodo == PeriodoKpi.Anual)
            .Select(v => new { v.Ano, v.ValorAbsoluto, v.Objetivo })
            .ToListAsync(cancellationToken);
        var porAno = registos.ToDictionary(v => v.Ano);

        var primeiro = Math.Min(hoje.Year - 3, registos.Where(r => r.ValorAbsoluto is not null).Select(r => r.Ano).DefaultIfEmpty(hoje.Year).Min());
        var anos = Enumerable.Range(primeiro, hoje.Year - primeiro + 1).ToList();

        if (def.Financeiro && !await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Ver, cancellationToken)
            && !await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Gerir, cancellationToken))
        {
            return new EvolucaoKpiDto(def.Numero, def.Nome, def.Unidade,
                anos.Select(a => new PontoEvolucaoDto(a, null, null, null, false, a == hoje.Year)).ToList(), "Só para quem pode ver o orçamento.");
        }

        var dados = await DadosKpi.CarregarAsync(context, cancellationToken);
        var pontos = anos.Select(ano =>
        {
            var registo = porAno.GetValueOrDefault(ano);
            var fechado = CalculoKpi.Intervalo(ano, PeriodoKpi.Anual).Fim < hoje;
            var oficial = fechado && registo?.ValorAbsoluto is not null;
            var valor = oficial
                ? registo!.ValorAbsoluto
                : CalculoKpi.Calcular(dados, ano).GetValueOrDefault(def.Numero).Valor;
            return new PontoEvolucaoDto(ano, valor, registo?.Objetivo, fechado ? ObjetivoKpi.Atingido(registo?.Objetivo, valor) : null,
                oficial, !fechado);
        }).ToList();

        return new EvolucaoKpiDto(def.Numero, def.Nome, def.Unidade, pontos, def.Nota);
    }
}
