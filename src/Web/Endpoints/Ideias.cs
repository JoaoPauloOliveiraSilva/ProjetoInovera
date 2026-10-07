using Innovera.Application.Ideias;
using Innovera.Application.Ideias.Commands;
using Innovera.Application.Ideias.Queries;
using Innovera.Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Innovera.Web.Endpoints;

public class Ideias : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.RequireAuthorization();

        groupBuilder.MapGet(ListarIdeias);
        groupBuilder.MapGet(ObterIdeia, "{id:int}");
        groupBuilder.MapPost(CriarIdeia);
        groupBuilder.MapPost(ConfirmarAutoria, "{id:int}/confirmar-autoria");
        groupBuilder.MapPost(ValidarIdeia, "{id:int}/validar");
        groupBuilder.MapPost(FecharDiscussao, "{id:int}/fechar-discussao");
        groupBuilder.MapPost(AlternarGosto, "{id:int}/gosto");
        groupBuilder.MapPost(Comentar, "{id:int}/comentarios");
        groupBuilder.MapPost(AlternarGostoComentario, "comentarios/{comentarioId:int}/gosto");
        groupBuilder.MapPost(RegistarAvaliacao, "{id:int}/avaliacao");
        groupBuilder.MapPost(RegistarDecisaoDst, "{id:int}/decisao-dst");
        groupBuilder.MapPost(DefinirClasse, "{id:int}/classe");
        groupBuilder.MapPost(AdicionarAnexo, "{id:int}/anexos").DisableAntiforgery();
        groupBuilder.MapGet(ObterAnexo, "anexos/{anexoId:int}");
    }

    public static async Task<Ok<IReadOnlyList<IdeiaResumoDto>>> ListarIdeias(ISender sender, EstadoIdeia? estado, bool? soMinhas)
        => TypedResults.Ok(await sender.Send(new ListarIdeiasQuery { Estado = estado, SoMinhas = soMinhas ?? false }));

    public static async Task<Ok<IdeiaDetalheDto>> ObterIdeia(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new ObterIdeiaQuery(id)));

    public static async Task<Created<IdeiaCriadaDto>> CriarIdeia(ISender sender, CriarIdeiaCommand command)
    {
        var criada = await sender.Send(command);
        return TypedResults.Created($"/api/{nameof(Ideias)}/{criada.Id}", criada);
    }

    public static async Task<NoContent> ConfirmarAutoria(ISender sender, int id)
    {
        await sender.Send(new ConfirmarAutoriaCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> ValidarIdeia(ISender sender, int id)
    {
        await sender.Send(new ValidarIdeiaCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> FecharDiscussao(ISender sender, int id)
    {
        await sender.Send(new FecharDiscussaoCommand(id));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<bool>> AlternarGosto(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new AlternarGostoIdeiaCommand(id)));

    public record ComentarioPedido(string Texto);

    public static async Task<NoContent> Comentar(ISender sender, int id, ComentarioPedido pedido)
    {
        await sender.Send(new ComentarIdeiaCommand(id, pedido.Texto));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<bool>> AlternarGostoComentario(ISender sender, int comentarioId)
        => TypedResults.Ok(await sender.Send(new AlternarGostoComentarioCommand(comentarioId)));

    public record AvaliacaoPedido(int Custo, int Enquadramento, int Beneficio, int AdequacaoTecnica, int Incerteza, DateOnly? DataReuniaoCE, string? Observacoes);

    public static async Task<Ok<IdeiaResumoNotaDto>> RegistarAvaliacao(ISender sender, int id, AvaliacaoPedido pedido)
        => TypedResults.Ok(await sender.Send(new RegistarAvaliacaoCommand
        {
            IdeiaId = id,
            Custo = pedido.Custo,
            Enquadramento = pedido.Enquadramento,
            Beneficio = pedido.Beneficio,
            AdequacaoTecnica = pedido.AdequacaoTecnica,
            Incerteza = pedido.Incerteza,
            DataReuniaoCE = pedido.DataReuniaoCE,
            Observacoes = pedido.Observacoes,
        }));

    public record DecisaoDstPedido(bool Aprovada);

    public static async Task<NoContent> RegistarDecisaoDst(ISender sender, int id, DecisaoDstPedido pedido)
    {
        await sender.Send(new RegistarDecisaoDstCommand(id, pedido.Aprovada));
        return TypedResults.NoContent();
    }

    public record ClassePedido(ClasseIdeia Classe, string? Status);

    public static async Task<Ok<int?>> DefinirClasse(ISender sender, int id, ClassePedido pedido)
        => TypedResults.Ok(await sender.Send(new DefinirClasseCommand(id, pedido.Classe, pedido.Status)));

    public static async Task<Results<Ok<AnexoDto>, BadRequest<string>>> AdicionarAnexo(ISender sender, int id, IFormFile ficheiro)
    {
        if (ficheiro.Length > Domain.Entities.AnexoIdeia.TamanhoMaximoBytes)
        {
            return TypedResults.BadRequest("O anexo tem de ter no máximo 25 MB.");
        }

        await using var conteudo = ficheiro.OpenReadStream();
        var anexo = await sender.Send(new AdicionarAnexoCommand(id, ficheiro.FileName, ficheiro.ContentType, ficheiro.Length, conteudo));
        return TypedResults.Ok(anexo);
    }

    public static async Task<FileStreamHttpResult> ObterAnexo(ISender sender, int anexoId)
    {
        var ficheiro = await sender.Send(new ObterAnexoQuery(anexoId));
        return TypedResults.File(ficheiro.Conteudo, ficheiro.TipoConteudo, ficheiro.NomeFicheiro);
    }
}
