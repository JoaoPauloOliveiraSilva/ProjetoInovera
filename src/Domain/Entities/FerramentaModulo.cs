namespace Innovera.Domain.Entities;

public class FerramentaMetodo : BaseAuditableEntity
{
    public string? Nome { get; set; }
    public string? Sumario { get; set; }
    public string? Potencial { get; set; }
    public decimal Custos { get; set; }
    public string? Divulgacao { get; set; }
    public string? Utilizacao { get; set; }
}