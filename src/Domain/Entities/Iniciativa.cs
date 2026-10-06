namespace Innovera.Domain.Entities;

public abstract class Iniciativa : BaseAuditableEntity
{
    public string? Codigo { get; set; } 
    public string? Titulo { get; set; }

    public string? AutorId { get; set; }
    public string? GestorId { get; set; }

    public DateTime DataSubmissao { get; set; }

    public ICollection<Parceiro> Parceiros { get; set; }
    public ICollection<Atividade> Atividades { get; set; }

    protected Iniciativa()
    {
        Parceiros = new List<Parceiro>();
        Atividades = new List<Atividade>();
    }
}