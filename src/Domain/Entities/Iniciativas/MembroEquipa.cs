namespace Innovera.Domain.Entities;

/// <summary>
/// Membro da equipa de uma iniciativa, com a alocação mensal (%). Pode ser um utilizador da plataforma
/// ou alguém de fora (ex.: um aluno ou um parceiro), registado só pelo nome.
/// </summary>
public class MembroEquipa : BaseAuditableEntity
{
    public int IniciativaId { get; set; }

    public Iniciativa Iniciativa { get; set; } = null!;

    public int? UtilizadorId { get; set; }

    public Utilizador? Utilizador { get; set; }

    /// <summary>Nome de quem não é utilizador da plataforma.</summary>
    public string? Nome { get; set; }

    public string NomeVisivel => Utilizador?.Nome ?? Nome ?? string.Empty;

    public string Funcao { get; set; } = string.Empty;

    public ICollection<AlocacaoMensal> Alocacoes { get; set; } = new List<AlocacaoMensal>();
}
