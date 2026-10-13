namespace Innovera.Application.Common.Interfaces;

/// <summary>
/// Uma folha de um Excel: cada linha é um array de valores (texto, número ou booleano; vazio = null).
/// <paramref name="Cores"/>: cor de fundo das células pintadas, por (linha, coluna) a contar de 0, em "RRGGBB"
/// (duas cores quando o fundo é um gradiente). Serve para o Mod.69, em que cada atividade tem a sua cor no calendário.
/// </summary>
public record FolhaLida(string Nome, IReadOnlyList<object?[]> Linhas, IReadOnlyDictionary<(int Linha, int Coluna), IReadOnlyList<string>>? Cores = null)
{
    /// <summary>Cores de fundo de uma célula (vazio se não estiver pintada).</summary>
    public IReadOnlyList<string> CoresDe(int linha, int coluna)
        => Cores is not null && Cores.TryGetValue((linha, coluna), out var cores) ? cores : Array.Empty<string>();
}

/// <summary>Lê os valores de um ficheiro .xlsx (sem fórmulas; da formatação só a cor de fundo das células).</summary>
public interface ILeitorFolhas
{
    IReadOnlyList<FolhaLida> Ler(Stream conteudo);
}
