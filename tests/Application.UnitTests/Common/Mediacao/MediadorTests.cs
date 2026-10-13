using Innovera.Application.Common.Mediacao;
using Innovera.Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Innovera.Application.UnitTests.Common.Mediacao;

public class MediadorTests
{
    private ServiceProvider _servicos = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddSingleton<Registo>();
        services.AddMediador(typeof(MediadorTests).Assembly, typeof(PorFora<,>), typeof(PorDentro<,>));
        _servicos = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => _servicos.Dispose();

    [Test]
    public async Task EntregaOPedidoAoHandlerPassandoPelosComportamentosPorOrdem()
    {
        using var scope = _servicos.CreateScope();
        var resposta = await scope.ServiceProvider.GetRequiredService<ISender>().Send(new Dobrar(21));

        Assert.That(resposta, Is.EqualTo(42));
        Assert.That(_servicos.GetRequiredService<Registo>().Passos, Is.EqualTo(new[] { "fora", "dentro", "handler" }));
    }

    [Test]
    public async Task ComandoSemRespostaTambemPassaPeloPipeline()
    {
        using var scope = _servicos.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(new Anotar("olá"));

        Assert.That(_servicos.GetRequiredService<Registo>().Passos, Is.EqualTo(new[] { "fora", "dentro", "olá" }));
    }

    [Test]
    public async Task EnviarUmObjetoDevolveAResposta()
    {
        using var scope = _servicos.CreateScope();
        var resposta = await scope.ServiceProvider.GetRequiredService<ISender>().Send((object)new Dobrar(5));

        Assert.That(resposta, Is.EqualTo(10));
    }

    [Test]
    public async Task PublicaOEventoATodosOsHandlers()
    {
        using var scope = _servicos.CreateScope();
        await scope.ServiceProvider.GetRequiredService<IPublisher>().Publish(new Aconteceu());

        Assert.That(_servicos.GetRequiredService<Registo>().Passos, Is.EquivalentTo(new[] { "evento 1", "evento 2" }));
    }

    [Test]
    public void ExcecaoDoHandlerChegaTalComoE()
    {
        using var scope = _servicos.CreateScope();
        Assert.ThrowsAsync<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService<ISender>().Send(new Falhar()));
    }
}

public class Registo
{
    public List<string> Passos { get; } = [];
}

public record Dobrar(int Valor) : IRequest<int>;

public class DobrarHandler(Registo registo) : IRequestHandler<Dobrar, int>
{
    public Task<int> Handle(Dobrar request, CancellationToken cancellationToken)
    {
        registo.Passos.Add("handler");
        return Task.FromResult(request.Valor * 2);
    }
}

public record Anotar(string Texto) : IRequest;

public class AnotarHandler(Registo registo) : IRequestHandler<Anotar>
{
    public Task Handle(Anotar request, CancellationToken cancellationToken)
    {
        registo.Passos.Add(request.Texto);
        return Task.CompletedTask;
    }
}

public record Falhar : IRequest;

public class FalharHandler : IRequestHandler<Falhar>
{
    public Task Handle(Falhar request, CancellationToken cancellationToken) => throw new InvalidOperationException("falhou");
}

public class Aconteceu : INotification
{
}

public class PrimeiroHandler(Registo registo) : INotificationHandler<Aconteceu>
{
    public Task Handle(Aconteceu notification, CancellationToken cancellationToken)
    {
        registo.Passos.Add("evento 1");
        return Task.CompletedTask;
    }
}

public class SegundoHandler(Registo registo) : INotificationHandler<Aconteceu>
{
    public Task Handle(Aconteceu notification, CancellationToken cancellationToken)
    {
        registo.Passos.Add("evento 2");
        return Task.CompletedTask;
    }
}

public class PorFora<TRequest, TResponse>(Registo registo) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not Falhar)
        {
            registo.Passos.Add("fora");
        }

        return next();
    }
}

public class PorDentro<TRequest, TResponse>(Registo registo) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not Falhar)
        {
            registo.Passos.Add("dentro");
        }

        return next();
    }
}
