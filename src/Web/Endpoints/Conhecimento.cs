using Innovera.Application.Charters;
using Innovera.Application.Registos;
using Innovera.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Repositórios de conhecimento: lições aprendidas (Mod.252) e conhecimento codificado (Mod.237, também com entradas manuais).</summary>
public class Conhecimento : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarLicoes, "licoes");
        groupBuilder.MapGet(ListarConhecimentoCodificado, "codificado");
        groupBuilder.MapPost(CriarConhecimentoCodificado, "codificado");
        groupBuilder.MapPut(EditarConhecimentoCodificado, "codificado/{id:int}");
        groupBuilder.MapDelete(ApagarConhecimentoCodificado, "codificado/{id:int}");
    }

    public static async Task<Ok<IReadOnlyList<LicaoRepositorioDto>>> ListarLicoes(ISender sender, CategoriaLicao? categoria, string? texto)
        => TypedResults.Ok(await sender.Send(new ListarLicoesQuery(categoria, texto)));

    public static async Task<Ok<IReadOnlyList<ConhecimentoCodificadoDto>>> ListarConhecimentoCodificado(ISender sender, OrigemConhecimento? origem,
        string? texto)
        => TypedResults.Ok(await sender.Send(new ListarConhecimentoCodificadoQuery(origem, texto)));

    public static async Task<Ok<int>> CriarConhecimentoCodificado(ISender sender, ConhecimentoCodificadoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarConhecimentoCodificadoCommand(null, dados)));

    public static async Task<Ok<int>> EditarConhecimentoCodificado(ISender sender, int id, ConhecimentoCodificadoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarConhecimentoCodificadoCommand(id, dados)));

    public static async Task<NoContent> ApagarConhecimentoCodificado(ISender sender, int id)
    {
        await sender.Send(new ApagarConhecimentoCodificadoCommand(id));
        return TypedResults.NoContent();
    }
}
