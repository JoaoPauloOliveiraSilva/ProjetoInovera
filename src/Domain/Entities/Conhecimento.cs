namespace Innovera.Domain.Entities;

public class Conhecimento : BaseAuditableEntity
{
 public string? Titulo { get; set; }
    public string? ConteudoCodificado { get; set; }
    public string? ObservacoesTacitas { get; set; }  
}
