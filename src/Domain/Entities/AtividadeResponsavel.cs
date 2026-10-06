namespace Innovera.Domain.Entities;

public class AtividadeResponsavel : BaseAuditableEntity
{
    public int AtividadeId { get; set; }
    public Atividade? Atividade { get; set; }
    public string UtilizadorId { get; set; } = string.Empty;
}
