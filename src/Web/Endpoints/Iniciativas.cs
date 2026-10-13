using Innovera.Application.Charters;
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

        // Links para documentos no OneDrive (RF15)
        groupBuilder.MapPost(AdicionarLink, "{id:int}/links");
        groupBuilder.MapDelete(RemoverLink, "{id:int}/links/{linkId:int}");

        // Project Charter (6 separadores)
        groupBuilder.MapGet(ObterCharter, "{id:int}/charter");
        groupBuilder.MapPut(GuardarCharterGeral, "{id:int}/charter/geral");
        groupBuilder.MapPost(CriarTarefa, "{id:int}/charter/tarefas");
        groupBuilder.MapPut(EditarTarefa, "{id:int}/charter/tarefas/{itemId:int}");
        groupBuilder.MapPost(CriarEntrega, "{id:int}/charter/entregas");
        groupBuilder.MapPut(EditarEntrega, "{id:int}/charter/entregas/{itemId:int}");
        groupBuilder.MapPost(CriarRisco, "{id:int}/charter/riscos");
        groupBuilder.MapPut(EditarRisco, "{id:int}/charter/riscos/{itemId:int}");
        groupBuilder.MapPost(CriarLinhaOrcamento, "{id:int}/charter/orcamento");
        groupBuilder.MapPut(EditarLinhaOrcamento, "{id:int}/charter/orcamento/{itemId:int}");
        groupBuilder.MapPost(CriarRequisito, "{id:int}/charter/conformidade");
        groupBuilder.MapPut(EditarRequisito, "{id:int}/charter/conformidade/{itemId:int}");
        groupBuilder.MapPost(CriarLicao, "{id:int}/charter/licoes");
        groupBuilder.MapPut(EditarLicao, "{id:int}/charter/licoes/{itemId:int}");
        groupBuilder.MapDelete(ApagarItemCharter, "{id:int}/charter/{seccao}/{itemId:int}");

        // Equipa de trabalho e alocação mensal (Charter › planeamento)
        groupBuilder.MapPost(CriarMembro, "{id:int}/equipa");
        groupBuilder.MapPut(EditarMembro, "{id:int}/equipa/{membroId:int}");
        groupBuilder.MapDelete(ApagarMembro, "{id:int}/equipa/{membroId:int}");
        groupBuilder.MapPut(GuardarAlocacoes, "{id:int}/equipa/{membroId:int}/alocacoes/{ano:int}");

        // Vigilâncias: execuções (prevista/realizada, resultado → Mod.237) e parceiros da iniciativa
        groupBuilder.MapGet(ListarExecucoes, "{id:int}/execucoes");
        groupBuilder.MapPost(CriarExecucao, "{id:int}/execucoes");
        groupBuilder.MapPut(EditarExecucao, "{id:int}/execucoes/{execucaoId:int}");
        groupBuilder.MapDelete(ApagarExecucao, "{id:int}/execucoes/{execucaoId:int}");
        groupBuilder.MapPut(DefinirParceiros, "{id:int}/parceiros");
    }

    public static async Task<Ok<IReadOnlyList<IniciativaResumoDto>>> ListarIniciativas(
        ISender sender, TipoIniciativa? tipo, EstadoIniciativa? estado, string? texto, PilarEstrategico? pilar, int? ano)
        => TypedResults.Ok(await sender.Send(new ListarIniciativasQuery(tipo, estado, texto, pilar, ano)));

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

    public record LinkPedido(string Titulo, string Url);

    public static async Task<Ok<int>> AdicionarLink(ISender sender, int id, LinkPedido pedido)
        => TypedResults.Ok(await sender.Send(new AdicionarLinkCommand(id, pedido.Titulo, pedido.Url)));

    public static async Task<NoContent> RemoverLink(ISender sender, int id, int linkId)
    {
        await sender.Send(new RemoverLinkCommand(id, linkId));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<ProjectCharterDto>> ObterCharter(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new ObterCharterQuery(id)));

    public static async Task<NoContent> GuardarCharterGeral(ISender sender, int id, CharterGeralDto dados)
    {
        await sender.Send(new GuardarCharterGeralCommand(id, dados));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<int>> CriarTarefa(ISender sender, int id, TarefaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarTarefaCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarTarefa(ISender sender, int id, int itemId, TarefaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarTarefaCommand(id, itemId, dados)));

    public static async Task<Ok<int>> CriarEntrega(ISender sender, int id, EntregaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarEntregaCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarEntrega(ISender sender, int id, int itemId, EntregaDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarEntregaCommand(id, itemId, dados)));

    public static async Task<Ok<int>> CriarRisco(ISender sender, int id, RiscoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarRiscoCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarRisco(ISender sender, int id, int itemId, RiscoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarRiscoCommand(id, itemId, dados)));

    public static async Task<Ok<int>> CriarLinhaOrcamento(ISender sender, int id, LinhaOrcamentoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLinhaOrcamentoCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarLinhaOrcamento(ISender sender, int id, int itemId, LinhaOrcamentoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLinhaOrcamentoCommand(id, itemId, dados)));

    public static async Task<Ok<int>> CriarRequisito(ISender sender, int id, RequisitoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarRequisitoCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarRequisito(ISender sender, int id, int itemId, RequisitoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarRequisitoCommand(id, itemId, dados)));

    public static async Task<Ok<int>> CriarLicao(ISender sender, int id, LicaoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLicaoCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarLicao(ISender sender, int id, int itemId, LicaoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarLicaoCommand(id, itemId, dados)));

    private static readonly Dictionary<string, SeccaoCharter> Seccoes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tarefas"] = SeccaoCharter.Tarefa,
        ["entregas"] = SeccaoCharter.Entrega,
        ["riscos"] = SeccaoCharter.Risco,
        ["orcamento"] = SeccaoCharter.Orcamento,
        ["conformidade"] = SeccaoCharter.Conformidade,
        ["licoes"] = SeccaoCharter.Licao,
    };

    public static async Task<Results<NoContent, NotFound>> ApagarItemCharter(ISender sender, int id, string seccao, int itemId)
    {
        if (!Seccoes.TryGetValue(seccao, out var s))
        {
            return TypedResults.NotFound();
        }

        await sender.Send(new ApagarItemCharterCommand(id, s, itemId));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<int>> CriarMembro(ISender sender, int id, MembroDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarMembroCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarMembro(ISender sender, int id, int membroId, MembroDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarMembroCommand(id, membroId, dados)));

    public static async Task<NoContent> ApagarMembro(ISender sender, int id, int membroId)
    {
        await sender.Send(new ApagarMembroCommand(id, membroId));
        return TypedResults.NoContent();
    }

    public record AlocacoesPedido(IReadOnlyList<decimal?> Meses);

    public static async Task<NoContent> GuardarAlocacoes(ISender sender, int id, int membroId, int ano, AlocacoesPedido pedido)
    {
        await sender.Send(new GuardarAlocacoesCommand(id, membroId, ano, pedido.Meses));
        return TypedResults.NoContent();
    }

    public static async Task<Ok<IReadOnlyList<ExecucaoDto>>> ListarExecucoes(ISender sender, int id)
        => TypedResults.Ok(await sender.Send(new ListarExecucoesQuery(id)));

    public static async Task<Ok<int>> CriarExecucao(ISender sender, int id, ExecucaoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarExecucaoCommand(id, null, dados)));

    public static async Task<Ok<int>> EditarExecucao(ISender sender, int id, int execucaoId, ExecucaoDados dados)
        => TypedResults.Ok(await sender.Send(new GuardarExecucaoCommand(id, execucaoId, dados)));

    public static async Task<NoContent> ApagarExecucao(ISender sender, int id, int execucaoId)
    {
        await sender.Send(new ApagarExecucaoCommand(id, execucaoId));
        return TypedResults.NoContent();
    }

    public record ParceirosPedido(IReadOnlyList<int> ParceirosIds);

    public static async Task<NoContent> DefinirParceiros(ISender sender, int id, ParceirosPedido pedido)
    {
        await sender.Send(new DefinirParceirosIniciativaCommand(id, pedido.ParceirosIds));
        return TypedResults.NoContent();
    }
}
