namespace Innovera.Domain.Entities;

/// <summary>
/// Linha do orçamento do Project Charter (previsto vs. real por ano).
/// A soma alimenta o investimento IDI (KPI 38).
/// </summary>
public class LinhaOrcamento : BaseAuditableEntity
{
    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    /// <summary>Ex.: "Recursos Humanos", "Ativos", "Serviços".</summary>
    public string Categoria { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int Ano { get; set; }

    public decimal ValorPrevisto { get; set; }

    public decimal? ValorReal { get; set; }

    public decimal? Variacao => ValorReal.HasValue ? ValorReal.Value - ValorPrevisto : null;

    public string? Observacoes { get; set; }
}
