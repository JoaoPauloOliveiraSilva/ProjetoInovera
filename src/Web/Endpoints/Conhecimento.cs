using Innovera.Application.Charters;
using Innovera.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Repositórios de conhecimento: lições aprendidas (Mod.252) e conhecimento codificado (Mod.237).</summary>
public class Conhecimento : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarLicoes, "licoes");
        groupBuilder.MapGet(ListarConhecimentoCodificado, "codificado");
    }

    public static async Task<Ok<IReadOnlyList<LicaoRepositorioDto>>> ListarLicoes(ISender sender, CategoriaLicao? categoria, string? texto)
        => TypedResults.Ok(await sender.Send(new ListarLicoesQuery(categoria, texto)));

    public static async Task<Ok<IReadOnlyList<ConhecimentoCodificadoDto>>> ListarConhecimentoCodificado(ISender sender, OrigemConhecimento? origem,
        string? texto)
        => TypedResults.Ok(await sender.Send(new ListarConhecimentoCodificadoQuery(origem, texto)));
}
