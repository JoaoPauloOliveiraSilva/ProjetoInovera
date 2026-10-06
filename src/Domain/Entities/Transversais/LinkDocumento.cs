namespace Innovera.Domain.Entities;

/// <summary>
/// Link para um ficheiro no OneDrive (RF15). A plataforma guarda só o link, não o ficheiro.
/// </summary>
public class LinkDocumento : BaseAuditableEntity
{
    public int IniciativaId { get; set; }

    public Iniciativa Iniciativa { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
