using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Innovera.Application.Common.Mediacao;

/// <summary>
/// Entrega cada pedido ao seu handler, passando pelos comportamentos registados (o primeiro registado é o mais exterior),
/// e publica os eventos de domínio a todos os seus handlers, por ordem.
/// </summary>
public sealed class Mediador(IServiceProvider servicos) : IMediator
{
    private static readonly ConcurrentDictionary<Type, Executor> Executores = new();
    private static readonly ConcurrentDictionary<Type, Publicador> Publicadores = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ((Executor<TResponse>)ExecutorDe(request.GetType())).ExecutarTipado(request, servicos, cancellationToken);
    }

    public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);
        return ((Executor<Unit>)ExecutorDe(request.GetType())).ExecutarTipado(request, servicos, cancellationToken);
    }

    public Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecutorDe(request.GetType()).Executar(request, servicos, cancellationToken);
    }

    public Task Publish(INotification notification, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(notification);
        var publicador = Publicadores.GetOrAdd(notification.GetType(),
            tipo => (Publicador)Activator.CreateInstance(typeof(Publicador<>).MakeGenericType(tipo))!);
        return publicador.Publicar(notification, servicos, cancellationToken);
    }

    private static Executor ExecutorDe(Type tipo) => Executores.GetOrAdd(tipo, CriarExecutor);

    private static Executor CriarExecutor(Type tipo)
    {
        if (typeof(IRequest).IsAssignableFrom(tipo))
        {
            return (Executor)Activator.CreateInstance(typeof(ExecutorComando<>).MakeGenericType(tipo))!;
        }

        var contrato = tipo.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequest<>))
            ?? throw new ArgumentException($"{tipo.Name} não é um pedido (IRequest).", nameof(tipo));
        return (Executor)Activator.CreateInstance(typeof(ExecutorPedido<,>).MakeGenericType(tipo, contrato.GetGenericArguments()[0]))!;
    }

    private abstract class Executor
    {
        public abstract Task<object?> Executar(object pedido, IServiceProvider servicos, CancellationToken cancellationToken);
    }

    private abstract class Executor<TResponse> : Executor
    {
        public abstract Task<TResponse> ExecutarTipado(object pedido, IServiceProvider servicos, CancellationToken cancellationToken);

        public override async Task<object?> Executar(object pedido, IServiceProvider servicos, CancellationToken cancellationToken)
            => await ExecutarTipado(pedido, servicos, cancellationToken);

        /// <summary>Envolve o handler com os comportamentos: o primeiro registado fica por fora.</summary>
        protected static Task<TResponse> ComComportamentos<TRequest>(TRequest pedido, RequestHandlerDelegate<TResponse> handler,
            IServiceProvider servicos, CancellationToken cancellationToken)
            where TRequest : notnull
        {
            var seguinte = handler;
            foreach (var comportamento in servicos.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse())
            {
                var proximo = seguinte;
                seguinte = _ => comportamento.Handle(pedido, proximo, cancellationToken);
            }

            return seguinte(cancellationToken);
        }
    }

    private sealed class ExecutorPedido<TRequest, TResponse> : Executor<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> ExecutarTipado(object pedido, IServiceProvider servicos, CancellationToken cancellationToken)
        {
            var p = (TRequest)pedido;
            return ComComportamentos(p,
                _ => servicos.GetRequiredService<IRequestHandler<TRequest, TResponse>>().Handle(p, cancellationToken),
                servicos, cancellationToken);
        }
    }

    private sealed class ExecutorComando<TRequest> : Executor<Unit>
        where TRequest : IRequest
    {
        public override Task<Unit> ExecutarTipado(object pedido, IServiceProvider servicos, CancellationToken cancellationToken)
        {
            var p = (TRequest)pedido;
            return ComComportamentos(p,
                async _ =>
                {
                    await servicos.GetRequiredService<IRequestHandler<TRequest>>().Handle(p, cancellationToken);
                    return Unit.Value;
                },
                servicos, cancellationToken);
        }
    }

    private abstract class Publicador
    {
        public abstract Task Publicar(INotification notificacao, IServiceProvider servicos, CancellationToken cancellationToken);
    }

    private sealed class Publicador<TNotification> : Publicador
        where TNotification : INotification
    {
        public override async Task Publicar(INotification notificacao, IServiceProvider servicos, CancellationToken cancellationToken)
        {
            foreach (var handler in servicos.GetServices<INotificationHandler<TNotification>>())
            {
                await handler.Handle((TNotification)notificacao, cancellationToken);
            }
        }
    }
}

public static class RegistoMediador
{
    private static readonly Type[] Contratos = [typeof(IRequestHandler<,>), typeof(IRequestHandler<>), typeof(INotificationHandler<>)];

    /// <summary>Regista o mediador, todos os handlers do assembly e os comportamentos (pela ordem dada: o primeiro fica por fora).</summary>
    public static IServiceCollection AddMediador(this IServiceCollection services, Assembly assembly, params Type[] comportamentos)
    {
        services.AddScoped<Mediador>();
        services.AddScoped<IMediator>(sp => sp.GetRequiredService<Mediador>());
        services.AddScoped<ISender>(sp => sp.GetRequiredService<Mediador>());
        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<Mediador>());

        foreach (var tipo in assembly.GetTypes().Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false }))
        {
            foreach (var contrato in tipo.GetInterfaces()
                         .Where(i => i.IsGenericType && Contratos.Contains(i.GetGenericTypeDefinition())))
            {
                services.AddTransient(contrato, tipo);
            }
        }

        foreach (var comportamento in comportamentos)
        {
            services.AddTransient(typeof(IPipelineBehavior<,>), comportamento);
        }

        return services;
    }
}
