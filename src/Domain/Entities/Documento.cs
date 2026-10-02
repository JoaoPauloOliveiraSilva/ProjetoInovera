namespace Innovera.Domain.Entities;

public class Documento : BaseAuditableEntity
{
   public string? NomeApresentacao { get; set; }
    
    public string? UrlOneDrive { get; set; }
}
