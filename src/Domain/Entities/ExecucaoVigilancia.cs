namespace Innovera.Domain.Entities;

public class ExecucaoVigilancia : BaseAuditableEntity
{
    public int VigilanciaId { get; set; }
    public Vigilancia? Vigilancia { get; set; }
    public DateTime DataPrevista { get; set; }
    public DateTime? DataRealizada { get; set; }
    public string? Resultado { get; set; }
    public string? RelatorioUrl { get; set; }
    public string? ResponsavelId { get; set; }
}
