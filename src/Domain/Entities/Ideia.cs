using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

/// <summary>An idea has its own public discussion and evaluation lifecycle.</summary>
public class Ideia : BaseAuditableEntity
{
    public string? Codigo { get; set; }
    public string? Titulo { get; set; }
    public string? Descricao { get; set; }
    public string? AutorId { get; set; }
    public DateTime DataSubmissao { get; set; }
    public string? Origem { get; set; }
    public Responsabilidade? Responsabilidade { get; set; }

    public EstadoIdeia Estado { get; set; }
    public DateTime? InicioDiscussao { get; set; }
    public DateTime? FimDiscussao { get; set; }
    public ICollection<IdeiaComentario> Comentarios { get; set; } = new List<IdeiaComentario>();
    public ICollection<IdeiaVoto> Votos { get; set; } = new List<IdeiaVoto>();

    public EscalaDeAvaliacao? AvaliacaoCusto { get; set; }
    public EscalaDeAvaliacao? AvaliacaoEnquadramento { get; set; }
    public EscalaDeAvaliacao? AvaliacaoBeneficio { get; set; }
    public EscalaDeAvaliacao? AvaliacaoAdequacaoTecnica { get; set; }
    public EscalaDeAvaliacao? AvaliacaoIncerteza { get; set; }
    public decimal? TotalAvaliacao { get; set; }
    public DateTime? AvaliadaEm { get; set; }
    public Classificacao? Classificacao { get; set; }
    public Classe? Classe { get; set; }
    public string? StatusDetalhado { get; set; }

    public int? IniciativaResultanteId { get; set; }
    public Iniciativa? IniciativaResultante { get; set; }
}
