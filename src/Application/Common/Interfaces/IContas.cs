using Innovera.Application.Common.Models;

namespace Innovera.Application.Common.Interfaces;

public enum EstadoConta
{
    /// <summary>Convite enviado, ainda sem password definida.</summary>
    ConvitePendente,
    Ativa,
    Desativada
}

public record ContaInfo(string IdentityId, EstadoConta Estado, IReadOnlyList<string> Perfis);

public record PerfilInfo(string Nome, bool DoSistema, bool TodasAsPermissoes, IReadOnlyList<string> Permissoes, int Utilizadores);

/// <summary>
/// Contas e perfis no ASP.NET Core Identity: convites por email, passwords, ativar/desativar,
/// perfis e as permissões de cada perfil.
/// </summary>
public interface IContas
{
    Task<IReadOnlyDictionary<string, ContaInfo>> ObterAsync(IEnumerable<string> identityIds, CancellationToken cancellationToken);

    /// <summary>Cria a conta sem password (a pessoa define-a pelo link do convite).</summary>
    Task<(Result Resultado, string IdentityId)> CriarAsync(string email, IReadOnlyCollection<string> perfis);

    Task<Result> DefinirPerfisAsync(string identityId, IReadOnlyCollection<string> perfis);

    Task<Result> DesativarAsync(string identityId);

    Task<Result> AtivarAsync(string identityId);

    /// <summary>Envia (ou reenvia) o email de convite com o link para definir a password.</summary>
    Task EnviarConviteAsync(string identityId, string nome, CancellationToken cancellationToken);

    /// <summary>Aceita o convite: confirma o email e define a password. Devolve o email da conta.</summary>
    Task<(Result Resultado, string? Email)> AceitarConviteAsync(string identityId, string token, string password);

    /// <summary>"Esqueci-me da password": envia o link (ou o convite, se ainda não foi aceite). Não diz se o email existe.</summary>
    Task PedirRecuperacaoAsync(string email, CancellationToken cancellationToken);

    Task<(Result Resultado, string? Email)> RedefinirPasswordAsync(string identityId, string token, string password);

    Task<IReadOnlyList<PerfilInfo>> ListarPerfisAsync();

    Task<Result> CriarPerfilAsync(string nome);

    Task<Result> DefinirPermissoesAsync(string perfil, IReadOnlyCollection<string> permissoes);

    Task<Result> ApagarPerfilAsync(string nome);
}
