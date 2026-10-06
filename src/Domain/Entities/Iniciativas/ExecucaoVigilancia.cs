namespace Innovera.Domain.Entities;

/// <summary>Uma execução (prevista e realizada) de uma vigilância.</summary>
public class ExecucaoVigilancia : BaseAuditableEntity
{
    public int VigilanciaId { get; set; }

    public Vigilancia Vigilancia { get; set; } = null!;

    public DateOnly DataPrevista { get; set; }

    public DateOnly? DataRealizada { get; set; }

    public string? Resultado { get; set; }

    /// <summary>Link OneDrive para o relatório da vigilância.</summary>
    public string? LinkResultado { get; set; }

    /// <summary>Realizada até à data prevista (KPI 24 — % de vigilâncias no prazo).</summary>
    public bool NoPrazo => DataRealizada.HasValue && DataRealizada.Value <= DataPrevista;

    public void RegistarResultado(string resultado, DateOnly dataRealizada, string? link)
    {
        if (string.IsNullOrWhiteSpace(resultado))
        {
            throw new RegraDeNegocioException("O resultado da vigilância não pode estar vazio.");
        }

        Resultado = resultado.Trim();
        DataRealizada = dataRealizada;
        LinkResultado = link;

        AddDomainEvent(new ResultadoVigilanciaRegistadoEvent(this));
    }
}
