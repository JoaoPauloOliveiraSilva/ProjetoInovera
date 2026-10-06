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
    public string? Recorrencia { get; set; }
    public string? DefinidoPorId { get; set; }
    public string? AtualizadoPorId { get; set; }
    public string? AprovadoPorId { get; set; }

    public TipoAtividade? Tipo { get; set; }

    public int? IniciativaId { get; set; }
    public Iniciativa? Iniciativa { get; set; }

    public int? EstrategiaAnualId { get; set; }
    public EstrategiaAnual? EstrategiaAnual { get; set; }

    public int? AtivoIntangivelId { get; set; }
    public AtivoIntangivel? AtivoIntangivel { get; set; }

    public int? ConhecimentoId { get; set; }
    public Conhecimento? Conhecimento { get; set; }

    public ICollection<AtividadeResponsavel> Responsaveis { get; set; } = new List<AtividadeResponsavel>();
}
