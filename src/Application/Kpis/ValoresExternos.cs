namespace Innovera.Application.Kpis;

// Valores que não existem na plataforma e entram à mão para os KPIs (RF09): n.º de trabalhadores, receitas,
// proveitos de IDI. Por omissão só o Administrador os vê e introduz (permissão kpis.valores-externos).

public record ValorExternoDto(int Id, TipoValorExterno Tipo, int Ano, PeriodoKpi Periodo, decimal Valor, string? Observacoes);

public record ValorExternoDados(TipoValorExterno Tipo, int Ano, PeriodoKpi Periodo, decimal Valor, string? Observacoes);

[Authorize(Permissao = Permissoes.Kpis.ValoresExternos)]
public record ListarValoresExternosQuery : IRequest<IReadOnlyList<ValorExternoDto>>;

public class ListarValoresExternosQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ListarValoresExternosQuery, IReadOnlyList<ValorExternoDto>>
{
    public async Task<IReadOnlyList<ValorExternoDto>> Handle(ListarValoresExternosQuery request, CancellationToken cancellationToken)
        => await context.ValoresExternos.AsNoTracking()
            .OrderByDescending(v => v.Ano).ThenBy(v => v.Tipo).ThenBy(v => v.Periodo)
            .Select(v => new ValorExternoDto(v.Id, v.Tipo, v.Ano, v.Periodo, v.Valor, v.Observacoes))
            .ToListAsync(cancellationToken);
}

[Authorize(Permissao = Permissoes.Kpis.ValoresExternos)]
public record GuardarValorExternoCommand(int? Id, ValorExternoDados Dados) : IRequest<int>;

public class GuardarValorExternoCommandValidator : AbstractValidator<GuardarValorExternoCommand>
{
    public GuardarValorExternoCommandValidator()
    {
        RuleFor(c => c.Dados.Tipo).IsInEnum();
        RuleFor(c => c.Dados.Periodo).IsInEnum();
        RuleFor(c => c.Dados.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Dados.Valor).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Dados.Observacoes).MaximumLength(1000);
    }
}

public class GuardarValorExternoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarValorExternoCommand, int>
{
    public async Task<int> Handle(GuardarValorExternoCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dados;
        var repetido = await context.ValoresExternos.AnyAsync(v => v.Tipo == d.Tipo && v.Ano == d.Ano && v.Periodo == d.Periodo
            && (request.Id == null || v.Id != request.Id), cancellationToken);
        if (repetido)
        {
            throw new RegraDeNegocioException("Já existe um valor deste tipo para esse ano e período. Altere o que já existe.");
        }

        ValorExterno v;
        if (request.Id is null)
        {
            v = new ValorExterno();
            context.ValoresExternos.Add(v);
        }
        else
        {
            var existente = await context.ValoresExternos.FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);
            Guard.Against.NotFound(request.Id.Value, existente);
            v = existente;
        }

        v.Tipo = d.Tipo;
        v.Ano = d.Ano;
        v.Periodo = d.Periodo;
        v.Valor = d.Valor;
        v.Observacoes = string.IsNullOrWhiteSpace(d.Observacoes) ? null : d.Observacoes.Trim();
        await context.SaveChangesAsync(cancellationToken);
        return v.Id;
    }
}

[Authorize(Permissao = Permissoes.Kpis.ValoresExternos)]
public record ApagarValorExternoCommand(int Id) : IRequest;

public class ApagarValorExternoCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarValorExternoCommand>
{
    public async Task Handle(ApagarValorExternoCommand request, CancellationToken cancellationToken)
    {
        var v = await context.ValoresExternos.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, v);
        context.ValoresExternos.Remove(v);
        await context.SaveChangesAsync(cancellationToken);
    }
}
