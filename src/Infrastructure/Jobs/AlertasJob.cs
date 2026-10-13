using Innovera.Application.Registos;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Innovera.Infrastructure.Jobs;

/// <summary>
/// De hora a hora gera os avisos do plano anual (Mod.69) e das validades dos ativos intangíveis (RF16).
/// Cada pessoa só é avisada uma vez de cada ocorrência (tabela Alertas).
/// </summary>
public class AlertasJob : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromHours(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeProvider _relogio;
    private readonly ILogger<AlertasJob> _logger;

    public AlertasJob(IServiceScopeFactory scopeFactory, TimeProvider relogio, ILogger<AlertasJob> logger)
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
                using var scope = _scopeFactory.CreateScope();
                var criados = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new GerarAlertasCommand(), stoppingToken);
                if (criados > 0)
                {
                    _logger.LogInformation("{Quantidade} aviso(s) do plano anual e de validades enviados.", criados);
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Erro ao gerar os avisos do plano anual e das validades.");
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
}
