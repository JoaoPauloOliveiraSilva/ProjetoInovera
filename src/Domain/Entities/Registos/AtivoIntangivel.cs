namespace Innovera.Domain.Entities;

/// <summary>
/// Ativo intangível (Mod.236): tipo normalizado (patente/design/marca, KPIs 29–31) separado da
/// entidade de registo (INPI, EUIPO…). A validade e as atividades geram alertas.
/// </summary>
public class AtivoIntangivel : BaseAuditableEntity
{
    public DateOnly Data { get; set; }

    public TipoAtivoIntangivel Tipo { get; set; }

    /// <summary>Entidade de registo (ex.: INPI, EUIPO).</summary>
    public string EntidadeRegisto { get; set; } = string.Empty;

    public string Designacao { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public string? NumeroPedido { get; set; }

    /// <summary>Inventores / autores.</summary>
    public string? Autores { get; set; }

    public DateOnly? DataRegisto { get; set; }

    public DateOnly? DataValidade { get; set; }

    public string? Ambito { get; set; }

    /// <summary>Situação atual (ex.: "Ativa").</summary>
    public string? Situacao { get; set; }

    public decimal? Custo { get; set; }

    public string? PerspetivaValor { get; set; }

    public string? NumeroProcesso { get; set; }

    public string? Observacoes { get; set; }

    /// <summary>Modelo de gestão: atividades, quem, quando.</summary>
    public ICollection<AtividadeGestao> Atividades { get; set; } = new List<AtividadeGestao>();
}
