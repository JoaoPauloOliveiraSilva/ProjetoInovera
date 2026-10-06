namespace Innovera.Domain.Entities;

/// <summary>
/// Projeto de mestrado (Mod.246, folha Mestrados). O título da iniciativa é o título da dissertação.
/// </summary>
public class Mestrado : Iniciativa
{
    public override TipoIniciativa Tipo => TipoIniciativa.Mestrado;

    public string NomeAluno { get; set; } = string.Empty;

    /// <summary>Curso e universidade.</summary>
    public string? Curso { get; set; }

    /// <summary>Orientador na empresa.</summary>
    public int? OrientadorId { get; set; }

    public Utilizador? Orientador { get; set; }
}
