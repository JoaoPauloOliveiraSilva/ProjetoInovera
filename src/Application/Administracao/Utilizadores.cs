using Innovera.Application.Common.Models;

namespace Innovera.Application.Administracao;

public record UtilizadorAdminDto(int Id, string Nome, string Email, string? Empresa, IReadOnlyList<string> Perfis, EstadoConta Estado);

/// <summary>Backoffice: lista de utilizadores com perfis e estado da conta.</summary>
[Authorize(Permissao = Permissoes.Administracao.GerirUtilizadores)]
public record ListarUtilizadoresQuery(string? Texto) : IRequest<IReadOnlyList<UtilizadorAdminDto>>;

public class ListarUtilizadoresQueryHandler(IApplicationDbContext context, IContas contas)
    : IRequestHandler<ListarUtilizadoresQuery, IReadOnlyList<UtilizadorAdminDto>>
{
    public async Task<IReadOnlyList<UtilizadorAdminDto>> Handle(ListarUtilizadoresQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.Utilizadores.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(u => u.Nome.ToLower().Contains(texto) || u.Email.ToLower().Contains(texto));
        }

        var utilizadores = await consulta.OrderBy(u => u.Nome).ToListAsync(cancellationToken);
        var contasInfo = await contas.ObterAsync(utilizadores.Select(u => u.IdentityId), cancellationToken);

        return utilizadores
            .Select(u =>
            {
                contasInfo.TryGetValue(u.IdentityId, out var conta);
                return new UtilizadorAdminDto(u.Id, u.Nome, u.Email, u.Empresa, conta?.Perfis ?? [], conta?.Estado ?? EstadoConta.Desativada);
            })
            .ToList();
    }
}

/// <summary>O Administrador adiciona a pessoa; ela recebe um email com o link para definir a password.</summary>
[Authorize(Permissao = Permissoes.Administracao.GerirUtilizadores)]
public record AdicionarUtilizadorCommand(string Nome, string Email, string? Empresa, IReadOnlyList<string> Perfis) : IRequest<int>;

public class AdicionarUtilizadorCommandValidator : AbstractValidator<AdicionarUtilizadorCommand>
{
    public AdicionarUtilizadorCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Empresa).MaximumLength(150);
        RuleFor(c => c.Perfis).NotEmpty().WithMessage("Escolha pelo menos um perfil.");
    }
}

public class AdicionarUtilizadorCommandHandler(IApplicationDbContext context, IContas contas)
    : IRequestHandler<AdicionarUtilizadorCommand, int>
{
    public async Task<int> Handle(AdicionarUtilizadorCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var (resultado, identityId) = await contas.CriarAsync(email, request.Perfis);
        resultado.Garantir();

        var utilizador = new Utilizador
        {
            IdentityId = identityId,
            Nome = request.Nome.Trim(),
            Email = email,
            Empresa = string.IsNullOrWhiteSpace(request.Empresa) ? null : request.Empresa.Trim(),
        };
        context.Utilizadores.Add(utilizador);
        await context.SaveChangesAsync(cancellationToken);

        await contas.EnviarConviteAsync(identityId, utilizador.Nome, cancellationToken);
        return utilizador.Id;
    }
}

/// <summary>Muda nome, empresa e perfis.</summary>
[Authorize(Permissao = Permissoes.Administracao.GerirUtilizadores)]
public record EditarUtilizadorCommand(int Id, string Nome, string? Empresa, IReadOnlyList<string> Perfis) : IRequest;

public class EditarUtilizadorCommandValidator : AbstractValidator<EditarUtilizadorCommand>
{
    public EditarUtilizadorCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Empresa).MaximumLength(150);
        RuleFor(c => c.Perfis).NotEmpty().WithMessage("Escolha pelo menos um perfil.");
    }
}

public class EditarUtilizadorCommandHandler(IApplicationDbContext context, IContas contas, IUtilizadores utilizadores)
    : IRequestHandler<EditarUtilizadorCommand>
{
    public async Task Handle(EditarUtilizadorCommand request, CancellationToken cancellationToken)
    {
        var utilizador = await context.Utilizadores.FindAsync([request.Id], cancellationToken);
        Guard.Against.NotFound(request.Id, utilizador);

        var eu = await utilizadores.AtualAsync(cancellationToken);
        if (utilizador.Id == eu.Id && !request.Perfis.Contains(Roles.Administrador))
        {
            var atuais = (await contas.ObterAsync([eu.IdentityId], cancellationToken)).GetValueOrDefault(eu.IdentityId);
            if (atuais?.Perfis.Contains(Roles.Administrador) == true)
            {
                throw new RegraDeNegocioException("Não pode tirar a si próprio o perfil de Administrador.");
            }
        }

        (await contas.DefinirPerfisAsync(utilizador.IdentityId, request.Perfis)).Garantir();

        utilizador.Nome = request.Nome.Trim();
        utilizador.Empresa = string.IsNullOrWhiteSpace(request.Empresa) ? null : request.Empresa.Trim();
        await context.SaveChangesAsync(cancellationToken);
    }
}

[Authorize(Permissao = Permissoes.Administracao.GerirUtilizadores)]
public record ReenviarConviteCommand(int Id) : IRequest;

public class ReenviarConviteCommandHandler(IApplicationDbContext context, IContas contas) : IRequestHandler<ReenviarConviteCommand>
{
    public async Task Handle(ReenviarConviteCommand request, CancellationToken cancellationToken)
    {
        var utilizador = await context.Utilizadores.FindAsync([request.Id], cancellationToken);
        Guard.Against.NotFound(request.Id, utilizador);

        var conta = (await contas.ObterAsync([utilizador.IdentityId], cancellationToken)).GetValueOrDefault(utilizador.IdentityId);
        if (conta?.Estado != EstadoConta.ConvitePendente)
        {
            throw new RegraDeNegocioException("Esta pessoa já aceitou o convite (pode usar \"esqueci-me da password\").");
        }

        await contas.EnviarConviteAsync(utilizador.IdentityId, utilizador.Nome, cancellationToken);
    }
}

/// <summary>Desativa (Ativo = false) ou volta a ativar uma conta. A pessoa deixa de conseguir entrar.</summary>
[Authorize(Permissao = Permissoes.Administracao.GerirUtilizadores)]
public record AlterarEstadoUtilizadorCommand(int Id, bool Ativo) : IRequest;

public class AlterarEstadoUtilizadorCommandHandler(IApplicationDbContext context, IContas contas, IUtilizadores utilizadores)
    : IRequestHandler<AlterarEstadoUtilizadorCommand>
{
    public async Task Handle(AlterarEstadoUtilizadorCommand request, CancellationToken cancellationToken)
    {
        var utilizador = await context.Utilizadores.FindAsync([request.Id], cancellationToken);
        Guard.Against.NotFound(request.Id, utilizador);

        var eu = await utilizadores.AtualAsync(cancellationToken);
        if (!request.Ativo && utilizador.Id == eu.Id)
        {
            throw new RegraDeNegocioException("Não pode desativar a sua própria conta.");
        }

        var resultado = request.Ativo
            ? await contas.AtivarAsync(utilizador.IdentityId)
            : await contas.DesativarAsync(utilizador.IdentityId);
        resultado.Garantir();

        utilizador.Ativo = request.Ativo;
        await context.SaveChangesAsync(cancellationToken);
    }
}
