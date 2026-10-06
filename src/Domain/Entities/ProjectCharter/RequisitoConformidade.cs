namespace Innovera.Domain.Entities;

/// <summary>
/// Requisito de conformidade do Project Charter: legal (NDA, contrato, RGPD…) ou
/// de Qualidade/Ambiente/Segurança por entidade.
/// </summary>
public class RequisitoConformidade : BaseAuditableEntity
{
    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    public TipoRequisitoConformidade Tipo { get; set; }

    /// <summary>Requisito ou documento (ex.: "NDA", "RGPD").</summary>
    public string Requisito { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    /// <summary>Entidade a que o requisito QAS se aplica (ex.: parceiro).</summary>
    public string? Entidade { get; set; }

    /// <summary>Resposta da entidade (requisitos QAS).</summary>
    public string? Resposta { get; set; }

    public int? ResponsavelId { get; set; }

    public Utilizador? Responsavel { get; set; }

    public DateOnly? DataPrevista { get; set; }

    public DateOnly? DataConfirmacao { get; set; }

    public EstadoRequisito Estado { get; set; } = EstadoRequisito.Pendente;

    public string? Observacoes { get; set; }
}
