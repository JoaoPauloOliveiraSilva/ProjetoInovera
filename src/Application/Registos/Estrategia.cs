using Innovera.Application.Kpis;

namespace Innovera.Application.Registos;

// Estratégia de inovação anual (Mod.52): por área de atuação, a abordagem, o plano de ação, os KPIs associados,
// os recursos (duração × n.º de RH × horas/mês = dedicação total) e o horizonte H1/H2/H3.

public record KpiAssociadoDto(int Numero, string Nome);

public record LinhaEstrategiaDto(int Id, int Ano, string AreaAtuacao, string? Abordagem, string? PlanoAcao, IReadOnlyList<KpiAssociadoDto> Kpis,
    int? DuracaoMeses, int? NumeroRecursosHumanos, decimal? HorasMesPorRecurso, decimal? DedicacaoTotalHoras, string? RecursosExtra,
    Horizonte? Horizonte, int? IniciativaId, string? IniciativaCodigo, string? IniciativaTitulo);

public record LinhaEstrategiaDados(int Ano, string AreaAtuacao, string? Abordagem, string? PlanoAcao, IReadOnlyList<int>? Kpis, int? DuracaoMeses,
    int? NumeroRecursosHumanos, decimal? HorasMesPorRecurso, string? RecursosExtra, Horizonte? Horizonte, int? IniciativaId);

[Authorize]
public record ListarEstrategiaQuery(int Ano) : IRequest<IReadOnlyList<LinhaEstrategiaDto>>;

public class ListarEstrategiaQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarEstrategiaQuery, IReadOnlyList<LinhaEstrategiaDto>>
{
    public async Task<IReadOnlyList<LinhaEstrategiaDto>> Handle(ListarEstrategiaQuery request, CancellationToken cancellationToken)
    {
        var linhas = await context.LinhasEstrategia.AsNoTracking()
            .Include(l => l.Iniciativa)
            .Where(l => l.Ano == request.Ano)
            .OrderBy(l => l.AreaAtuacao).ThenBy(l => l.Id)
            .ToListAsync(cancellationToken);

        var nomes = CatalogoKpi.Todos.ToDictionary(k => k.Numero, k => k.Nome);
        return linhas.Select(l => new LinhaEstrategiaDto(l.Id, l.Ano, l.AreaAtuacao, l.Abordagem, l.PlanoAcao,
                l.Kpis.Order().Select(n => new KpiAssociadoDto(n, nomes.GetValueOrDefault(n, $"KPI {n}"))).ToList(),
                l.DuracaoMeses, l.NumeroRecursosHumanos, l.HorasMesPorRecurso, l.DedicacaoTotalHoras, l.RecursosExtra, l.Horizonte,
                l.IniciativaId, l.Iniciativa?.Codigo, l.Iniciativa?.Titulo))
            .ToList();
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarLinhaEstrategiaCommand(int? Id, LinhaEstrategiaDados Dados) : IRequest<int>;

public class GuardarLinhaEstrategiaCommandValidator : AbstractValidator<GuardarLinhaEstrategiaCommand>
{
    public GuardarLinhaEstrategiaCommandValidator()
    {
        RuleFor(c => c.Dados.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Dados.AreaAtuacao).NotEmpty().WithMessage("Indique a área de atuação.").MaximumLength(200);
        RuleFor(c => c.Dados.Abordagem).MaximumLength(500);
        RuleFor(c => c.Dados.PlanoAcao).MaximumLength(2000);
        RuleFor(c => c.Dados.RecursosExtra).MaximumLength(500);
        RuleForEach(c => c.Dados.Kpis).InclusiveBetween(1, CatalogoKpi.Todos.Count).WithMessage("Os KPIs vão de 1 a 41 (Mod.239).");
        RuleFor(c => c.Dados.DuracaoMeses).InclusiveBetween(0, 120).When(c => c.Dados.DuracaoMeses is not null);
        RuleFor(c => c.Dados.NumeroRecursosHumanos).InclusiveBetween(0, 500).When(c => c.Dados.NumeroRecursosHumanos is not null);
        RuleFor(c => c.Dados.HorasMesPorRecurso).InclusiveBetween(0, 744).When(c => c.Dados.HorasMesPorRecurso is not null);
        RuleFor(c => c.Dados.Horizonte).IsInEnum().When(c => c.Dados.Horizonte is not null);
    }
}

public class GuardarLinhaEstrategiaCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarLinhaEstrategiaCommand, int>
{
    public async Task<int> Handle(GuardarLinhaEstrategiaCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dados;
        if (d.IniciativaId is { } iniciativaId && !await context.Iniciativas.AnyAsync(i => i.Id == iniciativaId, cancellationToken))
        {
            throw new RegraDeNegocioException("A iniciativa escolhida já não existe.");
        }

        var l = await Registo.ObterOuNovoAsync(context.LinhasEstrategia, request.Id, cancellationToken);
        l.Ano = d.Ano;
        l.AreaAtuacao = d.AreaAtuacao.Trim();
        l.Abordagem = Registo.Limpar(d.Abordagem);
        l.PlanoAcao = Registo.Limpar(d.PlanoAcao);
        l.Kpis = (d.Kpis ?? Array.Empty<int>()).Distinct().Order().ToList();
        l.DuracaoMeses = d.DuracaoMeses;
        l.NumeroRecursosHumanos = d.NumeroRecursosHumanos;
        l.HorasMesPorRecurso = d.HorasMesPorRecurso;
        l.RecursosExtra = Registo.Limpar(d.RecursosExtra);
        l.Horizonte = d.Horizonte;
        l.IniciativaId = d.IniciativaId;
        await context.SaveChangesAsync(cancellationToken);
        return l.Id;
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record ApagarLinhaEstrategiaCommand(int Id) : IRequest;

public class ApagarLinhaEstrategiaCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarLinhaEstrategiaCommand>
{
    public async Task Handle(ApagarLinhaEstrategiaCommand request, CancellationToken cancellationToken)
    {
        var l = await context.LinhasEstrategia.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, l);
        context.LinhasEstrategia.Remove(l);
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Começa a estratégia de um ano a partir da do ano anterior (o Mod.52 tem uma folha por ano). Devolve quantas linhas copiou.</summary>
[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record CopiarEstrategiaCommand(int DeAno, int ParaAno) : IRequest<int>;

public class CopiarEstrategiaCommandHandler(IApplicationDbContext context) : IRequestHandler<CopiarEstrategiaCommand, int>
{
    public async Task<int> Handle(CopiarEstrategiaCommand request, CancellationToken cancellationToken)
    {
        if (await context.LinhasEstrategia.AnyAsync(l => l.Ano == request.ParaAno, cancellationToken))
        {
            throw new RegraDeNegocioException($"A estratégia de {request.ParaAno} já tem linhas: só se copia para um ano vazio.");
        }

        var origem = await context.LinhasEstrategia.AsNoTracking().Where(l => l.Ano == request.DeAno).ToListAsync(cancellationToken);
        foreach (var l in origem)
        {
            context.LinhasEstrategia.Add(new LinhaEstrategia
            {
                Ano = request.ParaAno, AreaAtuacao = l.AreaAtuacao, Abordagem = l.Abordagem, PlanoAcao = l.PlanoAcao, Kpis = [.. l.Kpis],
                DuracaoMeses = l.DuracaoMeses, NumeroRecursosHumanos = l.NumeroRecursosHumanos, HorasMesPorRecurso = l.HorasMesPorRecurso,
                RecursosExtra = l.RecursosExtra, Horizonte = l.Horizonte, IniciativaId = l.IniciativaId,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        return origem.Count;
    }
}
