using Microsoft.AspNetCore.Identity;

namespace Innovera.Infrastructure.Identity;

/// <summary>Mensagens do ASP.NET Core Identity em português (as que aparecem a quem usa a plataforma).</summary>
public class IdentityErrorDescriberPt : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => Erro(nameof(DefaultError), "Ocorreu um erro.");
    public override IdentityError InvalidToken() => Erro(nameof(InvalidToken), "O link já não é válido (expirou ou já foi usado). Peça um novo.");
    public override IdentityError PasswordMismatch() => Erro(nameof(PasswordMismatch), "Password errada.");
    public override IdentityError InvalidEmail(string? email) => Erro(nameof(InvalidEmail), $"O email \"{email}\" não é válido.");
    public override IdentityError InvalidUserName(string? userName) => Erro(nameof(InvalidUserName), $"O email \"{userName}\" não é válido.");
    public override IdentityError DuplicateEmail(string email) => Erro(nameof(DuplicateEmail), $"Já existe uma conta com o email {email}.");
    public override IdentityError DuplicateUserName(string userName) => Erro(nameof(DuplicateUserName), $"Já existe uma conta com o email {userName}.");
    public override IdentityError InvalidRoleName(string? role) => Erro(nameof(InvalidRoleName), $"O nome de perfil \"{role}\" não é válido.");
    public override IdentityError DuplicateRoleName(string role) => Erro(nameof(DuplicateRoleName), $"Já existe o perfil {role}.");
    public override IdentityError UserAlreadyHasPassword() => Erro(nameof(UserAlreadyHasPassword), "Esta conta já tem password.");
    public override IdentityError UserAlreadyInRole(string role) => Erro(nameof(UserAlreadyInRole), $"Já tem o perfil {role}.");
    public override IdentityError UserNotInRole(string role) => Erro(nameof(UserNotInRole), $"Não tem o perfil {role}.");
    public override IdentityError PasswordTooShort(int length) => Erro(nameof(PasswordTooShort), $"A password tem de ter pelo menos {length} caracteres.");
    public override IdentityError PasswordRequiresDigit() => Erro(nameof(PasswordRequiresDigit), "A password tem de ter pelo menos um número.");
    public override IdentityError PasswordRequiresLower() => Erro(nameof(PasswordRequiresLower), "A password tem de ter pelo menos uma letra minúscula.");
    public override IdentityError PasswordRequiresUpper() => Erro(nameof(PasswordRequiresUpper), "A password tem de ter pelo menos uma letra maiúscula.");
    public override IdentityError PasswordRequiresNonAlphanumeric() => Erro(nameof(PasswordRequiresNonAlphanumeric), "A password tem de ter pelo menos um símbolo.");
    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => Erro(nameof(PasswordRequiresUniqueChars), $"A password tem de ter pelo menos {uniqueChars} caracteres diferentes.");

    private static IdentityError Erro(string codigo, string descricao) => new() { Code = codigo, Description = descricao };
}
