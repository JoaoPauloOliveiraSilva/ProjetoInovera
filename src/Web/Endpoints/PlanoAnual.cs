using Innovera.Application.Registos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Plano anual de atividades (Mod.69).</summary>
public class PlanoAnual : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarPlanoAnual);
        groupBuilder.MapPost(CriarAtividadePlano);
        groupBuilder.MapPut(EditarAtividadePlano, "{id:int}");
        groupBuilder.MapDelete(ApagarAtividadePlano, "{id:int}");
    }

    public static async Task<Ok<IReadOnlyList<AtividadePlanoDto>>> ListarPlanoAnual(ISender sender, int? ano, TimeProvider relogio)
        => TypedResults.Ok(await sender.Send(new ListarPlanoAnualQuery(ano ?? relogio.GetUtcNow().Year)));

    public static async Task<Ok<int>> CriarAtividadePlano(ISender sender, AtividadePlanoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAtividadePlanoCommand(null, dados)));

    public static async Task<Ok<int>> EditarAtividadePlano(ISender sender, int id, AtividadePlanoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAtividadePlanoCommand(id, dados)));

    public static async Task<NoContent> ApagarAtividadePlano(ISender sender, int id)
    {
        await sender.Send(new ApagarAtividadePlanoCommand(id));
        return TypedResults.NoContent();
    }
}
