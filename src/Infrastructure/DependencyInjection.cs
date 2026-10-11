using Innovera.Application.Common.Interfaces;
using Innovera.Infrastructure.Data;
using Innovera.Infrastructure.Data.Interceptors;
using Innovera.Infrastructure.Identity;
using Innovera.Infrastructure.Jobs;
using Innovera.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString(Services.Database);
        Guard.Against.Null(connectionString, message: $"Connection string '{Services.Database}' not found.");

        // Ordem: eventos de domínio primeiro (os handlers podem acrescentar entidades), depois datas/autor, depois auditoria.
        builder.Services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        builder.Services.AddScoped<ISaveChangesInterceptor, AuditoriaInterceptor>();

        builder.Services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            // Consultas com vários Include (ideia + comentários + gostos...) em pedidos separados, sem produto cartesiano.
            options.UseNpgsql(connectionString, npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
        });

        builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        builder.Services.AddScoped<ApplicationDbContextInitialiser>();

        builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        // A API responde 401/403 em vez de redirecionar para uma página de login.
        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "innovera.auth";
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };
        });

        builder.Services.AddAuthorizationBuilder();

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                // Só entra quem aceitou o convite (o email fica confirmado ao definir a password).
                options.SignIn.RequireConfirmedEmail = true;
                // 5 tentativas erradas bloqueiam a conta durante 15 minutos.
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<IdentityErrorDescriberPt>();

        // Validade dos links de convite e de recuperação da password.
        builder.Services.Configure<DataProtectionTokenProviderOptions>(options => options.TokenLifespan = ServicoContas.ValidadeLinks);

        // Contas desativadas perdem a sessão em poucos minutos (o cookie é revalidado).
        builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromMinutes(5));

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddTransient<IIdentityService, IdentityService>();
        builder.Services.AddScoped<IUtilizadores, UtilizadoresService>();
        builder.Services.AddScoped<IContas, ServicoContas>();
        builder.Services.AddTransient<IEmail, EmailSmtp>();
        builder.Services.AddScoped<INotificacoes, ServicoNotificacoes>();
        builder.Services.AddSingleton<ILeitorFolhas, LeitorXlsx>();
        builder.Services.AddScoped<IGeradorNumeroIdeia, GeradorNumeroIdeia>();
        builder.Services.AddSingleton<IArmazenamentoFicheiros, ArmazenamentoLocal>();

        if (builder.Configuration.GetValue("Tarefas:FecharDiscussoes", true))
        {
            builder.Services.AddHostedService<FecharDiscussoesJob>();
        }
    }
}
