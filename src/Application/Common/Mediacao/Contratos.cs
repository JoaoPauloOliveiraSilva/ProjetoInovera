namespace Innovera.Application.Common.Mediacao;

// Mediador próprio (substitui o MediatR, que passou a ter licença paga para empresas como a dst).
// Os nomes são os mesmos do MediatR para o resto do código não mudar: cada pedido (comando ou consulta)
// tem um handler, e todos passam pelos mesmos comportamentos (registo, autorização, validação, desempenho).

/// <summary>Base de todos os pedidos.</summary>
public interface IBaseRequest
{
}

/// <summary>Pedido que devolve uma resposta (consulta ou comando com resultado).</summary>
public interface IRequest<out TResponse> : IBaseRequest
{
}

/// <summary>Pedido sem resposta (comando).</summary>
public interface IRequest : IBaseRequest
{
}

/// <summary>Resposta "vazia" dos comandos, usada dentro do pipeline.</summary>
public readonly record struct Unit
{
    public static readonly Unit Value;
}

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public interface IRequestHandler<in TRequest>
    where TRequest : IRequest
{
    Task Handle(TRequest request, CancellationToken cancellationToken);
}

public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    Task Handle(TNotification notification, CancellationToken cancellationToken);
}

/// <summary>O passo seguinte do pipeline (o próximo comportamento ou o handler).</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>(CancellationToken cancellationToken = default);

/// <summary>Comportamento que envolve todos os pedidos (ex.: autorização, validação).</summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);
}

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest;

    /// <summary>Envia um pedido de tipo só conhecido em execução; devolve a resposta (ou null nos comandos).</summary>
    Task<object?> Send(object request, CancellationToken cancellationToken = default);
}

public interface IPublisher
{
    Task Publish(INotification notification, CancellationToken cancellationToken = default);
}

public interface IMediator : ISender, IPublisher
{
}
