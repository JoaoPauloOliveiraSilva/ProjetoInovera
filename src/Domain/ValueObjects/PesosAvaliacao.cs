namespace Innovera.Domain.ValueObjects;

/// <summary>
/// Pesos das 5 variáveis da avaliação da CE. São configuráveis porque a especificação
/// fala em média aritmética e o Mod.246 usa média ponderada (questão em aberto).
/// </summary>
public class PesosAvaliacao : ValueObject
{
    public PesosAvaliacao(decimal custo, decimal enquadramento, decimal beneficio, decimal adequacaoTecnica, decimal incerteza)
    {
        if (custo < 0 || enquadramento < 0 || beneficio < 0 || adequacaoTecnica < 0 || incerteza < 0)
        {
            throw new RegraDeNegocioException("Os pesos da avaliação não podem ser negativos.");
        }

        if (custo + enquadramento + beneficio + adequacaoTecnica + incerteza != 1m)
        {
            throw new RegraDeNegocioException("A soma dos pesos da avaliação tem de ser 1 (100%).");
        }

        Custo = custo;
        Enquadramento = enquadramento;
        Beneficio = beneficio;
        AdequacaoTecnica = adequacaoTecnica;
        Incerteza = incerteza;
    }

    /// <summary>Pesos usados hoje no Excel Mod.246 (15/25/30/20/10).</summary>
    public static PesosAvaliacao Mod246 => new(0.15m, 0.25m, 0.30m, 0.20m, 0.10m);

    /// <summary>Média aritmética, como descrito na especificação (20% cada).</summary>
    public static PesosAvaliacao MediaSimples => new(0.20m, 0.20m, 0.20m, 0.20m, 0.20m);

    public decimal Custo { get; }

    public decimal Enquadramento { get; }

    public decimal Beneficio { get; }

    public decimal AdequacaoTecnica { get; }

    public decimal Incerteza { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Custo;
        yield return Enquadramento;
        yield return Beneficio;
        yield return AdequacaoTecnica;
        yield return Incerteza;
    }
}
