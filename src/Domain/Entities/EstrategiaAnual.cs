using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class EstrategiaAnual : BaseAuditableEntity
{
    public int Ano { get; set; }
    public string? AreaAtuacao { get; set; }
    public string? Abordagem { get; set; }
    public string? PlanoAcao { get; set; }
    public int DuracaoMeses { get; set; }
    public int NumeroRecursosHumanos { get; set; }
    public decimal HorasPorMes { get; set; }
    public decimal DedicacaoTotalHoras => DuracaoMeses * NumeroRecursosHumanos * HorasPorMes;
    public decimal OrcamentoRecursosExtra { get; set; }
    public Horizonte? Horizonte { get; set; }

    public ICollection<Indicador> Indicadores { get; set; } = new List<Indicador>();
    public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
}
