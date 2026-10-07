namespace Innovera.Domain.Entities;

/// <summary>
/// Autor de uma ideia. Quem regista pode indicar outros autores ("outro autor ou autores"),
/// que têm de confirmar a autoria (passo "01. validação dos autores").
/// </summary>
public class AutorIdeia : BaseEntity
{
    public int IdeiaId { get; set; }

    public Ideia Ideia { get; set; } = null!;

    public int UtilizadorId { get; set; }

    public Utilizador Utilizador { get; set; } = null!;

    public bool Confirmado { get; set; }

    public DateTimeOffset? ConfirmadoEm { get; set; }
}
