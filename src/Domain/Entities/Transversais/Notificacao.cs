namespace Innovera.Domain.Entities;

/// <summary>
/// Aviso para um utilizador (sino no topo do menu e, se for importante, também por email):
/// coautoria por confirmar, ideia publicada, discussão terminada, decisão da CE, comentários.
/// </summary>
public class Notificacao : BaseAuditableEntity
{
    public int DestinatarioId { get; set; }

    public Utilizador Destinatario { get; set; } = null!;

    public string Titulo { get; set; } = string.Empty;

    public string Mensagem { get; set; } = string.Empty;

    /// <summary>Caminho no site (ex.: /inovacao/ideias/12).</summary>
    public string? Link { get; set; }

    public DateTimeOffset Data { get; set; }

    public bool Lida { get; set; }

    public bool EmailEnviado { get; set; }
}
