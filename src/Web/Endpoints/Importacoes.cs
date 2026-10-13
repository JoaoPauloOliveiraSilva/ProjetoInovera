using Innovera.Application.Importacoes;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Importação dos modelos Excel da dstelecom (o modelo é reconhecido pelo conteúdo do ficheiro).</summary>
public class Importacoes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapPost(ImportarModelo, string.Empty).DisableAntiforgery();
    }

    public static async Task<Results<Ok<ResultadoImportacaoModeloDto>, BadRequest<string>>> ImportarModelo(ISender sender, IFormFile ficheiro)
    {
        if (ficheiro.Length > 25 * 1024 * 1024)
        {
            return TypedResults.BadRequest("O ficheiro tem de ter no máximo 25 MB.");
        }

        await using var conteudo = ficheiro.OpenReadStream();
        return TypedResults.Ok(await sender.Send(new ImportarModeloCommand(conteudo)));
    }
}
