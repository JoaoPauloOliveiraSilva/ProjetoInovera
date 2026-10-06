namespace Innovera.Domain.Events;

/// <summary>
/// Uma vigilância registou um resultado (→ conhecimento codificado Mod.237).
/// </summary>
public class ResultadoVigilanciaRegistadoEvent : BaseEvent
{
    public ResultadoVigilanciaRegistadoEvent(ExecucaoVigilancia execucao)
    {
        Execucao = execucao;
    }

    public ExecucaoVigilancia Execucao { get; }
}
