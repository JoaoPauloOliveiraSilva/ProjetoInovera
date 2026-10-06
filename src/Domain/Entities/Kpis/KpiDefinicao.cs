namespace Innovera.Domain.Entities;

/// <summary>Definição de um dos 41 KPIs do Mod.239 (lista fechada, 11 categorias).</summary>
public class KpiDefinicao : BaseAuditableEntity
{
    /// <summary>Número do KPI no Mod.239 (1–41).</summary>
    public int Numero { get; set; }

    public string Nome { get; set; } = string.Empty;

    public CategoriaKpi Categoria { get; set; }

    public string? Formula { get; set; }

    public FonteKpi Fonte { get; set; } = FonteKpi.Automatica;

    /// <summary>Objetivo anual em texto, como no Mod.239 (ex.: "≥ 30", "≥ 80%").</summary>
    public string? ObjetivoAnual { get; set; }

    public bool Ativo { get; set; } = true;

    public ICollection<ValorKpi> Valores { get; set; } = new List<ValorKpi>();
}
