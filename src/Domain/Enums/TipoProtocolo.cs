namespace Innovera.Domain.Enums;

/// <summary>
/// Tipo de protocolo de um acordo de parceria (Mod.235).
/// </summary>
public enum TipoProtocolo
{
    InvestigacaoPartilhada,
    CoPromocao,
    /// <summary>Acordo de confidencialidade (NDA).</summary>
    Confidencialidade,
    Inovacao,
    Investigacao
}
