namespace Innovera.Application.Utilizadores;

public record UtilizadorAtualDto(int Id, string Nome, string Email, string? Empresa, PerfilUtilizador Perfil, bool EAdministrador);

public record UtilizadorResumoDto(int Id, string Nome, string? Empresa);

/// <summary>Dados do utilizador com sessão iniciada (cria o perfil na primeira vez).</summary>
[Authorize]
public record ObterUtilizadorAtualQuery : IRequest<UtilizadorAtualDto>;

public class ObterUtilizadorAtualQueryHandler(IUtilizadores utilizadores) : IRequestHandler<ObterUtilizadorAtualQuery, UtilizadorAtualDto>
{
    public async Task<UtilizadorAtualDto> Handle(ObterUtilizadorAtualQuery request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        return new UtilizadorAtualDto(eu.Id, eu.Nome, eu.Email, eu.Empresa, eu.Perfil, utilizadores.EAdministrador);
    }
}

/// <summary>Pesquisa de colegas (para indicar coautores de uma ideia).</summary>
[Authorize]
public record PesquisarUtilizadoresQuery(string? Texto) : IRequest<IReadOnlyList<UtilizadorResumoDto>>;

public class PesquisarUtilizadoresQueryHandler(IApplicationDbContext context) : IRequestHandler<PesquisarUtilizadoresQuery, IReadOnlyList<UtilizadorResumoDto>>
{
    public async Task<IReadOnlyList<UtilizadorResumoDto>> Handle(PesquisarUtilizadoresQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.Utilizadores.AsNoTracking().Where(u => u.Ativo);
        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(u => u.Nome.ToLower().Contains(texto) || u.Email.ToLower().Contains(texto));
        }

        return await consulta
            .OrderBy(u => u.Nome)
            .Take(20)
            .Select(u => new UtilizadorResumoDto(u.Id, u.Nome, u.Empresa))
            .ToListAsync(cancellationToken);
    }
}

/// <summary>Cria uma conta de trabalhador (contas próprias da plataforma, sem integração com a dst).</summary>
public record RegistarContaCommand(string Nome, string Email, string Password, string? Empresa) : IRequest<int>
{
    // Os pedidos são escritos no log (LoggingBehaviour): a password nunca aparece.
    protected virtual bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"Nome = {Nome}, Email = {Email}, Password = ***, Empresa = {Empresa}");
        return true;
    }
}

public class RegistarContaCommandValidator : AbstractValidator<RegistarContaCommand>
{
    public RegistarContaCommandValidator()
    {
        RuleFor(c => c.Nome).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.Empresa).MaximumLength(150);
    }
}

public class RegistarContaCommandHandler(IApplicationDbContext context, IIdentityService identityService)
    : IRequestHandler<RegistarContaCommand, int>
{
    public async Task<int> Handle(RegistarContaCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var (resultado, identityId) = await identityService.CreateUserAsync(email, request.Password);
        if (!resultado.Succeeded)
        {
            throw new RegraDeNegocioException(string.Join(" ", resultado.Errors));
        }

        await identityService.AddToRoleAsync(identityId, Roles.Trabalhador);

        var utilizador = new Utilizador
        {
            IdentityId = identityId,
            Nome = request.Nome.Trim(),
            Email = email,
            Empresa = string.IsNullOrWhiteSpace(request.Empresa) ? null : request.Empresa.Trim(),
            Perfil = PerfilUtilizador.Trabalhador,
        };
        context.Utilizadores.Add(utilizador);
        await context.SaveChangesAsync(cancellationToken);

        return utilizador.Id;
    }
}
