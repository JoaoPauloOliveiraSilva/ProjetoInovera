using Innovera.Application.Registos;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Registos manuais (RF14): parcerias (Mod.235), ativos intangíveis (Mod.236), conhecimento tácito (Mod.238), ferramentas (Mod.241).</summary>
public class Registos : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarParceiros, "parceiros");
        groupBuilder.MapPost(CriarParceiro, "parceiros");
        groupBuilder.MapPut(EditarParceiro, "parceiros/{id:int}");
        groupBuilder.MapGet(ListarAcordos, "acordos");
        groupBuilder.MapPost(CriarAcordo, "acordos");
        groupBuilder.MapPut(EditarAcordo, "acordos/{id:int}");
        groupBuilder.MapGet(ListarAtivos, "ativos");
        groupBuilder.MapPost(CriarAtivo, "ativos");
        groupBuilder.MapPut(EditarAtivo, "ativos/{id:int}");
        groupBuilder.MapGet(ListarConhecimentoTacito, "conhecimento-tacito");
        groupBuilder.MapPost(CriarConhecimentoTacito, "conhecimento-tacito");
        groupBuilder.MapPut(EditarConhecimentoTacito, "conhecimento-tacito/{id:int}");
        groupBuilder.MapGet(ListarFerramentas, "ferramentas");
        groupBuilder.MapPost(CriarFerramenta, "ferramentas");
        groupBuilder.MapPut(EditarFerramenta, "ferramentas/{id:int}");
        groupBuilder.MapDelete(ApagarRegisto, "{tipo}/{id:int}");
    }

    public static async Task<Ok<IReadOnlyList<ParceiroDto>>> ListarParceiros(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarParceirosQuery()));

    public static async Task<Ok<int>> CriarParceiro(ISender sender, ParceiroDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarParceiroCommand(null, dados)));

    public static async Task<Ok<int>> EditarParceiro(ISender sender, int id, ParceiroDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarParceiroCommand(id, dados)));

    public static async Task<Ok<IReadOnlyList<AcordoDto>>> ListarAcordos(ISender sender, string? texto)
        => TypedResults.Ok(await sender.Send(new ListarAcordosQuery(texto)));

    public static async Task<Ok<int>> CriarAcordo(ISender sender, AcordoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAcordoCommand(null, dados)));

    public static async Task<Ok<int>> EditarAcordo(ISender sender, int id, AcordoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAcordoCommand(id, dados)));

    public static async Task<Ok<IReadOnlyList<AtivoDto>>> ListarAtivos(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarAtivosQuery()));

    public static async Task<Ok<int>> CriarAtivo(ISender sender, AtivoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAtivoCommand(null, dados)));

    public static async Task<Ok<int>> EditarAtivo(ISender sender, int id, AtivoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarAtivoCommand(id, dados)));

    public static async Task<Ok<IReadOnlyList<ConhecimentoTacitoDto>>> ListarConhecimentoTacito(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarConhecimentoTacitoQuery()));

    public static async Task<Ok<int>> CriarConhecimentoTacito(ISender sender, ConhecimentoTacitoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarConhecimentoTacitoCommand(null, dados)));

    public static async Task<Ok<int>> EditarConhecimentoTacito(ISender sender, int id, ConhecimentoTacitoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarConhecimentoTacitoCommand(id, dados)));

    public static async Task<Ok<IReadOnlyList<FerramentaDto>>> ListarFerramentas(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarFerramentasQuery()));

    public static async Task<Ok<int>> CriarFerramenta(ISender sender, FerramentaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarFerramentaCommand(null, dados)));

    public static async Task<Ok<int>> EditarFerramenta(ISender sender, int id, FerramentaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarFerramentaCommand(id, dados)));

    private static readonly Dictionary<string, TipoRegisto> Tipos = new(StringComparer.OrdinalIgnoreCase)
    {
        ["parceiros"] = TipoRegisto.Parceiro,
        ["acordos"] = TipoRegisto.Acordo,
        ["ativos"] = TipoRegisto.Ativo,
        ["conhecimento-tacito"] = TipoRegisto.ConhecimentoTacito,
        ["ferramentas"] = TipoRegisto.Ferramenta,
    };

    public static async Task<Results<NoContent, NotFound>> ApagarRegisto(ISender sender, string tipo, int id)
    {
        if (!Tipos.TryGetValue(tipo, out var t))
        {
            return TypedResults.NotFound();
        }

        await sender.Send(new ApagarRegistoCommand(t, id));
        return TypedResults.NoContent();
    }
}
