namespace Innovera.Domain.Entities;

public class ProjectCharterRisco : BaseAuditableEntity
{
    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
    public string? Descricao { get; set; }
    public int Impacto { get; set; }
    public int Probabilidade { get; set; }
    public int Nivel => Impacto * Probabilidade;
    public bool RequerMitigacao => Nivel > 4;
    public string? EstrategiaMitigacao { get; set; }
    public int? ImpactoReavaliado { get; set; }
    public int? ProbabilidadeReavaliada { get; set; }
    public string? ResponsavelId { get; set; }
}
