namespace Innovera.Domain.Entities;

public class ProjectCharterEntregavel : BaseAuditableEntity
{
    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
    public string? Nome { get; set; }
    public string? Descricao { get; set; }
    public DateTime? DataPlaneada { get; set; }
    public DateTime? DataReal { get; set; }
    public string? UrlDocumento { get; set; }
    public string? ResponsavelId { get; set; }
}
