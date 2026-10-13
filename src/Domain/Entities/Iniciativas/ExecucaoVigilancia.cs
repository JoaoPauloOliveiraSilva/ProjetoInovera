namespace Innovera.Domain.Entities;

/// <summary>Uma execução (prevista e realizada) de uma vigilância.</summary>
public class ExecucaoVigilancia : BaseAuditableEntity
{
    public int VigilanciaId { get; set; }

    public Vigilancia Vigilancia { get; set; } = null!;

    public DateOnly DataPrevista { get; set; }

    public DateOnly? DataRealizada { get; set; }

    public string? Resultado { get; set; }

    /// <summary>Link para o relatório da vigilância (OneDrive ou o ficheiro carregado na plataforma).</summary>
    public string? LinkResultado { get; set; }

    /// <summary>Relatório carregado na plataforma (gerado pela tarefa automática da vigilância).</summary>
    public string? NomeFicheiro { get; set; }

    public string? TipoConteudo { get; set; }

    public long? TamanhoBytes { get; set; }

    public string? ChaveArmazenamento { get; set; }

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
