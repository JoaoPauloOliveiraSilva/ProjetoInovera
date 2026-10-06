namespace Innovera.Domain.Enums;

/// <summary>
/// Estado genérico de uma iniciativa (os estados de cada tipo ainda estão a validar com a dstelecom).
/// </summary>
public enum EstadoIniciativa
{
    Pendente,
    EmAnalise,
    EmCurso,
    EmImplementacao,
    Concluida,
    Rejeitada,
    Cancelada
}
