namespace Innovera.Domain.Entities;

/// <summary>
/// Linha da estratégia de inovação anual (Mod.52). Dedicação horária total = duração × n.º RH × horas/mês.
/// </summary>
public class LinhaEstrategia : BaseAuditableEntity
{
    public int Ano { get; set; }

    /// <summary>Área de atuação (ex.: "Tecnologia de Fibra Ótica", "Cultura").</summary>
    public string AreaAtuacao { get; set; } = string.Empty;

    /// <summary>Abordagem / aplicação específica.</summary>
    public string? Abordagem { get; set; }

    public string? PlanoAcao { get; set; }

    /// <summary>KPIs associados (Mod.239).</summary>
    /// <summary>Números dos KPIs (1–41), às vezes dois (ex.: "14 e 36").</summary>
    public List<int> Kpis { get; set; } = [];

    public int? DuracaoMeses { get; set; }

    public int? NumeroRecursosHumanos { get; set; }

    public decimal? HorasMesPorRecurso { get; set; }

    public decimal? DedicacaoTotalHoras => DuracaoMeses * NumeroRecursosHumanos * HorasMesPorRecurso;

    /// <summary>Recursos extra (ex.: "Consultoria INESC - 30k€").</summary>
    public string? RecursosExtra { get; set; }

    public Horizonte? Horizonte { get; set; }

    /// <summary>Iniciativa ligada (opcional).</summary>
    public int? IniciativaId { get; set; }

    public Iniciativa? Iniciativa { get; set; }
}
