using Innovera.Domain.Enums;
using Innovera.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Innovera.Infrastructure.Jobs;

/// <summary>
/// De hora a hora fecha as discussões que já fizeram 30 dias: a ideia passa para "em avaliação pelo manager".
/// (Simples por agora; pode passar para o Hangfire quando houver mais tarefas agendadas.)
/// </summary>
public class FecharDiscussoesJob : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _relogio;
    private readonly ILogger<FecharDiscussoesJob> _logger;

    public FecharDiscussoesJob(IServiceScopeFactory scopeFactory, TimeProvider relogio, ILogger<FecharDiscussoesJob> logger)
    {
        _scopeFactory = scopeFactory;
        _relogio = relogio;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecutarUmaVezAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao fechar as discussões das ideias.");
            }

            try
            {
                await Task.Delay(Intervalo, _relogio, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public async Task<int> ExecutarUmaVezAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var agora = _relogio.GetUtcNow();

        var terminadas = await context.Ideias
            .Where(i => i.Estado == EstadoIdeia.EmDiscussao && i.FimDiscussao <= agora)
            .ToListAsync(cancellationToken);

        foreach (var ideia in terminadas)
        {
            ideia.FecharDiscussao();
        }

        if (terminadas.Count > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Discussão fechada em {Quantidade} ideia(s).", terminadas.Count);
        }

        return terminadas.Count;
    }
}
