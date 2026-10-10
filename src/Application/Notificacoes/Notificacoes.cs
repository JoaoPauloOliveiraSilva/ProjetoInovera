namespace Innovera.Application.Notificacoes;

public record NotificacaoDto(int Id, string Titulo, string Mensagem, string? Link, DateTimeOffset Data, bool Lida);

public record NotificacoesDto(int NaoLidas, IReadOnlyList<NotificacaoDto> Itens);

/// <summary>Os avisos do utilizador atual (os 30 mais recentes) e quantos estão por ler.</summary>
[Authorize]
public record ListarNotificacoesQuery : IRequest<NotificacoesDto>;

public class ListarNotificacoesQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    : IRequestHandler<ListarNotificacoesQuery, NotificacoesDto>
{
    public async Task<NotificacoesDto> Handle(ListarNotificacoesQuery request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var minhas = context.Notificacoes.AsNoTracking().Where(n => n.DestinatarioId == eu.Id);

        var naoLidas = await minhas.CountAsync(n => !n.Lida, cancellationToken);
        var itens = await minhas
            .OrderByDescending(n => n.Data).ThenByDescending(n => n.Id)
            .Take(30)
            .Select(n => new NotificacaoDto(n.Id, n.Titulo, n.Mensagem, n.Link, n.Data, n.Lida))
            .ToListAsync(cancellationToken);

        return new NotificacoesDto(naoLidas, itens);
    }
}

/// <summary>Marca um aviso como lido (ou todos, se <see cref="Id"/> for nulo).</summary>
[Authorize]
public record MarcarNotificacoesLidasCommand(int? Id) : IRequest;

public class MarcarNotificacoesLidasCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    : IRequestHandler<MarcarNotificacoesLidasCommand>
{
    public async Task Handle(MarcarNotificacoesLidasCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var porLer = await context.Notificacoes
            .Where(n => n.DestinatarioId == eu.Id && !n.Lida && (request.Id == null || n.Id == request.Id))
            .ToListAsync(cancellationToken);

        foreach (var notificacao in porLer)
        {
            notificacao.Lida = true;
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
