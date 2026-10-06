namespace Innovera.Domain.Constants;

/// <summary>
/// Perfis de acesso definidos no documento: Administrador (equipa de Inovação),
/// Gestor de projeto (a validar com a dstelecom) e Trabalhador (todos, como guest).
/// </summary>
public abstract class Roles
{
    public const string Administrador = nameof(Administrador);

    public const string GestorProjeto = nameof(GestorProjeto);

    public const string Trabalhador = nameof(Trabalhador);
}
