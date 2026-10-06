namespace Innovera.Domain.Entities;

/// <summary>Definition/catalog entry for one of the innovation KPIs.</summary>
public class Indicador : BaseAuditableEntity
{
    public int NumeroId { get; set; }
    public string? Categoria { get; set; }
    public string? NomeIndicador { get; set; }
    public string? Definicao { get; set; }
    public string? FormulaOuFonte { get; set; }
    public bool CalculadoAutomaticamente { get; set; }
    public string? PlanoAcaoInicial { get; set; }

    public int? EstrategiaAnualId { get; set; }
    public EstrategiaAnual? EstrategiaAnual { get; set; }
    public ICollection<IndicadorValor> Valores { get; set; } = new List<IndicadorValor>();
}
