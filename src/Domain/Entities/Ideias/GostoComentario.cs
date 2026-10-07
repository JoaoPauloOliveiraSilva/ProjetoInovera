namespace Innovera.Domain.Entities;

/// <summary>Like num comentário (1 por pessoa).</summary>
public class GostoComentario : BaseEntity
{
    public int ComentarioId { get; set; }

    public ComentarioIdeia Comentario { get; set; } = null!;

    public int UtilizadorId { get; set; }

    public Utilizador Utilizador { get; set; } = null!;

    public DateTimeOffset Data { get; set; }
}
