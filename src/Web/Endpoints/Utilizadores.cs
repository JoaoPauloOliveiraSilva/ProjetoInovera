using Innovera.Application.Utilizadores;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

public class Utilizadores : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapGet(ObterUtilizadorAtual, "eu").RequireAuthorization();
        groupBuilder.MapGet(PesquisarUtilizadores).RequireAuthorization();
        groupBuilder.MapPost(RegistarConta, "registo").AllowAnonymous();
    }

    public static async Task<Ok<UtilizadorAtualDto>> ObterUtilizadorAtual(ISender sender)
        => TypedResults.Ok(await sender.Send(new ObterUtilizadorAtualQuery()));

    public static async Task<Ok<IReadOnlyList<UtilizadorResumoDto>>> PesquisarUtilizadores(ISender sender, string? texto)
        => TypedResults.Ok(await sender.Send(new PesquisarUtilizadoresQuery(texto)));

    public static async Task<Ok<int>> RegistarConta(ISender sender, RegistarContaCommand command)
        => TypedResults.Ok(await sender.Send(command));
}
