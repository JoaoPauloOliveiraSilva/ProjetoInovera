namespace Innovera.Application.Common.Validacao;

/// <summary>Links para o OneDrive / SharePoint (RF15): a plataforma só guarda o endereço.</summary>
public static class ValidacaoLinks
{
    public static bool EValido(string? url)
        => Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp);
}
