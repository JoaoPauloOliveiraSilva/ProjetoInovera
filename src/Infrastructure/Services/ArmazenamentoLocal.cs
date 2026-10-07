using Innovera.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Innovera.Infrastructure.Services;

/// <summary>
/// Guarda os ficheiros numa pasta do servidor (configuração "Armazenamento:Pasta", por omissão "ficheiros").
/// Na AWS será trocado por uma implementação para o bucket do Lightsail (S3), sem mexer no resto.
/// </summary>
public class ArmazenamentoLocal : IArmazenamentoFicheiros
{
    private readonly string _raiz;

    public ArmazenamentoLocal(IConfiguration configuration, IHostEnvironment environment)
    {
        var pasta = configuration["Armazenamento:Pasta"] ?? "ficheiros";
        _raiz = Path.GetFullPath(Path.IsPathRooted(pasta) ? pasta : Path.Combine(environment.ContentRootPath, pasta));
    }

    public async Task<string> GuardarAsync(Stream conteudo, string pasta, string nomeFicheiro, CancellationToken cancellationToken)
    {
        var nomeSeguro = string.Concat(Path.GetFileName(nomeFicheiro).Split(Path.GetInvalidFileNameChars()));
        var chave = $"{pasta}/{Guid.NewGuid():N}-{nomeSeguro}";
        var caminho = CaminhoDe(chave);

        Directory.CreateDirectory(Path.GetDirectoryName(caminho)!);
        await using var destino = File.Create(caminho);
        await conteudo.CopyToAsync(destino, cancellationToken);

        return chave;
    }

    public Task<Stream?> AbrirAsync(string chave, CancellationToken cancellationToken)
    {
        var caminho = CaminhoDe(chave);
        Stream? stream = File.Exists(caminho) ? File.OpenRead(caminho) : null;
        return Task.FromResult(stream);
    }

    private string CaminhoDe(string chave)
    {
        var caminho = Path.GetFullPath(Path.Combine(_raiz, chave));
        if (!caminho.StartsWith(_raiz, StringComparison.Ordinal))
        {
            throw new UnauthorizedAccessException("Chave de ficheiro inválida.");
        }

        return caminho;
    }
}
