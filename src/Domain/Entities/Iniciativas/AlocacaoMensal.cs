namespace Innovera.Domain.Entities;

/// <summary>
/// Percentagem de dedicação de um membro num mês. Horas = 8 h × 21 dias × %, como no Project Charter.
/// </summary>
public class AlocacaoMensal : BaseEntity
{
    public const decimal HorasPorMes = 8m * 21m;

    public int MembroEquipaId { get; set; }

    public MembroEquipa MembroEquipa { get; set; } = null!;

    public int Ano { get; set; }

    /// <summary>Mês (1–12).</summary>
    public int Mes { get; set; }

    /// <summary>Dedicação de 0 a 100 (%).</summary>
    public decimal Percentagem { get; set; }

    public decimal Horas => HorasPorMes * Percentagem / 100m;
}
