namespace Innovera.Application.Common.Interfaces;

/// <summary>Devolve o próximo número das ideias (SEQUENCE da base de dados).</summary>
public interface IGeradorNumeroIdeia
{
    Task<int> ProximoAsync(CancellationToken cancellationToken);
}
