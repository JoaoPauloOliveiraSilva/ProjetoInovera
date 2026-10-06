namespace Innovera.Domain.Events;

/// <summary>
/// Uma ideia foi avaliada pela CE (recalcular KPIs 1–8, portefólio Mod.246, auditoria).
/// </summary>
public class IdeiaAvaliadaEvent : BaseEvent
{
    public IdeiaAvaliadaEvent(Ideia ideia)
    {
        Ideia = ideia;
    }

    public Ideia Ideia { get; }
}
