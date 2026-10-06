namespace Innovera.Domain.Enums;

/// <summary>
/// Estado de uma ideia no ciclo de vida (Figura 3 do documento).
/// </summary>
public enum EstadoIdeia
{
    Submetida,
    EmDiscussao,
    EmAvaliacao,
    Aprovada,
    NaoAprovada,
    /// <summary>Ideia duplicada ou com responsabilidade "dst" (grupo): não é avaliada pela CE.</summary>
    SemAvaliacao
}
