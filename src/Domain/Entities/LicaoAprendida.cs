using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class LicaoAprendida : BaseAuditableEntity
{
    public string? Descricao { get; set; }
    public string? Impacto { get; set; }
    public Categoria Categoria { get; set; }

    public int ProjectCharterId { get; set; }
    public ProjectCharter? ProjectCharter { get; set; }
}
