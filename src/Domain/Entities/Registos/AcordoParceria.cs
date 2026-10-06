namespace Innovera.Domain.Entities;

/// <summary>Acordo de parceria (Mod.235), com datas normalizadas e estado.</summary>
public class AcordoParceria : BaseAuditableEntity
{
    public int ParceiroId { get; set; }

    public Parceiro Parceiro { get; set; } = null!;

    /// <summary>Descrição da parceria.</summary>
    public string Descricao { get; set; } = string.Empty;

    public bool ProtocoloEstabelecido { get; set; }

    public TipoProtocolo? TipoProtocolo { get; set; }

    public DateOnly? DataInicio { get; set; }

    /// <summary>Vazio = sem fim.</summary>
    public DateOnly? DataFim { get; set; }

    public bool IncluiGestaoPropriedadeIntelectual { get; set; }

    public EstadoAcordo Estado { get; set; } = EstadoAcordo.EmCurso;

    public string? Observacoes { get; set; }

    /// <summary>Link OneDrive para o protocolo assinado.</summary>
    public string? LinkDocumento { get; set; }
}
