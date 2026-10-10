using Innovera.Application.Common.Interfaces;
using Innovera.Application.Common.Models;
using Innovera.Domain.Constants;
using Innovera.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Innovera.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserClaimsPrincipalFactory<ApplicationUser> _userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService;
    private readonly ApplicationDbContext _context;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        IUserClaimsPrincipalFactory<ApplicationUser> userClaimsPrincipalFactory,
        IAuthorizationService authorizationService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
        _authorizationService = authorizationService;
        _context = context;
    }

    public async Task<IReadOnlyList<string>> PerfisAsync(string userId)
        => await (from ur in _context.UserRoles
                  join r in _context.Roles on ur.RoleId equals r.Id
                  where ur.UserId == userId
                  orderby r.Name
                  select r.Name!).ToListAsync();

    public async Task<IReadOnlyList<string>> ContasComPermissaoAsync(string permissao)
    {
        var perfis = await _context.Roles
            .Where(r => r.Name == Roles.Administrador
                || _context.RoleClaims.Any(c => c.RoleId == r.Id && c.ClaimType == Permissoes.TipoClaim && c.ClaimValue == permissao))
            .Select(r => r.Id)
            .ToListAsync();

        return await _context.UserRoles
            .Where(ur => perfis.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();
    }

    public async Task<bool> TemPermissaoAsync(string userId, string permissao)
        => (await PermissoesAsync(userId)).Contains(permissao);

    /// <summary>Lido sempre da base de dados: uma mudança no backoffice conta logo, sem voltar a entrar.</summary>
    public async Task<IReadOnlyList<string>> PermissoesAsync(string userId)
    {
        var perfis = await (from ur in _context.UserRoles
                            join r in _context.Roles on ur.RoleId equals r.Id
                            where ur.UserId == userId
                            select new { r.Id, r.Name }).ToListAsync();

        if (perfis.Any(p => p.Name == Roles.Administrador))
        {
            return Permissoes.Todas.Select(p => p.Codigo).ToList();
        }

        var ids = perfis.Select(p => p.Id).ToList();
        return await _context.RoleClaims
            .Where(c => ids.Contains(c.RoleId) && c.ClaimType == Permissoes.TipoClaim && c.ClaimValue != null)
            .Select(c => c.ClaimValue!)
            .Distinct()
            .ToListAsync();
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<(Result Result, string UserId)> CreateUserAsync(string userName, string password)
    {
        var user = new ApplicationUser
        {
            UserName = userName,
            Email = userName,
        };

        var result = await _userManager.CreateAsync(user, password);

        return (result.ToApplicationResult(), user.Id);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }

    public async Task<bool> AuthorizeAsync(string userId, string policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return false;
        }

        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);

        var result = await _authorizationService.AuthorizeAsync(principal, policyName);

        return result.Succeeded;
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null ? await DeleteUserAsync(user) : Result.Success();
    }

    public async Task<Result> AddToRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure(["Utilizador não encontrado."]);
        }

        var result = await _userManager.AddToRoleAsync(user, role);
        return result.ToApplicationResult();
    }

    public async Task<Result> DeleteUserAsync(ApplicationUser user)
    {
        var result = await _userManager.DeleteAsync(user);

        return result.ToApplicationResult();
    }
}
