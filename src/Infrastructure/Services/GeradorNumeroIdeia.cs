using Innovera.Application.Common.Interfaces;
using Innovera.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Innovera.Infrastructure.Services;

public class GeradorNumeroIdeia : IGeradorNumeroIdeia
{
    // Constante (sem interpolação): o nome da sequência é fixo, não vem do utilizador.
    private const string ProximoNumeroSql = "SELECT nextval('" + ApplicationDbContext.SequenciaIdeias + "')::int AS \"Value\"";

    private readonly ApplicationDbContext _context;

    public GeradorNumeroIdeia(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task<int> ProximoAsync(CancellationToken cancellationToken)
        => _context.Database
            .SqlQueryRaw<int>(ProximoNumeroSql)
            .SingleAsync(cancellationToken);
}
