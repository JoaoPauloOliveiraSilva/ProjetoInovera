using System.Reflection;
using Innovera.Application.Common.Behaviours;
using Innovera.Application.Common.Mediacao;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Mediador próprio (sem MediatR): o primeiro comportamento é o mais exterior.
        builder.Services.AddMediador(Assembly.GetExecutingAssembly(),
            typeof(LoggingBehaviour<,>),
            typeof(UnhandledExceptionBehaviour<,>),
            typeof(AuthorizationBehaviour<,>),
            typeof(ValidationBehaviour<,>),
            typeof(PerformanceBehaviour<,>));
    }
}
