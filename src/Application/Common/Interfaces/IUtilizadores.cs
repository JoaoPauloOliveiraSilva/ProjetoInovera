using Innovera.Domain.Entities;

namespace Innovera.Application.Common.Interfaces;

/// <summary>Liga a conta autenticada (login) ao <see cref="Utilizador"/> do domínio.</summary>
public interface IUtilizadores
{
    /// <summary>Utilizador da conta autenticada; é criado na primeira vez. Lança UnauthorizedAccessException se não houver login.</summary>
    Task<Utilizador> AtualAsync(CancellationToken cancellationToken);

    bool EAdministrador { get; }
}
