namespace Innovera.Domain.Events;

/// <summary>
/// Foi registado o relatório de um milestone/entregável (→ conhecimento codificado Mod.237).
/// </summary>
public class RelatorioDeMilestoneRegistadoEvent : BaseEvent
{
    public RelatorioDeMilestoneRegistadoEvent(Entrega entrega)
    {
        Entrega = entrega;
    }

    public Entrega Entrega { get; }
}
