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
}
