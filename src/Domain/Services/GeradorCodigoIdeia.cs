namespace Innovera.Domain.Services;

/// <summary>
/// Gera o código interno das ideias (AA01, AA02 … AA99, AB01 … ZZ99) a partir de um número sequencial
/// (na base de dados vem de uma SEQUENCE). Letras = (n − 1) div 99 em base 26; número = (n − 1) mod 99 + 1.
/// </summary>
public static class GeradorCodigoIdeia
{
    public const int NumerosPorPrefixo = 99;

    /// <summary>Último número representável (ZZ99).</summary>
    public const int Maximo = 26 * 26 * NumerosPorPrefixo;

    public static string Gerar(int numero)
    {
        if (numero < 1 || numero > Maximo)
        {
            throw new ArgumentOutOfRangeException(nameof(numero), numero, $"O número tem de estar entre 1 e {Maximo}.");
        }

        var indice = numero - 1;
        var prefixo = indice / NumerosPorPrefixo;
        var primeiraLetra = (char)('A' + (prefixo / 26));
        var segundaLetra = (char)('A' + (prefixo % 26));
        var sufixo = (indice % NumerosPorPrefixo) + 1;

        return $"{primeiraLetra}{segundaLetra}{sufixo:D2}";
    }
}
