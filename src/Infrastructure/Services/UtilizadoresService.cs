using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Entities;
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
    private readonly IIdentityService _identityService;
    private Utilizador? _atual;
    private IReadOnlyList<string>? _permissoes;

    public UtilizadoresService(ApplicationDbContext context, IUser user, UserManager<ApplicationUser> userManager, IIdentityService identityService)
    {
        _context = context;
        _user = user;
        _userManager = userManager;
        _identityService = identityService;
    }

    public async Task<bool> TemPermissaoAsync(string permissao, CancellationToken cancellationToken)
    {
        if (_user.Id is null)
        {
            return false;
        }

        // Uma vez por pedido HTTP (o serviço é scoped).
        _permissoes ??= await _identityService.PermissoesAsync(_user.Id);
        return _permissoes.Contains(permissao);
    }

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

        // Conta sem registo de Utilizador (ex.: criada fora do backoffice): cria-o agora.
        var conta = await _userManager.FindByIdAsync(identityId) ?? throw new UnauthorizedAccessException();
        var email = conta.Email ?? conta.UserName ?? identityId;
        _atual = new Utilizador
        {
            IdentityId = identityId,
            Email = email,
            Nome = email.Split('@')[0],
        };
        _context.Utilizadores.Add(_atual);
        await _context.SaveChangesAsync(cancellationToken);

        return _atual;
    }
}
