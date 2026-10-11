using Innovera.Infrastructure.Data;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.AddServiceDefaults();

builder.AddKeyVaultIfConfigured();
builder.AddApplicationServices();
builder.AddInfrastructureServices();
builder.AddWebServices();

var app = builder.Build();

// Aplica as migrações e cria os perfis e a conta inicial (e dados de exemplo se "DadosExemplo" = true).
// Não corre quando o build gera o openapi/v1.json (a ferramenta arranca a aplicação sem base de dados).
var aGerarOpenApi = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";
if (!aGerarOpenApi)
{
    await app.InitialiseDatabaseAsync();
}

// Em produção a aplicação fica atrás do Caddy (HTTPS): usar o protocolo e o IP originais do pedido.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseCors(static builder =>
        builder.AllowAnyMethod()
            .AllowAnyHeader()
            .AllowAnyOrigin());
}

app.UseFileServer();

// Documentação da API (/scalar) só em desenvolvimento.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler(options => { });


app.MapDefaultEndpoints();
app.MapEndpoints(typeof(Program).Assembly);

app.MapFallbackToFile("index.html");

app.Run();
