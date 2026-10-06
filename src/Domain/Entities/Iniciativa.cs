using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public abstract class Iniciativa : BaseAuditableEntity
{
    public string? Codigo { get; set; } 
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }

    public string? AutorId { get; set; }
    public string? GestorId { get; set; }

    public DateTime DataSubmissao { get; set; }
    public DateTime? DataInicio { get; set; }
    public DateTime? DataFim { get; set; }

    public EstadoIniciativa Estado { get; set; }
    public string? PontoDeSituacao { get; set; }
    public string? PilarEstrategico { get; set; }
    public Horizonte? Horizonte { get; set; }
    public string? Origem { get; set; }

    public ICollection<Parceiro> Parceiros { get; set; }
    public ICollection<Atividade> Atividades { get; set; }
    public ProjectCharter? Charter { get; set; }

    protected Iniciativa()
    {
        Parceiros = new List<Parceiro>();
        Atividades = new List<Atividade>();
    }
}
