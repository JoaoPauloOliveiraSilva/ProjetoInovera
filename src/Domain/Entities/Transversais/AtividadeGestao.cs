namespace Innovera.Domain.Entities;

/// <summary>
/// Linha do "Modelo de gestão" (Atividades · Quem · Quando) usada nos Mod.236, 237 e 238.
/// É um tipo próprio (owned) de cada registo: não tem tabela nem identidade própria.
/// </summary>
public class AtividadeGestao
{
    public string Descricao { get; set; } = string.Empty;

    /// <summary>Quem realiza a atividade.</summary>
    public string? Responsavel { get; set; }

    /// <summary>Quando: data prevista (serve também para os alertas).</summary>
    public DateOnly? Data { get; set; }

    public bool Concluida { get; set; }
}
