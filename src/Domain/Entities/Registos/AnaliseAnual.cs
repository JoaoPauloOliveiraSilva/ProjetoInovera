namespace Innovera.Domain.Entities;

/// <summary>Análise anual de uma ferramenta/método (coluna "Análise &lt;ano&gt;" do Mod.241). Tipo próprio (owned).</summary>
public class AnaliseAnual
{
    public int Ano { get; set; }

    public string Analise { get; set; } = string.Empty;
}
