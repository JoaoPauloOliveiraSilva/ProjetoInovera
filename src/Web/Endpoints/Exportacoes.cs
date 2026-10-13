using Innovera.Application.Exportacoes;
using Innovera.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Outputs em Excel dos modelos da dstelecom (Mod.246, 239, 252, 237, 238, 235, 236, 241, 52 e 69).</summary>
public class Exportacoes : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ExportarModelo, "{modelo}");
    }

    public static async Task<FileStreamHttpResult> ExportarModelo(ISender sender, string modelo, int? ano, PeriodoKpi? periodo)
    {
        var ficheiro = await sender.Send(new ExportarModeloQuery(modelo, ano, periodo));
        return TypedResults.File(ficheiro.Conteudo, ficheiro.TipoConteudo, ficheiro.NomeFicheiro);
    }
}
