namespace Innovera.Domain.Entities;

/// <summary>Like numa ideia em discussão (1 por pessoa).</summary>
public class LikeIdeia : BaseEntity
{
    public int IdeiaId { get; set; }

    public Ideia Ideia { get; set; } = null!;

    public int UtilizadorId { get; set; }

    public Utilizador Utilizador { get; set; } = null!;

    public DateTimeOffset Data { get; set; }
}
