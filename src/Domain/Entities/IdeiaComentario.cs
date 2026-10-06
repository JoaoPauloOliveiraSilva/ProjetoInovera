namespace Innovera.Domain.Entities;

public class IdeiaComentario : BaseAuditableEntity
{
    public int IdeiaId { get; set; }
    public Ideia? Ideia { get; set; }
    public string AutorId { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
}
