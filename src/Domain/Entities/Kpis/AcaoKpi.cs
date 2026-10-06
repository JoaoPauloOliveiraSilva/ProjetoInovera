namespace Innovera.Domain.Entities;

/// <summary>Plano de ação inicial ou acompanhamento de um KPI (Mod.239).</summary>
public class AcaoKpi : BaseAuditableEntity
{
    public int ValorKpiId { get; set; }

    public ValorKpi ValorKpi { get; set; } = null!;

    public TipoAcaoKpi Tipo { get; set; }

    public DateOnly DataCriacao { get; set; }

    /// <summary>Análise dos resultados (acompanhamento).</summary>
    public string? AnaliseResultados { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public string? Recursos { get; set; }

    public int? ResponsavelId { get; set; }

    public Utilizador? Responsavel { get; set; }

    public DateOnly? Prazo { get; set; }

    public DateOnly? ConcluidaEm { get; set; }

    /// <summary>Resultado eficaz / não eficaz.</summary>
    public bool? Eficaz { get; set; }

    public string? AvaliacaoAnual { get; set; }
}
