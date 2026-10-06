namespace Innovera.Domain.Entities;

/// <summary>
/// Entregável (E1…) ou milestone (M1…) do Project Charter, com meio de verificação e controlo.
/// </summary>
public class Entrega : BaseAuditableEntity
{
    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    public TipoEntrega Tipo { get; set; }

    /// <summary>Ex.: "E1", "M1".</summary>
    public string Codigo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public DateOnly? Prazo { get; set; }

    public EstadoTrabalho Estado { get; set; } = EstadoTrabalho.Planeado;

    /// <summary>Meio de verificação (ex.: "Relatório").</summary>
    public string? MeioVerificacao { get; set; }

    /// <summary>Controlo (ex.: "Foram gastas 840 h em RH e 3250 € em serviços, sem desvios").</summary>
    public string? Controlo { get; set; }

    public DateOnly? DataConclusao { get; set; }

    /// <summary>Link OneDrive para o relatório.</summary>
    public string? LinkRelatorio { get; set; }

    /// <summary>
    /// Conclui a entrega com o respetivo relatório; o evento cria a entrada no conhecimento codificado (Mod.237).
    /// </summary>
    public void RegistarRelatorio(string linkRelatorio, DateOnly dataConclusao)
    {
        if (string.IsNullOrWhiteSpace(linkRelatorio))
        {
            throw new RegraDeNegocioException("O link do relatório é obrigatório.");
        }

        LinkRelatorio = linkRelatorio.Trim();
        DataConclusao = dataConclusao;
        Estado = EstadoTrabalho.Concluido;

        AddDomainEvent(new RelatorioDeMilestoneRegistadoEvent(this));
    }
}
