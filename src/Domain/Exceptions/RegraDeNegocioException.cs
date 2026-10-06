namespace Innovera.Domain.Exceptions;

/// <summary>
/// Lançada quando uma operação viola uma regra de negócio do domínio
/// (ex.: avaliar uma ideia que ainda está em discussão).
/// </summary>
public class RegraDeNegocioException : Exception
{
    public RegraDeNegocioException(string message)
        : base(message)
    {
    }
}
