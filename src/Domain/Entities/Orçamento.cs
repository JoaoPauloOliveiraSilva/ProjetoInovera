namespace Innovera.Domain.Entities;

public class Orçamento : BaseAuditableEntity
{
   public string? DescricaoRubrica { get; set; }
    
    public decimal ValorAtribuido { get; set; }

    public int IniciativaId { get; set; }
    public Iniciativa? Iniciativa { get; set; }
}
