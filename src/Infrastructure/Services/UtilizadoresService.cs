using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Constants;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Infrastructure.Data;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Innovera.Infrastructure.Services;

public class UtilizadoresService : IUtilizadores
{
    private readonly ApplicationDbContext _context;
    private readonly IUser _user;
    private readonly UserManager<ApplicationUser> _userManager;
    private Utilizador? _atual;

    public UtilizadoresService(ApplicationDbContext context, IUser user, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _user = user;
        _userManager = userManager;
    }

    public bool EAdministrador => _user.Roles?.Contains(Roles.Administrador) ?? false;

    public async Task<Utilizador> AtualAsync(CancellationToken cancellationToken)
    {
        if (_atual is not null)
        {
            return _atual;
        }

        var identityId = _user.Id ?? throw new UnauthorizedAccessException();

        _atual = await _context.Utilizadores.FirstOrDefaultAsync(u => u.IdentityId == identityId, cancellationToken);
        if (_atual is not null)
        {
            return _atual;
        }

        // Primeira vez que esta conta usa a plataforma: cria o perfil.
        var conta = await _userManager.FindByIdAsync(identityId) ?? throw new UnauthorizedAccessException();
        var email = conta.Email ?? conta.UserName ?? identityId;
        _atual = new Utilizador
        {
            IdentityId = identityId,
            Email = email,
            Nome = email.Split('@')[0],
            Perfil = EAdministrador ? PerfilUtilizador.Administrador : PerfilUtilizador.Trabalhador,
        };
        _context.Utilizadores.Add(_atual);
        await _context.SaveChangesAsync(cancellationToken);

        return _atual;
    }
}
