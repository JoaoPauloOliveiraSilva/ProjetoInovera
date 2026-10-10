using Innovera.Application.Common.Models;

namespace Innovera.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<string?> GetUserNameAsync(string userId);

    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string policyName);

    Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password);

    Task<Result> DeleteUserAsync(string userId);

    Task<Result> AddToRoleAsync(string userId, string role);

    /// <summary>Verdadeiro se algum perfil do utilizador tem a permissão (o Administrador tem sempre todas).</summary>
    Task<bool> TemPermissaoAsync(string userId, string permissao);

    /// <summary>Todas as permissões do utilizador (junção das dos seus perfis).</summary>
    Task<IReadOnlyList<string>> PermissoesAsync(string userId);

    Task<IReadOnlyList<string>> PerfisAsync(string userId);
}
