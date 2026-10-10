using System.Net;
using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Entities;
using Innovera.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Innovera.Infrastructure.Services;

/// <summary>Grava os avisos (sino) e envia o email quando pedido. Um email que falhe não estraga a operação.</summary>
public class ServicoNotificacoes : INotificacoes
{
    private readonly ApplicationDbContext _context;
    private readonly IEmail _email;
    private readonly IIdentityService _identityService;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _relogio;
    private readonly ILogger<ServicoNotificacoes> _logger;

    public ServicoNotificacoes(ApplicationDbContext context, IEmail email, IIdentityService identityService, IConfiguration configuration,
        TimeProvider relogio, ILogger<ServicoNotificacoes> logger)
    {
        _context = context;
        _email = email;
        _identityService = identityService;
        _configuration = configuration;
        _relogio = relogio;
        _logger = logger;
    }

    public async Task NotificarAsync(IEnumerable<int> utilizadorIds, string titulo, string mensagem, string? link, bool enviarEmail,
        CancellationToken cancellationToken)
    {
        var ids = utilizadorIds.Distinct().ToList();
        if (ids.Count == 0)
        {
            return;
        }

        var destinatarios = await _context.Utilizadores.Where(u => ids.Contains(u.Id) && u.Ativo).ToListAsync(cancellationToken);
        var agora = _relogio.GetUtcNow();
        var notificacoes = destinatarios.Select(u => new Notificacao
        {
            DestinatarioId = u.Id,
            Titulo = Cortar(titulo, 200),
            Mensagem = Cortar(mensagem, 1000),
            Link = link,
            Data = agora,
        }).ToList();
        _context.Notificacoes.AddRange(notificacoes);
        await _context.SaveChangesAsync(cancellationToken);

        if (!enviarEmail)
        {
            return;
        }

        foreach (var (utilizador, notificacao) in destinatarios.Zip(notificacoes))
        {
            try
            {
                await _email.EnviarAsync(Mensagem(utilizador, titulo, mensagem, link), cancellationToken);
                notificacao.EmailEnviado = true;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Não foi possível enviar o email \"{Titulo}\" para {Email}.", titulo, utilizador.Email);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task NotificarComPermissaoAsync(string permissao, string titulo, string mensagem, string? link, CancellationToken cancellationToken)
    {
        var contas = await _identityService.ContasComPermissaoAsync(permissao);
        var ids = await _context.Utilizadores.Where(u => contas.Contains(u.IdentityId)).Select(u => u.Id).ToListAsync(cancellationToken);
        await NotificarAsync(ids, titulo, mensagem, link, enviarEmail: false, cancellationToken);
    }

    private MensagemEmail Mensagem(Utilizador para, string titulo, string mensagem, string? link)
    {
        var site = (_configuration["Email:UrlSite"] ?? "http://localhost:5173").TrimEnd('/');
        var url = link is null ? site : site + link;
        var texto = $"Olá {para.Nome},\n\n{mensagem}\n\n{url}\n\nEquipa de Inovação";
        var html = $"""
            <div style="font-family:Arial,sans-serif;color:#222;max-width:560px">
              <div style="background:#006d6f;color:#fff;padding:18px 24px;font-size:18px;font-weight:bold">Inovação · dstelecom</div>
              <div style="padding:24px;background:#f3f3f3">
                <p>Olá {WebUtility.HtmlEncode(para.Nome)},</p>
                <p>{WebUtility.HtmlEncode(mensagem)}</p>
                <p style="margin:28px 0"><a href="{WebUtility.HtmlEncode(url)}" style="background:#c8102e;color:#fff;padding:12px 22px;border-radius:22px;text-decoration:none;font-weight:bold">abrir na plataforma</a></p>
              </div>
            </div>
            """;
        return new MensagemEmail(para.Email, para.Nome, titulo, html, texto);
    }

    private static string Cortar(string texto, int maximo) => texto.Length <= maximo ? texto : texto[..(maximo - 1)] + "…";
}
