namespace Innovera.Domain.Entities;

/// <summary>Valor de um KPI num período (snapshot): absoluto, relativo e objetivo.</summary>
public class ValorKpi : BaseAuditableEntity
{
    public int KpiId { get; set; }

    public KpiDefinicao Kpi { get; set; } = null!;

    public int Ano { get; set; }

    public PeriodoKpi Periodo { get; set; } = PeriodoKpi.Anual;

    public decimal? ValorAbsoluto { get; set; }

    public decimal? ValorRelativo { get; set; }

    public string? Objetivo { get; set; }

    public DateTimeOffset? CalculadoEm { get; set; }

    /// <summary>Plano de ação inicial e acompanhamento (bloco ISO do Mod.239).</summary>
    public ICollection<AcaoKpi> Acoes { get; set; } = new List<AcaoKpi>();
}
