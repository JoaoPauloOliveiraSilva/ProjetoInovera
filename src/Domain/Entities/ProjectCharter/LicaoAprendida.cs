namespace Innovera.Domain.Entities;

/// <summary>Lição aprendida num Project Charter; agregada no repositório global (Mod.252).</summary>
public class LicaoAprendida : BaseAuditableEntity
{
    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    public CategoriaLicao Categoria { get; set; }

    public string Licao { get; set; } = string.Empty;

    /// <summary>Descrição / contexto.</summary>
    public string? Descricao { get; set; }

    /// <summary>Impacto na iniciativa.</summary>
    public string? Impacto { get; set; }
}
