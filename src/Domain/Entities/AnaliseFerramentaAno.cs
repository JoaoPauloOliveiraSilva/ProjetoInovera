namespace Innovera.Domain.Entities;

public class AnaliseFerramentaAno : BaseAuditableEntity
{
    public int FerramentaMetodoId { get; set; }
    public FerramentaMetodo? FerramentaMetodo { get; set; }
    public int Ano { get; set; }
    public string? Analise { get; set; }
}
