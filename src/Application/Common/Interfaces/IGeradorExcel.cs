namespace Innovera.Application.Common.Interfaces;

/// <summary>Uma folha do Excel: título por cima, cabeçalhos e linhas (texto, números, datas ou sim/não).</summary>
public record FolhaExcel(string Nome, string Titulo, IReadOnlyList<string> Cabecalhos, IReadOnlyList<IReadOnlyList<object?>> Linhas);

/// <summary>Gera um ficheiro .xlsx (os outputs dos modelos da dstelecom).</summary>
public interface IGeradorExcel
{
    byte[] Gerar(IReadOnlyList<FolhaExcel> folhas);
}
