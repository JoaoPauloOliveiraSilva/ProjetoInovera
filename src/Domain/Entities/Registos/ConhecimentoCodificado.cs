namespace Innovera.Domain.Entities;

/// <summary>
/// Entrada do repositório de conhecimento codificado (Mod.237, KPI 22). Criada automaticamente a partir
/// de relatórios de milestones/entregáveis e de resultados de vigilâncias, ou manualmente.
/// </summary>
public class ConhecimentoCodificado : BaseAuditableEntity
{
    public DateOnly Data { get; set; }

    /// <summary>Título do documento (ex.: "Relatório AI Poles Milestone 1").</summary>
    public string Documento { get; set; } = string.Empty;

    public DateOnly? ArquivadoEm { get; set; }

    /// <summary>Modo de consulta / local (ex.: "OneDrive dstelecom inov").</summary>
    public string? ModoConsulta { get; set; }

    public string? Url { get; set; }

    public string? Descricao { get; set; }

    /// <summary>Resultado obtido (ex.: "Síntese e partilha do conhecimento gerado").</summary>
    public string? Resultado { get; set; }

    public OrigemConhecimento Origem { get; set; } = OrigemConhecimento.Manual;

    public int? IniciativaId { get; set; }

    public Iniciativa? Iniciativa { get; set; }

    public int? EntregaId { get; set; }

    public Entrega? Entrega { get; set; }

    public int? ExecucaoVigilanciaId { get; set; }

    public ExecucaoVigilancia? ExecucaoVigilancia { get; set; }

    /// <summary>Partilha: atividades, quem, quando.</summary>
    public ICollection<AtividadeGestao> Atividades { get; set; } = new List<AtividadeGestao>();
}
