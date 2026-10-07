using Innovera.Shared;

namespace Innovera.TestAppHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = DistributedApplication.CreateBuilder(args);

        builder
            .AddPostgres(Services.DatabaseServer)
            .AddDatabase(Services.Database);

        builder.Build().Run();
    }
}
