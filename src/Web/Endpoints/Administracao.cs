using Innovera.Application.Administracao;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

/// <summary>Backoffice: utilizadores, perfis e permissões. As permissões são verificadas nos casos de uso.</summary>
public class Administracao : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarUtilizadores, "utilizadores");
        groupBuilder.MapPost(AdicionarUtilizador, "utilizadores");
        groupBuilder.MapPut(EditarUtilizador, "utilizadores/{id:int}");
        groupBuilder.MapPost(ReenviarConvite, "utilizadores/{id:int}/reenviar-convite");
        groupBuilder.MapPost(DesativarUtilizador, "utilizadores/{id:int}/desativar");
        groupBuilder.MapPost(AtivarUtilizador, "utilizadores/{id:int}/ativar");

        groupBuilder.MapGet(ListarPerfis, "perfis");
        groupBuilder.MapPost(CriarPerfil, "perfis");
        groupBuilder.MapPut(DefinirPermissoes, "perfis/{nome}/permissoes");
        groupBuilder.MapDelete(ApagarPerfil, "perfis/{nome}");
    }

    public static async Task<Ok<IReadOnlyList<UtilizadorAdminDto>>> ListarUtilizadores(ISender sender, string? texto)
        => TypedResults.Ok(await sender.Send(new ListarUtilizadoresQuery(texto)));

    public static async Task<Ok<int>> AdicionarUtilizador(ISender sender, AdicionarUtilizadorCommand command)
        => TypedResults.Ok(await sender.Send(command));

    public record EditarUtilizadorPedido(string Nome, string? Empresa, IReadOnlyList<string> Perfis);

    public static async Task<NoContent> EditarUtilizador(ISender sender, int id, EditarUtilizadorPedido pedido)
    {
        await sender.Send(new EditarUtilizadorCommand(id, pedido.Nome, pedido.Empresa, pedido.Perfis));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ReenviarConvite(ISender sender, int id)
    {
        await sender.Send(new ReenviarConviteCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> DesativarUtilizador(ISender sender, int id)
    {
        await sender.Send(new AlterarEstadoUtilizadorCommand(id, false));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> AtivarUtilizador(ISender sender, int id)
    {
        await sender.Send(new AlterarEstadoUtilizadorCommand(id, true));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<PerfisDto>> ListarPerfis(ISender sender)
        => TypedResults.Ok(await sender.Send(new ListarPerfisQuery()));

    public static async Task<NoContent> CriarPerfil(ISender sender, CriarPerfilCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public record PermissoesPedido(IReadOnlyList<string> Permissoes);

    public static async Task<NoContent> DefinirPermissoes(ISender sender, string nome, PermissoesPedido pedido)
    {
        await sender.Send(new DefinirPermissoesCommand(nome, pedido.Permissoes));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ApagarPerfil(ISender sender, string nome)
    {
        await sender.Send(new ApagarPerfilCommand(nome));
        return TypedResults.NoContent();
    }
}
