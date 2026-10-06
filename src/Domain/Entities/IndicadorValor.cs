namespace Innovera.Domain.Entities;

/// <summary>Period-specific KPI value and its review/action plan (Mod.239).</summary>
public class IndicadorValor : BaseAuditableEntity
{
    public int IndicadorId { get; set; }
    public Indicador? Indicador { get; set; }
    public DateTime InicioPeriodo { get; set; }
    public DateTime FimPeriodo { get; set; }
    public decimal? ValorAbsoluto { get; set; }
    public decimal? ValorRelativo { get; set; }
    public decimal? ValorObjetivo { get; set; }
    public bool IntroduzidoManualmente { get; set; }
    public string? PlanoAcaoInicial { get; set; }
    public string? Analise { get; set; }
    public string? Acao { get; set; }
    public string? ResponsavelId { get; set; }
    public DateTime? Prazo { get; set; }
    public string? Eficacia { get; set; }
    public string? AvaliacaoAnual { get; set; }
}
