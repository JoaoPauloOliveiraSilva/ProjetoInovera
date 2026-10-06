namespace Innovera.Domain.Entities;

/// <summary>
/// Aviso gerado pelo job diário para prazos, validades e atividades do plano anual (Fluxo D).
/// </summary>
public class Alerta : BaseAuditableEntity
{
    public TipoAlerta Tipo { get; set; }

    public AlvoAlerta Alvo { get; set; }

    /// <summary>Id da entidade indicada em <see cref="Alvo"/>.</summary>
    public int AlvoId { get; set; }

    public int DestinatarioId { get; set; }

    public Utilizador Destinatario { get; set; } = null!;

    public string Mensagem { get; set; } = string.Empty;

    public DateOnly DataPrevista { get; set; }

    public bool Enviado { get; set; }

    public DateTimeOffset? EnviadoEm { get; set; }

    public bool Lido { get; set; }
}
