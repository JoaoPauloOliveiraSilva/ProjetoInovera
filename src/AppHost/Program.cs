using Innovera.Shared;

var builder = DistributedApplication.CreateBuilder(args);

// PostgreSQL num contentor Docker (precisa do Docker Desktop a correr). Os dados ficam num volume.
var databaseServer = builder
    .AddPostgres(Services.DatabaseServer)
    .WithDataVolume("innovera-postgres-dados");

var database = databaseServer.AddDatabase(Services.Database);

// Servidor de email de testes: os emails enviados (convites, recuperar password) veem-se no painel do Mailpit.
var mailpit = builder.AddContainer("mailpit", "axllent/mailpit")
    .WithHttpEndpoint(targetPort: 8025, name: "http")
    .WithEndpoint(targetPort: 1025, name: "smtp", scheme: "tcp");
var smtp = mailpit.GetEndpoint("smtp");

var web = builder.AddProject<Projects.Web>(Services.WebApi)
    .WithReference(database)
    .WaitFor(database)
    .WithEnvironment("Smtp__Host", smtp.Property(EndpointProperty.Host))
    .WithEnvironment("Smtp__Porta", smtp.Property(EndpointProperty.Port))
    .WithEnvironment("Smtp__Ssl", "false")
    .WithExternalHttpEndpoints()
    .WithAspNetCoreEnvironment()
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayText = "Scalar API Reference";
        url.Url = "/scalar";
    });

if (builder.ExecutionContext.IsRunMode)
{
    var frontend = builder.AddJavaScriptApp(Services.WebFrontend, "./../Web/ClientApp")
        .WithRunScript("start")
        .WithReference(web)
        .WaitFor(web)
        .WithHttpEndpoint(env: "PORT")
        .WithExternalHttpEndpoints();

    // Os links dos emails apontam para o frontend.
    web.WithEnvironment("Email__UrlSite", frontend.GetEndpoint("http"));
}

builder.Build().Run();
