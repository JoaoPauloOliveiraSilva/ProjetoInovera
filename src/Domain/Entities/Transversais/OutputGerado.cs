namespace Innovera.Domain.Entities;

/// <summary>
/// Ficheiro Excel (modelos Mod.xxx) ou PDF gerado pela plataforma.
/// </summary>
public class OutputGerado : BaseAuditableEntity
{
    public ModeloOutput Modelo { get; set; }

    public FormatoOutput Formato { get; set; }

    public GatilhoOutput Gatilho { get; set; }

    /// <summary>Iniciativa a que o output se refere (nulo nos modelos globais, ex.: Mod.239).</summary>
    public int? IniciativaId { get; set; }

    public Iniciativa? Iniciativa { get; set; }

    /// <summary>Ano ou período a que os dados se referem (ex.: "2026", "2026-S1").</summary>
    public string? Periodo { get; set; }

    public DateTimeOffset GeradoEm { get; set; }

    public int? GeradoPorId { get; set; }

    public Utilizador? GeradoPor { get; set; }

    public string NomeFicheiro { get; set; } = string.Empty;

    /// <summary>Link para o ficheiro (armazenamento da plataforma ou OneDrive).</summary>
    public string? Url { get; set; }
}
