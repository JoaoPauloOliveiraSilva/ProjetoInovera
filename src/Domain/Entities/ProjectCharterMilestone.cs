namespace Innovera.Domain.Entities;

public class ProjectCharterMilestone : BaseAuditableEntity
{
    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
    public string? Nome { get; set; }
    public string? CriterioVerificacao { get; set; }
    public string? MetodoControlo { get; set; }
    public DateTime DataPlaneada { get; set; }
    public DateTime? DataReal { get; set; }
    public string? RelatorioUrl { get; set; }
    public string? ResponsavelId { get; set; }
}
