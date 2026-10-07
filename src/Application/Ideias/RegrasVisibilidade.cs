namespace Innovera.Application.Ideias;

internal static class RegrasVisibilidade
{
    /// <summary>
    /// A equipa de Inovação vê tudo. Os autores veem as suas ideias. Os restantes veem as ideias publicadas
    /// (em discussão, em avaliação ou decididas) que não sejam privadas.
    /// </summary>
    public static bool PodeVer(bool eAdministrador, int utilizadorId, string? identityId, int? autorId, string? criadaPor,
        IEnumerable<int> coautores, bool privada, EstadoIdeia estado)
    {
        if (eAdministrador || autorId == utilizadorId || (identityId is not null && criadaPor == identityId) || coautores.Contains(utilizadorId))
        {
            return true;
        }

        return !privada && estado is EstadoIdeia.EmDiscussao or EstadoIdeia.EmAvaliacao or EstadoIdeia.Aprovada or EstadoIdeia.NaoAprovada;
    }

    public static string Autores(bool anonima, string? autor, IEnumerable<string> coautores)
    {
        if (anonima)
        {
            return "Autor Anónimo";
        }

        var nomes = new List<string>();
        if (!string.IsNullOrWhiteSpace(autor))
        {
            nomes.Add(autor);
        }

        nomes.AddRange(coautores);
        return nomes.Count == 0 ? "—" : string.Join(", ", nomes);
    }
}
