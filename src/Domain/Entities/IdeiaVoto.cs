namespace Innovera.Domain.Entities;

/// <summary>A single employee's like on an idea; uniqueness is enforced by persistence configuration.</summary>
public class IdeiaVoto : BaseAuditableEntity
{
    public int IdeiaId { get; set; }
    public Ideia? Ideia { get; set; }
    public string UtilizadorId { get; set; } = string.Empty;
}
