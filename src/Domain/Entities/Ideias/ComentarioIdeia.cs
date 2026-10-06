namespace Innovera.Domain.Entities;

/// <summary>Comentário feito durante o mês de discussão pública.</summary>
public class ComentarioIdeia : BaseAuditableEntity
{
    public int IdeiaId { get; set; }

    public Ideia Ideia { get; set; } = null!;

    public int AutorId { get; set; }

    public Utilizador Autor { get; set; } = null!;

    public DateTimeOffset Data { get; set; }

    public string Texto { get; set; } = string.Empty;
}
