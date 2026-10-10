using System.Collections.Concurrent;
using Innovera.Application.Common.Interfaces;

namespace Innovera.Application.FunctionalTests.Infrastructure;

/// <summary>Guarda os emails em memória em vez de os enviar (para os testes lerem os links).</summary>
public class EmailFalso : IEmail
{
    public static ConcurrentQueue<MensagemEmail> Enviados { get; } = new();

    public Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken)
    {
        Enviados.Enqueue(mensagem);
        return Task.CompletedTask;
    }
}
