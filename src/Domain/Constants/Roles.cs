namespace Innovera.Domain.Constants;

/// <summary>
/// Perfis criados de início (reunião de 09/10). Os perfis são configuráveis: o Administrador pode criar outros
/// e mudar as permissões de cada um no backoffice (ver <see cref="Permissoes"/>), sem mexer no código.
/// </summary>
public abstract class Roles
{
    /// <summary>"Chefe": faz tudo o que a Equipa de Inovação faz e gere a plataforma. Tem sempre todas as permissões.</summary>
    public const string Administrador = nameof(Administrador);

    /// <summary>Equipa de Inovação: valida e avalia as ideias, gere projetos e iniciativas.</summary>
    public const string EquipaInovacao = nameof(EquipaInovacao);

    /// <summary>Todos os trabalhadores: registam ideias, comentam e dão gosto.</summary>
    public const string Trabalhador = nameof(Trabalhador);

    /// <summary>Perfis que não se podem apagar.</summary>
    public static readonly IReadOnlyList<string> DoSistema = [Administrador, EquipaInovacao, Trabalhador];
}
