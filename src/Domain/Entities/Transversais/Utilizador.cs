namespace Innovera.Domain.Entities;

/// <summary>
/// Trabalhador da dstelecom com acesso à plataforma.
/// A conta (login) fica na camada de Infrastructure: Entra ID ou conta local (ASP.NET Core Identity).
/// </summary>
public class Utilizador : BaseAuditableEntity
{
    /// <summary>Id da conta: object id do Entra ID ou id da conta local.</summary>
    public string IdentityId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? NumeroColaborador { get; set; }

    public string? Departamento { get; set; }

    /// <summary>Empresa do grupo (ex.: "fiber t, s.a."), mostrada nos comentários.</summary>
    public string? Empresa { get; set; }

    public string? FotoUrl { get; set; }

    public PerfilUtilizador Perfil { get; set; } = PerfilUtilizador.Trabalhador;

    public bool Ativo { get; set; } = true;
}
