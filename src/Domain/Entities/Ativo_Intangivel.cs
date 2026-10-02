namespace Innovera.Domain.Entities;

public class Ativo_intangivel : BaseAuditableEntity
{
    public string? TituloPropriedade { get; set; } 
    public string? Descricao { get; set; }
    
    public string? EstadoGestao { get; set; }
}
