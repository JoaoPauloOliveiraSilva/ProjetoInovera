namespace Innovera.Domain.Enums;

/// <summary>
/// Estado de uma ideia, igual ao fluxo do site atual:
/// 01. validação dos autores → 02. validação equipa inovação → 03. discussão pública (30 dias)
/// → em avaliação pelo manager → aprovada / não aprovada.
/// </summary>
public enum EstadoIdeia
{
    /// <summary>Os coautores indicados ainda têm de confirmar a autoria.</summary>
    ValidacaoAutores,
    /// <summary>A equipa de Inovação valida a ideia antes de a publicar.</summary>
    ValidacaoEquipa,
    EmDiscussao,
    /// <summary>Discussão terminada: "Em avaliação pelo manager" (fila de avaliação).</summary>
    EmAvaliacao,
    Aprovada,
    NaoAprovada
}
