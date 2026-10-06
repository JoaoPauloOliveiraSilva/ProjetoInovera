namespace Innovera.Domain.Entities;

/// <summary>Entrada do histórico do ponto de situação (RF03).</summary>
public class NotaPontoSituacao : BaseAuditableEntity
{
    public int IniciativaId { get; set; }

    public Iniciativa Iniciativa { get; set; } = null!;

    public DateTimeOffset Data { get; set; }

    public string Texto { get; set; } = string.Empty;

    public int? AutorId { get; set; }

    public Utilizador? Autor { get; set; }
}
