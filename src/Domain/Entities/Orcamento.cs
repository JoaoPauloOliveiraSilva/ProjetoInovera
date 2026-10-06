namespace Innovera.Domain.Entities;

public class Orcamento : BaseAuditableEntity
{
    public string? DescricaoRubrica { get; set; }
    public int Ano { get; set; }
    public decimal ValorPrevisto { get; set; }
    public decimal ValorReal { get; set; }

    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
}
