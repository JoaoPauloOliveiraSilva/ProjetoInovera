using Innovera.Application.Common.Models;

namespace Innovera.Application.Administracao;

public record PerfisDto(IReadOnlyList<PerfilInfo> Perfis, IReadOnlyList<PermissaoInfo> Permissoes);

/// <summary>Perfis com as suas permissões e o catálogo de permissões (backoffice).</summary>
[Authorize]
public record ListarPerfisQuery : IRequest<PerfisDto>;

public class ListarPerfisQueryHandler(IContas contas, IUtilizadores utilizadores) : IRequestHandler<ListarPerfisQuery, PerfisDto>
{
    public async Task<PerfisDto> Handle(ListarPerfisQuery request, CancellationToken cancellationToken)
    {
        // Quem gere utilizadores também precisa da lista de perfis para os atribuir.
        if (!await utilizadores.TemPermissaoAsync(Permissoes.Administracao.GerirPerfis, cancellationToken)
            && !await utilizadores.TemPermissaoAsync(Permissoes.Administracao.GerirUtilizadores, cancellationToken))
        {
            throw new Common.Exceptions.ForbiddenAccessException();
        }

        return new PerfisDto(await contas.ListarPerfisAsync(), Permissoes.Todas);
    }
}

[Authorize(Permissao = Permissoes.Administracao.GerirPerfis)]
public record CriarPerfilCommand(string Nome) : IRequest;

public class CriarPerfilCommandValidator : AbstractValidator<CriarPerfilCommand>
{
    public CriarPerfilCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(50)
            .Matches(@"^[\p{L}0-9][\p{L}0-9 ._-]*$").WithMessage("O nome só pode ter letras, números, espaços, pontos e hífenes.");
    }
}

public class CriarPerfilCommandHandler(IContas contas) : IRequestHandler<CriarPerfilCommand>
{
    public async Task Handle(CriarPerfilCommand request, CancellationToken cancellationToken)
        => (await contas.CriarPerfilAsync(request.Nome.Trim())).Garantir();
}

/// <summary>Substitui as permissões de um perfil (o Administrador tem sempre todas e não se altera).</summary>
[Authorize(Permissao = Domain.Constants.Permissoes.Administracao.GerirPerfis)]
public record DefinirPermissoesCommand(string Perfil, IReadOnlyList<string> Permissoes) : IRequest;

public class DefinirPermissoesCommandValidator : AbstractValidator<DefinirPermissoesCommand>
{
    public DefinirPermissoesCommandValidator()
    {
        RuleFor(c => c.Perfil).NotEmpty();
        RuleForEach(c => c.Permissoes).Must(Permissoes.Existe).WithMessage("Permissão desconhecida: {PropertyValue}.");
        RuleFor(c => c.Perfil).NotEqual(Roles.Administrador).WithMessage("O Administrador tem sempre todas as permissões.");
    }
}

public class DefinirPermissoesCommandHandler(IContas contas) : IRequestHandler<DefinirPermissoesCommand>
{
    public async Task Handle(DefinirPermissoesCommand request, CancellationToken cancellationToken)
        => (await contas.DefinirPermissoesAsync(request.Perfil, request.Permissoes.Distinct().ToList())).Garantir();
}

/// <summary>Apaga um perfil criado no backoffice (não os do sistema, nem perfis com utilizadores).</summary>
[Authorize(Permissao = Permissoes.Administracao.GerirPerfis)]
public record ApagarPerfilCommand(string Nome) : IRequest;

public class ApagarPerfilCommandHandler(IContas contas) : IRequestHandler<ApagarPerfilCommand>
{
    public async Task Handle(ApagarPerfilCommand request, CancellationToken cancellationToken)
        => (await contas.ApagarPerfilAsync(request.Nome)).Garantir();
}
