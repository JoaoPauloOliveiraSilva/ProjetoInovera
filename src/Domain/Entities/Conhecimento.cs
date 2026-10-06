using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class Conhecimento : BaseAuditableEntity
{
    public TipoConhecimento Tipo { get; set; }

    public string? Titulo { get; set; }
    public string? Descricao { get; set; }

    // Codificado (Mod.237)
    public string? DocumentoUrl { get; set; }
    public string? ArquivadoEm { get; set; }
    public string? ModoConsulta { get; set; }
    public string? Resultado { get; set; }

    // Tacito (Mod.238)
    public string? Empresa { get; set; }
    public string? DetentorId { get; set; }
    public string? Confidencialidade { get; set; }

    public int? ProjectCharterMilestoneId { get; set; }
    public ProjectCharterMilestone? ProjectCharterMilestone { get; set; }
    public int? ExecucaoVigilanciaId { get; set; }
    public ExecucaoVigilancia? ExecucaoVigilancia { get; set; }

    public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
}
