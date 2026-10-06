namespace Innovera.Domain.Entities;

/// <summary>
/// Parceiro (Mod.235). Separado do acordo porque o mesmo parceiro (ex.: INESC, UMinho) tem vários acordos.
/// O tipo (empresarial vs. académico/ENESI) alimenta os KPIs 26–27.
/// </summary>
public class Parceiro : BaseAuditableEntity
{
    public string Nome { get; set; } = string.Empty;

    public string? Nif { get; set; }

    public TipoParceiro Tipo { get; set; }

    public string? Descricao { get; set; }

    public ICollection<AcordoParceria> Acordos { get; set; } = new List<AcordoParceria>();

    public ICollection<Iniciativa> Iniciativas { get; set; } = new List<Iniciativa>();
}
