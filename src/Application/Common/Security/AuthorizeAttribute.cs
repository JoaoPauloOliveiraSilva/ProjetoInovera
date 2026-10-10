namespace Innovera.Application.Common.Security;

/// <summary>
/// Specifies the class this attribute is applied to requires authorization.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AuthorizeAttribute"/> class. 
    /// </summary>
    public AuthorizeAttribute() { }

    /// <summary>
    /// Gets or sets a comma delimited list of roles that are allowed to access the resource.
    /// </summary>
    public string Roles { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the policy name that determines access to the resource.
    /// </summary>
    public string Policy { get; set; } = string.Empty;

    /// <summary>
    /// Permissão necessária (ver <c>Domain.Constants.Permissoes</c>). É a forma a usar nesta aplicação:
    /// as permissões de cada perfil são configuráveis no backoffice.
    /// </summary>
    public string Permissao { get; set; } = string.Empty;
}
