namespace Innovera.Domain.Entities;

/// <summary>Tarefa da WBS com datas planeadas e reais (separador Planeamento).</summary>
public class TarefaWbs : BaseAuditableEntity
{
    public int CharterId { get; set; }

    public ProjectCharter Charter { get; set; } = null!;

    /// <summary>Código hierárquico (ex.: "1.0", "1.1").</summary>
    public string CodigoWbs { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public TipoTarefa Tipo { get; set; }

    public int? DependeDeId { get; set; }

    public TarefaWbs? DependeDe { get; set; }

    /// <summary>Milestone a que a tarefa pertence (ex.: M1).</summary>
    public int? MilestoneId { get; set; }

    public Entrega? Milestone { get; set; }

    public EstadoTrabalho Estado { get; set; } = EstadoTrabalho.Planeado;

    public DateOnly? InicioPlaneado { get; set; }

    public DateOnly? FimPlaneado { get; set; }

    public DateOnly? InicioReal { get; set; }

    public DateOnly? FimReal { get; set; }

    public int? ResponsavelId { get; set; }

    public Utilizador? Responsavel { get; set; }

    /// <summary>Atrasada se passou o fim planeado e ainda não terminou.</summary>
    public bool EstaAtrasada(DateOnly hoje)
        => FimReal is null && FimPlaneado.HasValue && FimPlaneado.Value < hoje;
}
