namespace Innovera.Domain.Entities;

/// <summary>Conhecimento tácito (Mod.238): detentores e estratégia de disseminação. Registo manual.</summary>
public class ConhecimentoTacito : BaseAuditableEntity
{
    public DateOnly Data { get; set; }

    public string? Empresa { get; set; }

    /// <summary>Pessoa que detém o conhecimento.</summary>
    public string Detentor { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public NivelConfidencialidade Confidencialidade { get; set; } = NivelConfidencialidade.Baixo;

    /// <summary>Disseminação: atividades, quem, quando.</summary>
    public ICollection<AtividadeGestao> Atividades { get; set; } = new List<AtividadeGestao>();
}
