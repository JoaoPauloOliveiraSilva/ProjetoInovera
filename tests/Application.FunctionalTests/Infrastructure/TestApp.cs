using Innovera.Domain.Constants;
using Innovera.Infrastructure.Data;
using Innovera.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Innovera.Application.FunctionalTests.Infrastructure;

public static class TestApp
{
    private static string? _userId;
    private static List<string>? _roles;

    public static async Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        return await mediator.Send(request);
    }

    public static async Task SendAsync(IBaseRequest request)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var mediator = scope.ServiceProvider.GetRequiredService<ISender>();

        await mediator.Send(request);
    }

    public static string? GetUserId() => _userId;

    public static List<string>? GetRoles() => _roles;

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", "Testing1234!", []);
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", "Administrator1234!", [Roles.Administrador]);
    }

    public static async Task<string> RunAsEquipaInovacaoAsync()
    {
        return await RunAsUserAsync("equipa@local", "Equipa1234!", [Roles.EquipaInovacao]);
    }

    public static async Task<string> RunAsUserAsync(string userName, string password, string[] roles)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // O mesmo teste pode "voltar" a ser um utilizador que já usou antes.
        var existente = await userManager.FindByNameAsync(userName);
        if (existente is not null)
        {
            _userId = existente.Id;
            _roles = [.. await userManager.GetRolesAsync(existente)];
            return _userId;
        }

        var user = new ApplicationUser { UserName = userName, Email = userName, EmailConfirmed = true };

        var result = await userManager.CreateAsync(user, password);

        if (roles.Length > 0)
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }

            await userManager.AddToRolesAsync(user, roles);
        }

        if (result.Succeeded)
        {
            _userId = user.Id;
            _roles = [..roles];
            return _userId;
        }

        var errors = string.Join(Environment.NewLine, result.ToApplicationResult().Errors);

        throw new Exception($"Unable to create {userName}.{Environment.NewLine}{errors}");
    }

    public static async Task ResetState()
    {
        if (FunctionalTestSetup.DbResetter is not null)
        {
            await FunctionalTestSetup.DbResetter.ResetAsync();
        }

        _userId = null;
        _roles = null;
        EmailFalso.Enviados.Clear();

        await GarantirPerfisDoSistemaAsync();
    }

    /// <summary>Como na aplicação: os 3 perfis do sistema com as permissões por omissão.</summary>
    private static async Task GarantirPerfisDoSistemaAsync()
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var nome in Roles.DoSistema)
        {
            var perfil = new IdentityRole(nome);
            await roleManager.CreateAsync(perfil);
            foreach (var permissao in Permissoes.PorOmissao.GetValueOrDefault(nome) ?? [])
            {
                await roleManager.AddClaimAsync(perfil, new System.Security.Claims.Claim(Permissoes.TipoClaim, permissao));
            }
        }
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }
}
