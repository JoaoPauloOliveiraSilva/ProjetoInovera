namespace Innovera.Application.Common.Interfaces;

/// <summary>Avisos aos utilizadores: no sino da plataforma e, se pedido, também por email.</summary>
public interface INotificacoes
{
    Task NotificarAsync(IEnumerable<int> utilizadorIds, string titulo, string mensagem, string? link, bool enviarEmail,
        CancellationToken cancellationToken);

    /// <summary>Avisa (só na plataforma) quem tem a permissão — ex.: a equipa de Inovação quando há ideias para validar.</summary>
    Task NotificarComPermissaoAsync(string permissao, string titulo, string mensagem, string? link, CancellationToken cancellationToken);
}
