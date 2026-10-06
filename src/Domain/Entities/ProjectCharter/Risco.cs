namespace Innovera.Domain.Entities;

/// <summary>
/// Risco do Project Charter: Impacto (1–4) × Probabilidade (1–4), resposta, plano e reavaliação.
/// Regra: I × P &gt; 4 obriga a uma estratégia de mitigação.
/// </summary>
public class Risco : BaseAuditableEntity
{
    public const int SeveridadeMaximaSemMitigacao = 4;

    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    public int Numero { get; set; }

    public DateOnly Data { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public NivelImpacto Impacto { get; set; } = NivelImpacto.Baixo;

    public NivelProbabilidade Probabilidade { get; set; } = NivelProbabilidade.Raro;

    /// <summary>I × P (1 a 16).</summary>
    public int Severidade => (int)Impacto * (int)Probabilidade;

    public bool ExigeMitigacao => Severidade > SeveridadeMaximaSemMitigacao;

    /// <summary>Verdadeiro quando o risco exige mitigação e ainda não tem plano de ação.</summary>
    public bool FaltaPlanoMitigacao => ExigeMitigacao && string.IsNullOrWhiteSpace(PlanoAcao);

    public RespostaRisco? Resposta { get; set; }

    public string? PlanoAcao { get; set; }

    public string? Recursos { get; set; }

    public int? ResponsavelId { get; set; }

    public Utilizador? Responsavel { get; set; }

    public DateOnly? Prazo { get; set; }

    public DateOnly? DataConclusao { get; set; }

    /// <summary>Avaliação da eficácia (metodologia, responsável, data, resultado).</summary>
    public string? AvaliacaoEficacia { get; set; }

    // Reavaliação
    public NivelImpacto? ImpactoReavaliado { get; set; }

    public NivelProbabilidade? ProbabilidadeReavaliada { get; set; }

    public RespostaRisco? RespostaReavaliada { get; set; }

    public int? SeveridadeReavaliada => ImpactoReavaliado.HasValue && ProbabilidadeReavaliada.HasValue
        ? (int)ImpactoReavaliado.Value * (int)ProbabilidadeReavaliada.Value
        : null;
}
