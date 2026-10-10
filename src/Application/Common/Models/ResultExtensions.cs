namespace Innovera.Application.Common.Models;

public static class ResultExtensions
{
    /// <summary>Transforma um erro do Identity numa mensagem para o utilizador (HTTP 400).</summary>
    public static void Garantir(this Result resultado)
    {
        if (!resultado.Succeeded)
        {
            throw new RegraDeNegocioException(string.Join(" ", resultado.Errors));
        }
    }
}
