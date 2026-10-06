namespace Innovera.Domain.Entities;

public class Atividade : BaseAuditableEntity
{
    public string? NomeDaTarefa { get; set; }

    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? ResponsavelId { get; set; } 
    public string? DescricaoEntregaveis { get; set; }
    public bool IsMilestone { get; set; } 
    public bool ConfigurarAlerta { get; set; } 
    public string? GrupoResponsavel { get; set; }
    public string? CorCalendario { get; set; } 

    public TipoAtividade? Tipo { get; set; }

    public int? IniciativaId { get; set; }
    public Iniciativa? Iniciativa { get; set; }
}