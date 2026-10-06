namespace Innovera.Domain.Entities;

public class EstrategiaAnual : BaseAuditableEntity
{
    public int? Ano { get; set; }
    public string? AreasAtuacao { get; set; }
    public string? Abordagens { get; set; }
    public decimal OrcamentoRecursosExtra { get; set; }
    
    public ICollection<Indicador> Indicadores { get; set; } 
    
    public EstrategiaAnual()
    {
        Indicadores = new List<Indicador>();
    }
}