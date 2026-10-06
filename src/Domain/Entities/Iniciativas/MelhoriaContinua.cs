namespace Innovera.Domain.Entities;

/// <summary>
/// Melhoria contínua (Mod.246). Estados: Pendente, Em análise, Rejeitada, Em implementação, Concluída.
/// </summary>
public class MelhoriaContinua : Iniciativa
{
    public override TipoIniciativa Tipo => TipoIniciativa.MelhoriaContinua;

    public string? Resultado { get; set; }
}
