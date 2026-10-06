namespace Innovera.Domain.Entities;

public class Utilizador : BaseAuditableEntity
{
   public string? EntraObjectId { get; set; } 
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public string? Departamento { get; set; }
}
