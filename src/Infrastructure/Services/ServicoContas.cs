using System.Buffers.Text;
using System.Net;
using System.Security.Claims;
using System.Text;
using Innovera.Application.Common.Interfaces;
using Innovera.Application.Common.Models;
using Innovera.Domain.Constants;
using Innovera.Infrastructure.Data;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Innovera.Infrastructure.Services;

/// <summary>Contas, convites, passwords e perfis/permissões sobre o ASP.NET Core Identity.</summary>
public class ServicoContas : IContas
{
    /// <summary>Validade dos links enviados por email (convite e recuperação). Igual a DataProtectionTokenProviderOptions.</summary>
    public static readonly TimeSpan ValidadeLinks = TimeSpan.FromDays(3);

    // Conta desativada = bloqueada "para sempre" (LockoutEnd no fim dos tempos).
    private static readonly DateTimeOffset LimiteDesativada = new(9000, 1, 1, 0, 0, 0, TimeSpan.Zero);

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;
    private readonly IEmail _email;
    private readonly IConfiguration _configuration;

    public ServicoContas(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context, IEmail email, IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
        _email = email;
        _configuration = configuration;
    }

    // ---------- Contas ----------

    public async Task<IReadOnlyDictionary<string, ContaInfo>> ObterAsync(IEnumerable<string> identityIds, CancellationToken cancellationToken)
    {
        var ids = identityIds.Distinct().ToList();
        var contas = await _context.Users
            .Where(u => ids.Contains(u.Id))
            .Select(u => new { u.Id, u.EmailConfirmed, u.LockoutEnd })
            .ToListAsync(cancellationToken);
        var perfis = await (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ids.Contains(ur.UserId)
                            select new { ur.UserId, r.Name }).ToListAsync(cancellationToken);

        return contas.ToDictionary(
            c => c.Id,
            c => new ContaInfo(
                c.Id,
                Estado(c.EmailConfirmed, c.LockoutEnd),
                perfis.Where(p => p.UserId == c.Id).Select(p => p.Name!).OrderBy(n => n).ToList()));
    }

    public async Task<(Result Resultado, string IdentityId)> CriarAsync(string email, IReadOnlyCollection<string> perfis)
    {
        var erro = await ValidarPerfisAsync(perfis);
        if (erro is not null)
        {
            return (erro, string.Empty);
        }

        var conta = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = false, LockoutEnabled = true };
        var resultado = await _userManager.CreateAsync(conta);
        if (!resultado.Succeeded)
        {
            return (resultado.ToApplicationResult(), string.Empty);
        }

