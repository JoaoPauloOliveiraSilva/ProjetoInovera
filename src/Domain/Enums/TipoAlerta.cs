namespace Innovera.Domain.Enums;

/// <summary>
/// Tipo de alerta.
/// </summary>
public enum TipoAlerta
{
    Prazo,
    Validade,
    PlanoAnual,

    /// <summary>Prazo já passou (tarefa ou entrega do Project Charter atrasada).</summary>
    Atraso
}
