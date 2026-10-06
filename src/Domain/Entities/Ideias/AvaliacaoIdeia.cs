namespace Innovera.Domain.Entities;

/// <summary>
/// Avaliação da Comissão Executiva (CE): 5 variáveis de 0 a 4, nota com pesos configuráveis
/// e decisão automática (nota ≥ 2 aprova). Um valor nulo corresponde a N.A.
/// </summary>
public class AvaliacaoIdeia : BaseAuditableEntity
{
    public const decimal LimiarAprovacao = 2m;

    public int IdeiaId { get; set; }

    public Ideia Ideia { get; set; } = null!;

    public EscalaDeAvaliacao? Custo { get; set; }

    public EscalaDeAvaliacao? Enquadramento { get; set; }

    public EscalaDeAvaliacao? Beneficio { get; set; }

    public EscalaDeAvaliacao? AdequacaoTecnica { get; set; }

    public EscalaDeAvaliacao? Incerteza { get; set; }

    /// <summary>Nota final (TOTAL do Mod.246).</summary>
    public decimal? Nota { get; private set; }

    public bool? Aprovada { get; private set; }

    /// <summary>Data da reunião mensal da CE (Mod.69).</summary>
    public DateOnly? DataReuniaoCE { get; set; }

    public string? Observacoes { get; set; }

    /// <summary>
    /// Calcula a nota e a decisão. Se faltar alguma variável (N.A.) a nota fica vazia, como no Mod.246.
    /// </summary>
    public decimal? CalcularNota(PesosAvaliacao pesos)
    {
        ArgumentNullException.ThrowIfNull(pesos);

        if (Custo is null || Enquadramento is null || Beneficio is null || AdequacaoTecnica is null || Incerteza is null)
        {
            Nota = null;
            Aprovada = null;
            return null;
        }

        Nota = (pesos.Custo * (int)Custo.Value)
             + (pesos.Enquadramento * (int)Enquadramento.Value)
             + (pesos.Beneficio * (int)Beneficio.Value)
             + (pesos.AdequacaoTecnica * (int)AdequacaoTecnica.Value)
             + (pesos.Incerteza * (int)Incerteza.Value);

        Aprovada = Nota >= LimiarAprovacao;
        return Nota;
    }
}
