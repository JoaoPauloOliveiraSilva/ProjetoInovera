namespace Innovera.Domain.Entities;

/// <summary>Ferramenta ou método do portefólio (Mod.241), com uma análise por ano.</summary>
public class FerramentaMetodo : BaseAuditableEntity
{
    public DateOnly Data { get; set; }

    /// <summary>Identificação (ex.: "Design Thinking", "PDCA").</summary>
    public string Nome { get; set; } = string.Empty;

    public string? SumarioExecutivo { get; set; }

    public string? Potencial { get; set; }

    public decimal? CustoAquisicao { get; set; }

    /// <summary>Divulgação (link).</summary>
    public string? Divulgacao { get; set; }

    public ICollection<AnaliseAnual> Analises { get; set; } = new List<AnaliseAnual>();
}
