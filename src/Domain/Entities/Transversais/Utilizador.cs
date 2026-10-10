namespace Innovera.Domain.Entities;

/// <summary>
/// Pessoa com acesso à plataforma. A conta (email, password, perfis) fica no ASP.NET Core Identity,
/// na camada de Infrastructure; os perfis e permissões vêm de lá.
/// </summary>
public class Utilizador : BaseAuditableEntity
{
    /// <summary>Id da conta no ASP.NET Core Identity.</summary>
    public string IdentityId { get; set; } = string.Empty;

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? NumeroColaborador { get; set; }

    public string? Departamento { get; set; }

    /// <summary>Empresa do grupo (ex.: "fiber t, s.a."), mostrada nos comentários.</summary>
    public string? Empresa { get; set; }

    public string? FotoUrl { get; set; }

    /// <summary>Falso quando o Administrador desativa a conta (deixa de poder entrar e de aparecer como coautor).</summary>
    public bool Ativo { get; set; } = true;
}
