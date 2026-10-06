namespace Innovera.Domain.Entities;

public class ProjectCharterTarefa : BaseAuditableEntity
{
    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
    public string? CodigoWbs { get; set; }
    public string? Descricao { get; set; }
    public DateTime? InicioPlaneado { get; set; }
    public DateTime? FimPlaneado { get; set; }
    public DateTime? InicioReal { get; set; }
    public DateTime? FimReal { get; set; }
    public string? ResponsavelId { get; set; }
}
