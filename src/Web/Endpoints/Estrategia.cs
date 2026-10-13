using Innovera.Application.Registos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Estratégia de inovação anual (Mod.52).</summary>
public class Estrategia : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarEstrategia);
        groupBuilder.MapPost(CriarLinhaEstrategia);
        groupBuilder.MapPut(EditarLinhaEstrategia, "{id:int}");
        groupBuilder.MapDelete(ApagarLinhaEstrategia, "{id:int}");
        groupBuilder.MapPost(CopiarEstrategia, "copiar");
    }

    public static async Task<Ok<IReadOnlyList<LinhaEstrategiaDto>>> ListarEstrategia(ISender sender, int? ano, TimeProvider relogio)
        => TypedResults.Ok(await sender.Send(new ListarEstrategiaQuery(ano ?? relogio.GetUtcNow().Year)));

    public static async Task<Ok<int>> CriarLinhaEstrategia(ISender sender, LinhaEstrategiaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLinhaEstrategiaCommand(null, dados)));

    public static async Task<Ok<int>> EditarLinhaEstrategia(ISender sender, int id, LinhaEstrategiaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLinhaEstrategiaCommand(id, dados)));

    public static async Task<NoContent> ApagarLinhaEstrategia(ISender sender, int id)
    {
        await sender.Send(new ApagarLinhaEstrategiaCommand(id));
        return TypedResults.NoContent();
    }

    public record CopiarPedido(int DeAno, int ParaAno);

    public static async Task<Ok<int>> CopiarEstrategia(ISender sender, CopiarPedido pedido)
        => TypedResults.Ok(await sender.Send(new CopiarEstrategiaCommand(pedido.DeAno, pedido.ParaAno)));
}
