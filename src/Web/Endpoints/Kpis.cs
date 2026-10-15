using Innovera.Application.Kpis;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Indicadores (Mod.239): os 41 KPIs calculados, o painel inicial e os valores externos introduzidos à mão.</summary>
public class Kpis : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ObterIndicadores, "indicadores");
        groupBuilder.MapGet(ObterPlanoKpi, "{numero:int}/plano/{ano:int}");
        groupBuilder.MapGet(ObterEvolucaoKpi, "{numero:int}/evolucao");
        groupBuilder.MapPut(GuardarObjetivoKpi, "{numero:int}/plano/{ano:int}/objetivo");
        groupBuilder.MapPost(CriarAcaoKpi, "{numero:int}/plano/{ano:int}/acoes");
        groupBuilder.MapPut(EditarAcaoKpi, "{numero:int}/plano/{ano:int}/acoes/{id:int}");
        groupBuilder.MapDelete(ApagarAcaoKpi, "{numero:int}/plano/{ano:int}/acoes/{id:int}");
        groupBuilder.MapGet(ListarValoresExternos, "valores-externos");
        groupBuilder.MapPost(CriarValorExterno, "valores-externos");
        groupBuilder.MapPut(EditarValorExterno, "valores-externos/{id:int}");
        groupBuilder.MapDelete(ApagarValorExterno, "valores-externos/{id:int}");
    }

    public static async Task<Ok<IndicadoresDto>> ObterIndicadores(ISender sender, int? ano, Domain.Enums.PeriodoKpi? periodo)
        => TypedResults.Ok(await sender.Send(new ObterIndicadoresQuery(ano ?? DateTime.Today.Year, periodo ?? Domain.Enums.PeriodoKpi.Anual)));

    public static async Task<Ok<PlanoKpiDto>> ObterPlanoKpi(ISender sender, int numero, int ano)
        => TypedResults.Ok(await sender.Send(new ObterPlanoKpiQuery(numero, ano)));

    public static async Task<Ok<EvolucaoKpiDto>> ObterEvolucaoKpi(ISender sender, int numero)
        => TypedResults.Ok(await sender.Send(new ObterEvolucaoKpiQuery(numero)));

    public record ObjetivoPedido(string? Objetivo);

    public static async Task<NoContent> GuardarObjetivoKpi(ISender sender, int numero, int ano, ObjetivoPedido pedido)
    {
        await sender.Send(new GuardarObjetivoKpiCommand(numero, ano, pedido.Objetivo));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<int>> CriarAcaoKpi(ISender sender, int numero, int ano, AcaoKpiDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAcaoKpiCommand(numero, ano, null, dados)));

    public static async Task<Ok<int>> EditarAcaoKpi(ISender sender, int numero, int ano, int id, AcaoKpiDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAcaoKpiCommand(numero, ano, id, dados)));

    public static async Task<NoContent> ApagarAcaoKpi(ISender sender, int numero, int ano, int id)
    {
        await sender.Send(new ApagarAcaoKpiCommand(numero, ano, id));
        return TypedResults.NoContent();
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
