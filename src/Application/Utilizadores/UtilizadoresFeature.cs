namespace Innovera.Application.Utilizadores;

/// <summary>Utilizador com sessão iniciada: o frontend usa as permissões para mostrar menus e botões.</summary>
public record UtilizadorAtualDto(int Id, string Nome, string Email, string? Empresa, IReadOnlyList<string> Perfis, IReadOnlyList<string> Permissoes);

public record UtilizadorResumoDto(int Id, string Nome, string? Empresa);

/// <summary>Dados do utilizador com sessão iniciada (cria o perfil na primeira vez).</summary>
[Authorize]
public record ObterUtilizadorAtualQuery : IRequest<UtilizadorAtualDto>;

public class ObterUtilizadorAtualQueryHandler(IUtilizadores utilizadores, IIdentityService identityService)
    : IRequestHandler<ObterUtilizadorAtualQuery, UtilizadorAtualDto>
{
    public async Task<UtilizadorAtualDto> Handle(ObterUtilizadorAtualQuery request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var perfis = await identityService.PerfisAsync(eu.IdentityId);
        var permissoes = await identityService.PermissoesAsync(eu.IdentityId);
        return new UtilizadorAtualDto(eu.Id, eu.Nome, eu.Email, eu.Empresa, perfis, permissoes);
    }
}

/// <summary>Pesquisa de colegas (para indicar coautores de uma ideia).</summary>
[Authorize]
public record PesquisarUtilizadoresQuery(string? Texto) : IRequest<IReadOnlyList<UtilizadorResumoDto>>;

public class PesquisarUtilizadoresQueryHandler(IApplicationDbContext context) : IRequestHandler<PesquisarUtilizadoresQuery, IReadOnlyList<UtilizadorResumoDto>>
{
    public async Task<IReadOnlyList<UtilizadorResumoDto>> Handle(PesquisarUtilizadoresQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.Utilizadores.AsNoTracking().Where(u => u.Ativo);
        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(u => u.Nome.ToLower().Contains(texto) || u.Email.ToLower().Contains(texto));
        }

        return await consulta
            .OrderBy(u => u.Nome)
            .Take(20)
            .Select(u => new UtilizadorResumoDto(u.Id, u.Nome, u.Empresa))
            .ToListAsync(cancellationToken);
    }
}
