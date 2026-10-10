using Innovera.Application.Contas;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;

namespace Innovera.Web.Endpoints;

/// <summary>Entrar, sair, aceitar o convite e recuperar a password (cookie de sessão do ASP.NET Core Identity).</summary>
public class Conta : IEndpointGroup
{
    public static void Map(RouteGroupBuilder groupBuilder)
    {
        groupBuilder.MapPost(Entrar, "entrar").AllowAnonymous();
        groupBuilder.MapPost(Sair, "sair").RequireAuthorization();
        groupBuilder.MapPost(AceitarConvite, "aceitar-convite").AllowAnonymous();
        groupBuilder.MapPost(RecuperarPassword, "recuperar-password").AllowAnonymous();
        groupBuilder.MapPost(RedefinirPassword, "redefinir-password").AllowAnonymous();
    }

    public record EntrarPedido(string Email, string Password);

    public static async Task<Results<NoContent, ProblemHttpResult>> Entrar(
        SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, EntrarPedido pedido)
    {
        var conta = await userManager.FindByEmailAsync(pedido.Email.Trim());
        if (conta is null)
        {
            return Recusado("Email ou password errados.");
        }

        var resultado = await signInManager.PasswordSignInAsync(conta, pedido.Password, isPersistent: true, lockoutOnFailure: true);
        if (resultado.Succeeded)
        {
            return TypedResults.NoContent();
        }

        if (resultado.IsLockedOut)
        {
            return Recusado(conta.LockoutEnd > DateTimeOffset.UtcNow.AddYears(100)
                ? "Esta conta está desativada. Fale com a equipa de Inovação."
                : "Demasiadas tentativas erradas. A conta fica bloqueada durante 15 minutos.");
        }

        if (resultado.IsNotAllowed)
        {
            return Recusado("Ainda não aceitou o convite. Use o link que recebeu por email para definir a password.");
        }

        return Recusado("Email ou password errados.");
    }

    public static async Task<NoContent> Sair(SignInManager<ApplicationUser> signInManager)
    {
        await signInManager.SignOutAsync();
        return TypedResults.NoContent();
    }

    /// <summary>Define a password a partir do link do convite e entra logo.</summary>
    public static async Task<NoContent> AceitarConvite(ISender sender, SignInManager<ApplicationUser> signInManager, AceitarConviteCommand command)
    {
        var email = await sender.Send(command);
        await EntrarDepoisDeDefinirAsync(signInManager, email);
        return TypedResults.NoContent();
    }

    /// <summary>Responde sempre 204, exista ou não uma conta com esse email.</summary>
    public static async Task<NoContent> RecuperarPassword(ISender sender, RecuperarPasswordCommand command)
    {
        await sender.Send(command);
        return TypedResults.NoContent();
    }

    public static async Task<NoContent> RedefinirPassword(ISender sender, SignInManager<ApplicationUser> signInManager, RedefinirPasswordCommand command)
    {
        var email = await sender.Send(command);
        await EntrarDepoisDeDefinirAsync(signInManager, email);
        return TypedResults.NoContent();
    }

    private static async Task EntrarDepoisDeDefinirAsync(SignInManager<ApplicationUser> signInManager, string email)
    {
        var conta = await signInManager.UserManager.FindByEmailAsync(email);
        if (conta is not null)
        {
            await signInManager.SignInAsync(conta, isPersistent: true);
        }
    }

    private static ProblemHttpResult Recusado(string mensagem)
        => TypedResults.Problem(title: "Não foi possível entrar.", detail: mensagem, statusCode: StatusCodes.Status401Unauthorized);
}
