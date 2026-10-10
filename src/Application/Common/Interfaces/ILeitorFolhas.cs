namespace Innovera.Application.Common.Interfaces;

/// <summary>Uma folha de um Excel: cada linha é um array de valores (texto, número ou booleano; vazio = null).</summary>
public record FolhaLida(string Nome, IReadOnlyList<object?[]> Linhas);

/// <summary>Lê os valores de um ficheiro .xlsx (sem fórmulas nem formatação).</summary>
public interface ILeitorFolhas
{
    IReadOnlyList<FolhaLida> Ler(Stream conteudo);
}
