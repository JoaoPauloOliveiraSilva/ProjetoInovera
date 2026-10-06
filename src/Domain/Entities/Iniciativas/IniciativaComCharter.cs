namespace Innovera.Domain.Entities;

/// <summary>
/// Iniciativas acompanhadas com Project Charter: projetos e desafios
/// ("acompanhados como se de um projeto se tratasse").
/// </summary>
public abstract class IniciativaComCharter : Iniciativa
{
    public ProjectCharter? Charter { get; set; }
}
