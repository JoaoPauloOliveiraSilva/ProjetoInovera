namespace Innovera.Domain.Entities;

public class Vigilancia : Iniciativa
{
    public string? Descritivo { get; set; }
    public string? Temas { get; set; }
    public string? Periodicidade { get; set; }
    public string? Resultado { get; set; }
    public ICollection<ExecucaoVigilancia> Execucoes { get; set; } = new List<ExecucaoVigilancia>();
}
