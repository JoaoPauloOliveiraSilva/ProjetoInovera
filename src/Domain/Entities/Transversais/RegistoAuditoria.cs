namespace Innovera.Domain.Entities;

/// <summary>
/// Registo de auditoria transversal (ISO 56001), preenchido automaticamente em cada gravação.
/// Não herda de BaseAuditableEntity porque é ele próprio o registo.
/// </summary>
public class RegistoAuditoria : BaseEntity
{
    /// <summary>Nome da entidade alterada (ex.: "Ideia").</summary>
    public string Entidade { get; set; } = string.Empty;

    public string EntidadeId { get; set; } = string.Empty;

    public AcaoAuditoria Acao { get; set; }

    /// <summary>Valores antes da alteração (JSON).</summary>
    public string? ValoresAntes { get; set; }

    /// <summary>Valores depois da alteração (JSON).</summary>
    public string? ValoresDepois { get; set; }

    public string? UtilizadorId { get; set; }

    public DateTimeOffset Data { get; set; }
}
