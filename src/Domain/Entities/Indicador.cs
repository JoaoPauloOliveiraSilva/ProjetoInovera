namespace Innovera.Domain.Entities;

public class Indicador : BaseAuditableEntity
{
    public string? Definicao { get; set; }
    public string? FormulaOuFonte { get; set; }

    public decimal ValorObjetivo { get; set; }
    public decimal ValorRealizado { get; set; }

    public string ?Periodo { get; set; }
}