        resultado = await _userManager.AddToRolesAsync(conta, perfis.Distinct());
        return (resultado.ToApplicationResult(), conta.Id);
    }

    public async Task<Result> DefinirPerfisAsync(string identityId, IReadOnlyCollection<string> perfis)
    {
        var erro = await ValidarPerfisAsync(perfis);
        if (erro is not null)
        {
            return erro;
        }

        var conta = await _userManager.FindByIdAsync(identityId);
        if (conta is null)
        {
            return Falha("A conta não existe.");
        }

        var atuais = await _userManager.GetRolesAsync(conta);
        if (atuais.Contains(Roles.Administrador) && !perfis.Contains(Roles.Administrador) && !await HaOutroAdministradorAsync(conta.Id))
        {
            return Falha("Tem de existir pelo menos um Administrador ativo.");
        }

        var resultado = await _userManager.RemoveFromRolesAsync(conta, atuais.Except(perfis));
        if (resultado.Succeeded)
        {
            resultado = await _userManager.AddToRolesAsync(conta, perfis.Distinct().Except(atuais));
        }

        return resultado.ToApplicationResult();
    }

    public async Task<Result> DesativarAsync(string identityId)
    {
        var conta = await _userManager.FindByIdAsync(identityId);
        if (conta is null)
        {
            return Falha("A conta não existe.");
        }

        if (await _userManager.IsInRoleAsync(conta, Roles.Administrador) && !await HaOutroAdministradorAsync(conta.Id))
        {
            return Falha("Tem de existir pelo menos um Administrador ativo.");
        }

        await _userManager.SetLockoutEnabledAsync(conta, true);
        var resultado = await _userManager.SetLockoutEndDateAsync(conta, DateTimeOffset.MaxValue);
        if (resultado.Succeeded)
        {
            // Termina as sessões abertas (o cookie deixa de ser aceite).
            resultado = await _userManager.UpdateSecurityStampAsync(conta);
        }

        return resultado.ToApplicationResult();
    }

    public async Task<Result> AtivarAsync(string identityId)
    {
        var conta = await _userManager.FindByIdAsync(identityId);
        if (conta is null)
        {
            return Falha("A conta não existe.");
        }

        var resultado = await _userManager.SetLockoutEndDateAsync(conta, null);
        if (resultado.Succeeded)
        {
            resultado = await _userManager.ResetAccessFailedCountAsync(conta);
        }

        return resultado.ToApplicationResult();
    }

    // ---------- Convites e passwords ----------

    public async Task EnviarConviteAsync(string identityId, string nome, CancellationToken cancellationToken)
    {
        var conta = await _userManager.FindByIdAsync(identityId) ?? throw new InvalidOperationException("Conta inexistente.");
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(conta);
        var link = Link("convite", conta.Id, token);

        await _email.EnviarAsync(Mensagem(conta.Email!, nome,
            "Convite para a plataforma de Inovação",
            "foi convidado para a plataforma de gestão da Inovação da dstelecom.",
            "Para entrar, defina a sua password neste link:",
            "definir password", link), cancellationToken);
    }

    public async Task<(Result Resultado, string? Email)> AceitarConviteAsync(string identityId, string token, string password)
    {
        var conta = await _userManager.FindByIdAsync(identityId);
        if (conta is null)
        {
            return (Result.Failure([_userManager.ErrorDescriber.InvalidToken().Description]), null);
        }

        if (EstaDesativada(conta.LockoutEnd))
        {
            return (Falha("Esta conta está desativada."), null);
        }

        if (conta.EmailConfirmed && await _userManager.HasPasswordAsync(conta))
        {
            return (Falha("Este convite já foi usado. Entre com a sua password ou use \"esqueci-me da password\"."), null);
        }

        // A password é validada antes de usar o link, para o link continuar a servir se a password for recusada.
        var erros = await ValidarPasswordAsync(conta, password);
        if (erros is not null)
        {
            return (erros, null);
        }

        var resultado = await _userManager.ConfirmEmailAsync(conta, Descodificar(token));
        if (!resultado.Succeeded)
        {
            return (resultado.ToApplicationResult(), null);
        }

        resultado = await _userManager.AddPasswordAsync(conta, password);
        return (resultado.ToApplicationResult(), conta.Email);
    }

    public async Task PedirRecuperacaoAsync(string email, CancellationToken cancellationToken)
    {
        var conta = await _userManager.FindByEmailAsync(email);
        if (conta is null || EstaDesativada(conta.LockoutEnd))
        {
            return;
        }

        var nome = await _context.Utilizadores.Where(u => u.IdentityId == conta.Id).Select(u => u.Nome).FirstOrDefaultAsync(cancellationToken)
            ?? email;

        if (!conta.EmailConfirmed)
        {
            // Ainda não aceitou o convite: manda o convite outra vez.
            await EnviarConviteAsync(conta.Id, nome, cancellationToken);
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(conta);
        await _email.EnviarAsync(Mensagem(conta.Email!, nome,
            "Recuperar a password",
            "recebemos um pedido para mudar a sua password na plataforma de Inovação.",
            "Para escolher uma password nova, use este link (se não foi você, ignore este email):",
            "escolher password nova", Link("recuperar", conta.Id, token)), cancellationToken);
    }

    public async Task<(Result Resultado, string? Email)> RedefinirPasswordAsync(string identityId, string token, string password)
    {
        var conta = await _userManager.FindByIdAsync(identityId);
        if (conta is null)
        {
            return (Result.Failure([_userManager.ErrorDescriber.InvalidToken().Description]), null);
        }

        if (EstaDesativada(conta.LockoutEnd))
        {
            return (Falha("Esta conta está desativada."), null);
        }

        var resultado = await _userManager.ResetPasswordAsync(conta, Descodificar(token), password);
        if (resultado.Succeeded)
        {
            await _userManager.ResetAccessFailedCountAsync(conta);
            await _userManager.SetLockoutEndDateAsync(conta, null);
        }

        return (resultado.ToApplicationResult(), conta.Email);
    }

    // ---------- Perfis e permissões ----------

    public async Task<IReadOnlyList<PerfilInfo>> ListarPerfisAsync()
    {
        var perfis = await _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        var claims = await _context.RoleClaims
            .Where(c => c.ClaimType == Permissoes.TipoClaim)
            .Select(c => new { c.RoleId, c.ClaimValue })
            .ToListAsync();
        var contagens = await _context.UserRoles
            .GroupBy(ur => ur.RoleId)
            .Select(g => new { RoleId = g.Key, Total = g.Count() })
            .ToListAsync();
        var todas = Permissoes.Todas.Select(p => p.Codigo).ToList();

        return perfis
            .Select(p =>
            {
                var admin = p.Name == Roles.Administrador;
                var permissoes = admin
                    ? todas
                    : todas.Where(c => claims.Any(cl => cl.RoleId == p.Id && cl.ClaimValue == c)).ToList();
                return new PerfilInfo(p.Name!, Roles.DoSistema.Contains(p.Name!), admin, permissoes,
                    contagens.FirstOrDefault(c => c.RoleId == p.Id)?.Total ?? 0);
            })
            .OrderBy(p => p.DoSistema ? Roles.DoSistema.ToList().IndexOf(p.Nome) : int.MaxValue)
            .ThenBy(p => p.Nome)
            .ToList();
    }

    public async Task<Result> CriarPerfilAsync(string nome)
    {
        if (await _roleManager.RoleExistsAsync(nome))
        {
            return Falha($"Já existe o perfil {nome}.");
        }

        return (await _roleManager.CreateAsync(new IdentityRole(nome))).ToApplicationResult();
    }

    public async Task<Result> DefinirPermissoesAsync(string perfil, IReadOnlyCollection<string> permissoes)
    {
        var role = await _roleManager.FindByNameAsync(perfil);
        if (role is null)
        {
            return Falha($"O perfil {perfil} não existe.");
        }

        if (role.Name == Roles.Administrador)
        {
            return Falha("O Administrador tem sempre todas as permissões.");
        }

        var atuais = (await _roleManager.GetClaimsAsync(role)).Where(c => c.Type == Permissoes.TipoClaim).ToList();
        foreach (var claim in atuais.Where(c => !permissoes.Contains(c.Value)))
        {
            var resultado = await _roleManager.RemoveClaimAsync(role, claim);
            if (!resultado.Succeeded)
            {
                return resultado.ToApplicationResult();
            }
        }

        foreach (var permissao in permissoes.Where(p => atuais.All(c => c.Value != p)))
        {
            var resultado = await _roleManager.AddClaimAsync(role, new Claim(Permissoes.TipoClaim, permissao));
            if (!resultado.Succeeded)
            {
                return resultado.ToApplicationResult();
            }
        }

        return Result.Success();
    }

    public async Task<Result> ApagarPerfilAsync(string nome)
    {
        if (Roles.DoSistema.Contains(nome))
        {
            return Falha($"O perfil {nome} faz parte da plataforma e não pode ser apagado.");
        }

        var role = await _roleManager.FindByNameAsync(nome);
        if (role is null)
        {
            return Falha($"O perfil {nome} não existe.");
        }

        var total = await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
        if (total > 0)
        {
            return Falha($"Há {total} utilizador(es) com o perfil {nome}. Mude-lhes o perfil primeiro.");
        }

        return (await _roleManager.DeleteAsync(role)).ToApplicationResult();
    }

    // ---------- Auxiliares ----------

    private static EstadoConta Estado(bool emailConfirmado, DateTimeOffset? lockoutEnd)
        => EstaDesativada(lockoutEnd) ? EstadoConta.Desativada
            : emailConfirmado ? EstadoConta.Ativa
            : EstadoConta.ConvitePendente;

    private static bool EstaDesativada(DateTimeOffset? lockoutEnd) => lockoutEnd is not null && lockoutEnd >= LimiteDesativada;

    private static Result Falha(string mensagem) => Result.Failure([mensagem]);

    private async Task<Result?> ValidarPerfisAsync(IReadOnlyCollection<string> perfis)
    {
        if (perfis.Count == 0)
        {
            return Falha("Escolha pelo menos um perfil.");
        }

        foreach (var perfil in perfis)
        {
            if (!await _roleManager.RoleExistsAsync(perfil))
            {
                return Falha($"O perfil {perfil} não existe.");
            }
        }

        return null;
    }

    private async Task<Result?> ValidarPasswordAsync(ApplicationUser conta, string password)
    {
        var erros = new List<string>();
        foreach (var validador in _userManager.PasswordValidators)
        {
            var resultado = await validador.ValidateAsync(_userManager, conta, password);
            erros.AddRange(resultado.Errors.Select(e => e.Description));
        }

        return erros.Count == 0 ? null : Result.Failure(erros);
    }

    private async Task<bool> HaOutroAdministradorAsync(string excetoId)
    {
        var administradores = await _userManager.GetUsersInRoleAsync(Roles.Administrador);
        return administradores.Any(a => a.Id != excetoId && a.EmailConfirmed && !EstaDesativada(a.LockoutEnd));
    }

    private string Link(string tipo, string identityId, string token)
    {
        var site = (_configuration["Email:UrlSite"] ?? "http://localhost:5173").TrimEnd('/');
        var codigo = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(token));
        return $"{site}/definir-password?tipo={tipo}&u={Uri.EscapeDataString(identityId)}&t={codigo}";
    }

    private static string Descodificar(string codigo)
    {
        try
        {
            return Encoding.UTF8.GetString(Base64Url.DecodeFromChars(codigo));
        }
        catch (FormatException)
        {
            return string.Empty;
        }
    }

    private static MensagemEmail Mensagem(string para, string nome, string assunto, string frase, string instrucao, string botao, string link)
    {
        var dias = (int)ValidadeLinks.TotalDays;
        var texto = $"Olá {nome},\n\n{char.ToUpper(frase[0])}{frase[1..]}\n{instrucao}\n\n{link}\n\nO link é válido durante {dias} dias.\n\nEquipa de Inovação";
        var n = WebUtility.HtmlEncode(nome);
        var l = WebUtility.HtmlEncode(link);
        var html = $"""
            <div style="font-family:Arial,sans-serif;color:#222;max-width:560px">
              <div style="background:#006d6f;color:#fff;padding:18px 24px;font-size:18px;font-weight:bold">Inovação · dstelecom</div>
              <div style="padding:24px;background:#f3f3f3">
                <p>Olá {n},</p>
                <p>{WebUtility.HtmlEncode(char.ToUpper(frase[0]) + frase[1..])}</p>
                <p>{WebUtility.HtmlEncode(instrucao)}</p>
                <p style="margin:28px 0"><a href="{l}" style="background:#c8102e;color:#fff;padding:12px 22px;border-radius:22px;text-decoration:none;font-weight:bold">{WebUtility.HtmlEncode(botao)}</a></p>
                <p style="font-size:12px;color:#777">O link é válido durante {dias} dias. Se o botão não funcionar, copie este endereço para o browser:<br>{l}</p>
              </div>
            </div>
            """;
        return new MensagemEmail(para, nome, assunto, html, texto);
    }
}
