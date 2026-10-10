using Innovera.Application.Iniciativas;
using Innovera.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Portefólio de iniciativas (Mod.246) e importação do Excel.</summary>
public class Iniciativas : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarIniciativas);
        groupBuilder.MapGet(ObterIniciativa, "{id:int}");
        groupBuilder.MapPost(CriarIniciativa);
        groupBuilder.MapPut(AtualizarIniciativa, "{id:int}");
        groupBuilder.MapPost(RegistarPontoSituacao, "{id:int}/ponto-situacao");
        groupBuilder.MapPost(ImportarMod246, "importar-mod246").DisableAntiforgery();
    }

    public static async Task<Ok<IReadOnlyList<IniciativaResumoDto>>> ListarIniciativas(ISender sender, TipoIniciativa? tipo, EstadoIniciativa? estado, string? texto)
        => TypedResults.Ok(await sender.Send(new ListarIniciativasQuery(tipo, estado, texto)));

    public static async Task<Ok<IniciativaDetalheDto>> ObterIniciativa(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new ObterIniciativaQuery(id)));

    public static async Task<Ok<int>> CriarIniciativa(ISender sender, CriarIniciativaCommand command)
        => TypedResults.Ok(await sender.Send(command));

    public static async Task<NoContent> AtualizarIniciativa(ISender sender, int id, DadosIniciativa dados)
    {
        await sender.Send(new AtualizarIniciativaCommand(id, dados));
        return TypedResults.NoContent();
    }

    public record PontoSituacaoPedido(string Texto);

    public static async Task<NoContent> RegistarPontoSituacao(ISender sender, int id, PontoSituacaoPedido pedido)
    {
        await sender.Send(new RegistarPontoSituacaoCommand(id, pedido.Texto));
        return TypedResults.NoContent();
    }

    public static async Task<Results<Ok<ResultadoImportacaoDto>, BadRequest<string>>> ImportarMod246(ISender sender, IFormFile ficheiro)
    {
        if (ficheiro.Length > 25 * 1024 * 1024)
        {
            return TypedResults.BadRequest("O ficheiro tem de ter no máximo 25 MB.");
        }

        await using var conteudo = ficheiro.OpenReadStream();
        return TypedResults.Ok(await sender.Send(new ImportarPortefolioCommand(conteudo)));
    }
}
