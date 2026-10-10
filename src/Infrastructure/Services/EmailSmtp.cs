using System.Net;
using System.Net.Mail;
using System.Text;
using Innovera.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Innovera.Infrastructure.Services;

/// <summary>
/// Envia emails por SMTP. Configuração:
/// <list type="bullet">
/// <item>"Email:Remetente" / "Email:NomeRemetente" — quem envia (ex.: inovacao@...);</item>
/// <item>"Smtp:Host", "Smtp:Porta", "Smtp:Ssl", "Smtp:Utilizador", "Smtp:Password".</item>
/// </list>
/// Em desenvolvimento usa o Mailpit (servidor de testes, os emails veem-se em http://localhost:8025);
/// na AWS usa o endpoint SMTP do Amazon SES. Sem "Smtp:Host" o email só é escrito no log.
/// </summary>
public class EmailSmtp : IEmail
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailSmtp> _logger;

    public EmailSmtp(IConfiguration configuration, ILogger<EmailSmtp> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken)
    {
        var host = _configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("SMTP não configurado: email para {Para} não enviado. Assunto: {Assunto}\n{Texto}",
                mensagem.Para, mensagem.Assunto, mensagem.Texto);
            return;
        }

        var remetente = _configuration["Email:Remetente"] ?? "inovacao@innovera.local";
        var nomeRemetente = _configuration["Email:NomeRemetente"] ?? "Inovação dstelecom";

        using var email = new MailMessage
        {
            From = new MailAddress(remetente, nomeRemetente),
            Subject = mensagem.Assunto,
            Body = mensagem.Texto,
            IsBodyHtml = false,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8,
            HeadersEncoding = Encoding.UTF8,
        };
        email.To.Add(new MailAddress(mensagem.Para, mensagem.NomePara));
        email.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(mensagem.Html, Encoding.UTF8, "text/html"));

        using var cliente = new SmtpClient(host, _configuration.GetValue("Smtp:Porta", 587))
        {
            EnableSsl = _configuration.GetValue("Smtp:Ssl", true),
        };
        var utilizador = _configuration["Smtp:Utilizador"];
        if (!string.IsNullOrEmpty(utilizador))
        {
            cliente.Credentials = new NetworkCredential(utilizador, _configuration["Smtp:Password"]);
        }

        await cliente.SendMailAsync(email, cancellationToken);
        _logger.LogInformation("Email \"{Assunto}\" enviado para {Para}.", mensagem.Assunto, mensagem.Para);
    }
}
