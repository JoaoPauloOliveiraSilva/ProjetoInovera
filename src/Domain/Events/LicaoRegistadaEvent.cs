namespace Innovera.Domain.Events;

/// <summary>
/// Foi registada uma lição num Project Charter (→ repositório Mod.252).
/// </summary>
public class LicaoRegistadaEvent : BaseEvent
{
    public LicaoRegistadaEvent(LicaoAprendida licao)
    {
        Licao = licao;
    }

    public LicaoAprendida Licao { get; }
}
