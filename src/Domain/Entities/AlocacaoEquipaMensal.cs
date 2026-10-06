namespace Innovera.Domain.Entities;

public class AlocacaoEquipaMensal : BaseAuditableEntity
{
    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
    public string? UtilizadorId { get; set; }
    public string? GrupoResponsavel { get; set; }
    public DateTime Mes { get; set; }
    public decimal PercentagemPlaneada { get; set; }
    public decimal? PercentagemReal { get; set; }
}
