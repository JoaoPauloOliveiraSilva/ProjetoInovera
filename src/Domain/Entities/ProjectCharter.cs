using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class ProjectCharter : BaseAuditableEntity
{
    public int IniciativaId { get; set; }
    public Iniciativa? Iniciativa { get; set; }

    public string? SupervisorId { get; set; }
    public string? Fase { get; set; }
    public Classe? Classe { get; set; }
    public string? ContextoJustificacao { get; set; }
    public string? BusinessCase { get; set; }
    public string? Decisao { get; set; }
    public string? Objetivos { get; set; }
    public string? ResultadosEsperados { get; set; }
    public string? ProjectCharterUrl { get; set; }
    public string? NotasConformidade { get; set; }

    public ICollection<ProjectCharterTarefa> Tarefas { get; set; } = new List<ProjectCharterTarefa>();
    public ICollection<ProjectCharterEntregavel> Entregaveis { get; set; } = new List<ProjectCharterEntregavel>();
    public ICollection<ProjectCharterMilestone> Milestones { get; set; } = new List<ProjectCharterMilestone>();
    public ICollection<ProjectCharterRisco> Riscos { get; set; } = new List<ProjectCharterRisco>();
    public ICollection<AlocacaoEquipaMensal> AlocacoesEquipa { get; set; } = new List<AlocacaoEquipaMensal>();
    public ICollection<Orcamento> Orcamentos { get; set; } = new List<Orcamento>();
    public ICollection<LicaoAprendida> LicoesAprendidas { get; set; } = new List<LicaoAprendida>();
}
