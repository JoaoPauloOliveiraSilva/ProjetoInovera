namespace Innovera.Application.Common.Interfaces;

/// <summary>
/// Guarda os anexos e os outputs. Em desenvolvimento usa uma pasta local; na AWS será o bucket do Lightsail (S3).
/// </summary>
public interface IArmazenamentoFicheiros
{
    /// <summary>Guarda o ficheiro e devolve a chave para o voltar a abrir.</summary>
    Task<string> GuardarAsync(Stream conteudo, string pasta, string nomeFicheiro, CancellationToken cancellationToken);

    Task<Stream?> AbrirAsync(string chave, CancellationToken cancellationToken);
}
