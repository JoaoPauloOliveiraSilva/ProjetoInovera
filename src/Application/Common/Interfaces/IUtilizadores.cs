namespace Innovera.Application.Common.Interfaces;

/// <summary>O utilizador com sessão iniciada, já como entidade do domínio.</summary>
public interface IUtilizadores
{
    /// <summary>Devolve o utilizador atual (cria o registo na primeira vez que a conta usa a plataforma).</summary>
    Task<Utilizador> AtualAsync(CancellationToken cancellationToken);

    /// <summary>Verdadeiro se o utilizador atual tem a permissão (ver <c>Permissoes</c>).</summary>
    Task<bool> TemPermissaoAsync(string permissao, CancellationToken cancellationToken);
}
