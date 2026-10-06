namespace Innovera.Domain.Entities;

/// <summary>Membro da equipa de uma iniciativa, com a alocação mensal (%).</summary>
public class MembroEquipa : BaseAuditableEntity
{
    public int IniciativaId { get; set; }

    public Iniciativa Iniciativa { get; set; } = null!;

    public int UtilizadorId { get; set; }

    public Utilizador Utilizador { get; set; } = null!;

    public string Funcao { get; set; } = string.Empty;

    public ICollection<AlocacaoMensal> Alocacoes { get; set; } = new List<AlocacaoMensal>();
}
