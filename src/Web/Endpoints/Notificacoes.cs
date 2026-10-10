using Innovera.Application.Notificacoes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

public class Notificacoes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarNotificacoes);
        groupBuilder.MapPost(MarcarNotificacaoLida, "{id:int}/lida");
        groupBuilder.MapPost(MarcarTodasLidas, "lidas");
    }

    public static async Task<Ok<NotificacoesDto>> ListarNotificacoes(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarNotificacoesQuery()));

    public static async Task<NoContent> MarcarNotificacaoLida(ISender sender, int id)
    {
        await sender.Send(new MarcarNotificacoesLidasCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> MarcarTodasLidas(ISender sender)
    {
        await sender.Send(new MarcarNotificacoesLidasCommand(null));
        return TypedResults.NoContent();
    }
}
