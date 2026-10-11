namespace Innovera.Application.Administracao;

public record RegistoAuditoriaDto(long Id, DateTimeOffset Data, string? Utilizador, AcaoAuditoria Acao, string Entidade, string EntidadeId,
    string? ValoresAntes, string? ValoresDepois);

/// <summary>Registo de auditoria (os 200 mais recentes que cumprem os filtros).</summary>
[Authorize(Permissao = Permissoes.Administracao.VerAuditoria)]
public record ListarAuditoriaQuery(string? Entidade, string? EntidadeId, string? Texto) : IRequest<IReadOnlyList<RegistoAuditoriaDto>>;

public class ListarAuditoriaQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarAuditoriaQuery, IReadOnlyList<RegistoAuditoriaDto>>
{
    public async Task<IReadOnlyList<RegistoAuditoriaDto>> Handle(ListarAuditoriaQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.RegistosAuditoria.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Entidade))
        {
            consulta = consulta.Where(r => r.Entidade == request.Entidade);
        }

        if (!string.IsNullOrWhiteSpace(request.EntidadeId))
        {
            consulta = consulta.Where(r => r.EntidadeId == request.EntidadeId);
        }

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            // Procura no nome de quem fez a alteração e nos valores (JSON) antes/depois.
            var texto = request.Texto.Trim().ToLower();
            var contasComNome = context.Utilizadores.Where(u => u.Nome.ToLower().Contains(texto)).Select(u => u.IdentityId);
            consulta = consulta.Where(r => (r.UtilizadorId != null && contasComNome.Contains(r.UtilizadorId))
                || (r.ValoresAntes != null && r.ValoresAntes.ToLower().Contains(texto))
                || (r.ValoresDepois != null && r.ValoresDepois.ToLower().Contains(texto)));
        }

        var registos = await consulta
            .OrderByDescending(r => r.Data).ThenByDescending(r => r.Id)
            .Take(200)
            .ToListAsync(cancellationToken);

        var contas = registos.Where(r => r.UtilizadorId != null).Select(r => r.UtilizadorId!).Distinct().ToList();
        var nomes = await context.Utilizadores
            .Where(u => contas.Contains(u.IdentityId))
            .ToDictionaryAsync(u => u.IdentityId, u => u.Nome, cancellationToken);

        return registos.Select(r => new RegistoAuditoriaDto(r.Id, r.Data,
                r.UtilizadorId is null ? null : nomes.GetValueOrDefault(r.UtilizadorId, r.UtilizadorId),
                r.Acao, r.Entidade, r.EntidadeId, r.ValoresAntes, r.ValoresDepois))
            .ToList();
    }
}
