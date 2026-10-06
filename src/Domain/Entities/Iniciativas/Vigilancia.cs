namespace Innovera.Domain.Entities;

/// <summary>
/// Vigilância tecnológica (Mod.246). Cada execução tem data prevista e realizada (KPI 24)
/// e o resultado alimenta o conhecimento codificado (Mod.237).
/// </summary>
public class Vigilancia : Iniciativa
{
    public override TipoIniciativa Tipo => TipoIniciativa.Vigilancia;

    public string? Descritivo { get; set; }

    public Periodicidade Periodicidade { get; set; } = Periodicidade.Mensal;

    public ICollection<ExecucaoVigilancia> Execucoes { get; set; } = new List<ExecucaoVigilancia>();
}
