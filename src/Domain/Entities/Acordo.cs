namespace Innovera.Domain.Entities;

public class Acordo : BaseAuditableEntity
{
    public string? TipoProtocolo { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }
    public bool GestaoPI { get; set; }
    public string? Observacoes { get; set; }
    public string? Estado { get; set; }

    public int ParceiroId { get; set; }
    public Parceiro? Parceiro { get; set; }
}
