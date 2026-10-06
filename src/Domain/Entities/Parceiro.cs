using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class Parceiro : BaseAuditableEntity
{
    public string? Nome { get; set; }
    public string? Nif { get; set; }
    public TipoParceiro TipoParceiro { get; set; }
    public ICollection<Acordo> Acordos { get; set; }

    public ICollection<Iniciativa> Iniciativas { get; set; }

    public Parceiro()
    {
        Acordos = new List<Acordo>();
        Iniciativas = new List<Iniciativa>();
    }
}
