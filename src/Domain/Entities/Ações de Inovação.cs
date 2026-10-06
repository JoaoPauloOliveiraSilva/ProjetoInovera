namespace Innovera.Domain.Entities;

public class AcaoInovacao : Iniciativa
{
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public string? Origem { get; set; }
    public string? PontoSituacao { get; set; }
    public string? Consultar { get; set; }

    

}
