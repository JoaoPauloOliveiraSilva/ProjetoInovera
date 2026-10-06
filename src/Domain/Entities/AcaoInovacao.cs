using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class AcaoInovacao : Iniciativa
{
    public string? TipoAcao { get; set; }
    public OrigemAcaoInovacao OrigemEvento { get; set; }
    public int NumeroParticipantes { get; set; }
    public string? LinkConsulta { get; set; }
    public string? ReportUrl { get; set; }
}
