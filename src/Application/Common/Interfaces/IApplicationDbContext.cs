namespace Innovera.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    // Os DbSet das entidades do domínio (Ideia, Iniciativa, ProjectCharter, KPIs…) são acrescentados
    // aqui quando se fizer o mapeamento EF, depois de fechada a escolha da base de dados com a dstelecom.

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
