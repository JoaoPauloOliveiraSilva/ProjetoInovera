using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Innovera.Infrastructure.Data;

/// <summary>
/// Usado só pelas ferramentas do EF Core (dotnet ef migrations add …). Não liga à base de dados.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=innovera;Username=postgres;Password=postgres")
            .Options;

        return new ApplicationDbContext(options);
    }
}
