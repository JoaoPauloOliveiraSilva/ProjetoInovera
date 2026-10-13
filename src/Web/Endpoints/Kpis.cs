using Innovera.Application.Kpis;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Indicadores (Mod.239). Por agora: valores externos introduzidos à mão.</summary>
public class Kpis : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarValoresExternos, "valores-externos");
        groupBuilder.MapPost(CriarValorExterno, "valores-externos");
        groupBuilder.MapPut(EditarValorExterno, "valores-externos/{id:int}");
        groupBuilder.MapDelete(ApagarValorExterno, "valores-externos/{id:int}");
    }

    public static async Task<Ok<IReadOnlyList<ValorExternoDto>>> ListarValoresExternos(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarValoresExternosQuery()));

    public static async Task<Ok<int>> CriarValorExterno(ISender sender, ValorExternoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarValorExternoCommand(null, dados)));

    public static async Task<Ok<int>> EditarValorExterno(ISender sender, int id, ValorExternoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarValorExternoCommand(id, dados)));

    public static async Task<NoContent> ApagarValorExterno(ISender sender, int id)
    {
        await sender.Send(new ApagarValorExternoCommand(id));
        return TypedResults.NoContent();
    }
}
