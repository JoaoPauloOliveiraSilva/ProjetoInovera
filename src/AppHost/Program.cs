using Innovera.Shared;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL num contentor Docker (precisa do Docker Desktop a correr). Os dados ficam num volume.
var databaseServer = builder
    .AddPostgres(Services.DatabaseServer)
    .WithDataVolume("innovera-postgres-dados");

var database = databaseServer.AddDatabase(Services.Database);

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

if (builder.ExecutionContext.IsRunMode)
{
    builder.AddJavaScriptApp(Services.WebFrontend, "./../Web/ClientApp")
        .WithRunScript("start")
        .WithReference(web)
        .WaitFor(web)
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints();
}

builder.Build().Run();
