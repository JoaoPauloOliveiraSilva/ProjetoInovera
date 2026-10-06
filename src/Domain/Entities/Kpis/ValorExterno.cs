namespace Innovera.Domain.Entities;

/// <summary>
/// Dado que não existe na plataforma e é introduzido à mão (RF09): n.º de trabalhadores, receitas, proveitos IDI.
/// </summary>
public class ValorExterno : BaseAuditableEntity
{
    public TipoValorExterno Tipo { get; set; }

    public int Ano { get; set; }

    public PeriodoKpi Periodo { get; set; } = PeriodoKpi.Anual;

    public decimal Valor { get; set; }

    public string? Observacoes { get; set; }
}
