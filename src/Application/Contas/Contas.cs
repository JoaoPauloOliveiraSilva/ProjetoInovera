using Innovera.Application.Common.Models;

namespace Innovera.Application.Contas;

// Pedidos sem sessão iniciada (link do email). O login e o logout ficam no endpoint (SignInManager).

/// <summary>A pessoa abre o link do convite e define a password. Devolve o email (para entrar logo a seguir).</summary>
public record AceitarConviteCommand(string UtilizadorId, string Token, string Password) : IRequest<string>;

public class AceitarConviteCommandValidator : AbstractValidator<AceitarConviteCommand>
{
    public AceitarConviteCommandValidator()
    {
        RuleFor(c => c.UtilizadorId).NotEmpty();
        RuleFor(c => c.Token).NotEmpty();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8).WithMessage("A password tem de ter pelo menos 8 caracteres.");
    }
}

public class AceitarConviteCommandHandler(IContas contas) : IRequestHandler<AceitarConviteCommand, string>
{
    public async Task<string> Handle(AceitarConviteCommand request, CancellationToken cancellationToken)
    {
        var (resultado, email) = await contas.AceitarConviteAsync(request.UtilizadorId, request.Token, request.Password);
        resultado.Garantir();
        return email!;
    }
}

/// <summary>"Esqueci-me da password". Responde sempre da mesma forma (não revela se o email existe).</summary>
public record RecuperarPasswordCommand(string Email) : IRequest;

public class RecuperarPasswordCommandValidator : AbstractValidator<RecuperarPasswordCommand>
{
    public RecuperarPasswordCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress();
    }
}

public class RecuperarPasswordCommandHandler(IContas contas) : IRequestHandler<RecuperarPasswordCommand>
{
    public Task Handle(RecuperarPasswordCommand request, CancellationToken cancellationToken)
        => contas.PedirRecuperacaoAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);
}

public record RedefinirPasswordCommand(string UtilizadorId, string Token, string Password) : IRequest<string>;

public class RedefinirPasswordCommandValidator : AbstractValidator<RedefinirPasswordCommand>
{
    public RedefinirPasswordCommandValidator()
    {
        RuleFor(c => c.UtilizadorId).NotEmpty();
        RuleFor(c => c.Token).NotEmpty();
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8).WithMessage("A password tem de ter pelo menos 8 caracteres.");
    }
}

public class RedefinirPasswordCommandHandler(IContas contas) : IRequestHandler<RedefinirPasswordCommand, string>
{
    public async Task<string> Handle(RedefinirPasswordCommand request, CancellationToken cancellationToken)
    {
        var (resultado, email) = await contas.RedefinirPasswordAsync(request.UtilizadorId, request.Token, request.Password);
        resultado.Garantir();
        return email!;
    }
}
