namespace Innovera.Domain.Entities;

/// <summary>
/// Atividade do planeamento anual (Mod.69), ex.: avaliação de ideias com a CE, GetTogether, SIFIDE.
/// Gera alertas com a antecedência configurada (RF16).
/// </summary>
public class AtividadePlanoAnual : BaseAuditableEntity
{
    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    /// <summary>Cor no calendário (hex, ex.: "#E05C4D").</summary>
    public string? Cor { get; set; }

    public DateOnly DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    public Periodicidade Recorrencia { get; set; } = Periodicidade.Nenhuma;

    public int DiasAntecedenciaAviso { get; set; } = 7;

    public ICollection<Utilizador> Responsaveis { get; set; } = new List<Utilizador>();

    /// <summary>Data em que deve ser enviado o aviso da próxima ocorrência.</summary>
    public DateOnly DataAviso(DateOnly ocorrencia) => ocorrencia.AddDays(-DiasAntecedenciaAviso);

    /// <summary>Duração de cada ocorrência, em dias (0 = um só dia).</summary>
    public int DuracaoDias => DataFim is { } fim && fim > DataInicio ? fim.DayNumber - DataInicio.DayNumber : 0;

    /// <summary>Datas de início das ocorrências entre <paramref name="de"/> e <paramref name="ate"/> (inclusive).</summary>
    public IEnumerable<DateOnly> Ocorrencias(DateOnly de, DateOnly ate)
    {
        if (ate < de)
        {
            yield break;
        }

        if (Recorrencia == Periodicidade.Nenhuma)
        {
            if (DataInicio >= de && DataInicio <= ate)
            {
                yield return DataInicio;
            }

            yield break;
        }

        for (var n = 0; ; n++)
        {
            var data = Avancar(n);
            if (data > ate)
            {
                yield break;
            }

            if (data >= de)
            {
                yield return data;
            }
        }
    }

    /// <summary>A n-ésima ocorrência, contada sempre a partir da data de início (sem acumular desvios nos meses curtos).</summary>
    private DateOnly Avancar(int n) => Recorrencia switch
    {
        Periodicidade.Semanal => DataInicio.AddDays(7 * n),
        Periodicidade.Mensal => DataInicio.AddMonths(n),
        Periodicidade.Trimestral => DataInicio.AddMonths(3 * n),
        Periodicidade.Semestral => DataInicio.AddMonths(6 * n),
        Periodicidade.Anual => DataInicio.AddYears(n),
        _ => DataInicio,
    };
}
