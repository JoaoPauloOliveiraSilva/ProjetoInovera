using Innovera.Domain.Enums;

namespace Innovera.Domain.Entities;

public class AtivoIntangivel : BaseAuditableEntity
{
    public DateTime DataRegisto { get; set; }
    public TipoAtivoIntangivel Tipo { get; set; }
    public string? Designacao { get; set; }
    public string? Descricao { get; set; }

    public string? Inventores { get; set; }
    public string? EntidadeRegisto { get; set; }
    public DateTime? DataValidade { get; set; }
    public string? Ambito { get; set; }
    public string? Situacao { get; set; }

    public decimal Custo { get; set; }
    public string? PerspetivaValor { get; set; }
    public string? NumeroProcesso { get; set; }

    public ICollection<Atividade> Atividades { get; set; } = new List<Atividade>();
}
