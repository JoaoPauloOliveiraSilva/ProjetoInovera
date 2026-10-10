namespace Innovera.Application.Common.Interfaces;

public record MensagemEmail(string Para, string? NomePara, string Assunto, string Html, string Texto);

/// <summary>Envio de emails (SMTP: Mailpit em desenvolvimento, Amazon SES na AWS).</summary>
public interface IEmail
{
    Task EnviarAsync(MensagemEmail mensagem, CancellationToken cancellationToken);
}
