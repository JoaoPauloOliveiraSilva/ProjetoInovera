namespace Innovera.Domain.Entities;

public class Parceiro : BaseAuditableEntity
{
    public string? Nome { get; set; }
    public string? Nif { get; set; }
    public string? TipoParceiro { get; set; } 
    public ICollection<Documento> Acordos { get; set; }


    public ICollection<Iniciativa> Iniciativas { get; set; }

    public ICollection<Indicador> Indicadores { get; set; }

    public Parceiro()
    {
        Acordos = new List<Documento>();
        Iniciativas = new List<Iniciativa>();
        Indicadores = new List<Indicador>();
    }
}