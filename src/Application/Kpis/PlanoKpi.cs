using System.Globalization;

namespace Innovera.Application.Kpis;

// Objetivo anual e plano de ação de cada KPI (bloco ISO do Mod.239): plano de ação inicial e acompanhamento
// (análise dos resultados, ação, responsável, prazo, concluída em, eficaz / não eficaz, avaliação no fim do ano).

/// <summary>Compara o valor com o objetivo escrito como no Mod.239 ("≥ 30", "> 1 000 000€", "≥ 80%").</summary>
public static class ObjetivoKpi
{
    public static bool? Atingido(string? objetivo, decimal? valor)
    {
        if (valor is null || string.IsNullOrWhiteSpace(objetivo))
        {
            return null;
        }

        var texto = new string(objetivo.Where(c => !char.IsWhiteSpace(c) && c is not ('€' or '%')).ToArray());
        var (operador, resto) = texto switch
        {
            _ when texto.StartsWith(">=", StringComparison.Ordinal) => ("≥", texto[2..]),
            _ when texto.StartsWith("<=", StringComparison.Ordinal) => ("≤", texto[2..]),
            _ when texto.StartsWith('≥') || texto.StartsWith('≤') || texto.StartsWith('>') || texto.StartsWith('<') || texto.StartsWith('=')
                => (texto[..1], texto[1..]),
            _ => ("≥", texto),
        };

        if (!TryNumero(resto, out var alvo))
        {
            return null;
        }

        var v = valor.Value;
        return operador switch
        {
            "≤" => v <= alvo,
            ">" => v > alvo,
            "<" => v < alvo,
            "=" => v == alvo,
            _ => v >= alvo,
        };
    }

    /// <summary>Lê "1.000.000", "1000000", "2,5" ou "2.5".</summary>
    private static bool TryNumero(string texto, out decimal numero)
    {
        var limpo = texto;
        if (limpo.Contains(','))
        {
            limpo = limpo.Replace(".", string.Empty, StringComparison.Ordinal).Replace(',', '.');
        }
        else if (limpo.Count(c => c == '.') > 1 || (limpo.Length > 4 && limpo[^4] == '.'))
        {
            limpo = limpo.Replace(".", string.Empty, StringComparison.Ordinal);
        }

        return decimal.TryParse(limpo, NumberStyles.Number, CultureInfo.InvariantCulture, out numero);
    }
}

public record AcaoKpiDto(int Id, TipoAcaoKpi Tipo, DateOnly DataCriacao, string Descricao, string? AnaliseResultados, string? Recursos,
    int? ResponsavelId, string? Responsavel, DateOnly? Prazo, DateOnly? ConcluidaEm, bool? Eficaz, string? AvaliacaoAnual);

public record AcaoKpiDados(TipoAcaoKpi Tipo, DateOnly? DataCriacao, string Descricao, string? AnaliseResultados, string? Recursos, int? ResponsavelId,
    DateOnly? Prazo, DateOnly? ConcluidaEm, bool? Eficaz, string? AvaliacaoAnual);

public record PlanoKpiDto(int Numero, string Nome, string Categoria, UnidadeKpi Unidade, int Ano, string? Objetivo, IReadOnlyList<AcaoKpiDto> Acoes,
    bool PodeEditar);

internal static class PlanosKpi
{
    public static DefinicaoKpi Definicao(int numero)
        => CatalogoKpi.Todos.FirstOrDefault(k => k.Numero == numero)
            ?? throw new RegraDeNegocioException($"O KPI {numero} não existe (os KPIs vão de 1 a {CatalogoKpi.Todos.Count}).");

    /// <summary>O registo anual do KPI (criado na primeira vez, tal como a definição do KPI).</summary>
    public static async Task<ValorKpi> ObterOuCriarAsync(IApplicationDbContext context, int numero, int ano, CancellationToken cancellationToken)
    {
        var def = Definicao(numero);
        var valor = await context.ValoresKpi
            .Include(v => v.Acoes)
            .FirstOrDefaultAsync(v => v.Kpi.Numero == numero && v.Ano == ano && v.Periodo == PeriodoKpi.Anual, cancellationToken);
        if (valor is not null)
        {
            return valor;
        }

        var kpi = await context.Kpis.FirstOrDefaultAsync(k => k.Numero == numero, cancellationToken);
        if (kpi is null)
        {
            kpi = new KpiDefinicao { Numero = def.Numero, Nome = def.Nome, Categoria = Categoria(def.Categoria) };
            context.Kpis.Add(kpi);
        }

        valor = new ValorKpi { Kpi = kpi, Ano = ano, Periodo = PeriodoKpi.Anual };
        context.ValoresKpi.Add(valor);
        return valor;
    }

    private static CategoriaKpi Categoria(string categoria) => categoria switch
    {
        "Ideias" => CategoriaKpi.Criatividade,
        "Desafios" => CategoriaKpi.Desafios,
        "Projetos" => CategoriaKpi.Projetos,
        "Melhoria contínua" => CategoriaKpi.MelhoriaContinua,
        "Mestrados" => CategoriaKpi.Mestrado,
        "Conhecimento" => CategoriaKpi.Conhecimento,
        "Parcerias" => CategoriaKpi.Colaboracao,
        "Propriedade intelectual" => CategoriaKpi.PropriedadeIntelectual,
        "Cultura" => CategoriaKpi.Cultura,
        "Estratégia" => CategoriaKpi.Estrategia,
        _ => CategoriaKpi.Orcamento,
    };
}

[Authorize]
public record ObterPlanoKpiQuery(int Numero, int Ano) : IRequest<PlanoKpiDto>;

public class ObterPlanoKpiQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores) : IRequestHandler<ObterPlanoKpiQuery, PlanoKpiDto>
{
    public async Task<PlanoKpiDto> Handle(ObterPlanoKpiQuery request, CancellationToken cancellationToken)
    {
        var def = PlanosKpi.Definicao(request.Numero);
        var valor = await context.ValoresKpi.AsNoTracking()
            .Include(v => v.Acoes).ThenInclude(a => a.Responsavel)
            .FirstOrDefaultAsync(v => v.Kpi.Numero == request.Numero && v.Ano == request.Ano && v.Periodo == PeriodoKpi.Anual, cancellationToken);

        var acoes = (valor?.Acoes ?? new List<AcaoKpi>())
            .OrderBy(a => a.Tipo).ThenBy(a => a.DataCriacao).ThenBy(a => a.Id)
            .Select(a => new AcaoKpiDto(a.Id, a.Tipo, a.DataCriacao, a.Descricao, a.AnaliseResultados, a.Recursos, a.ResponsavelId,
                a.Responsavel?.Nome, a.Prazo, a.ConcluidaEm, a.Eficaz, a.AvaliacaoAnual))
            .ToList();

        return new PlanoKpiDto(def.Numero, def.Nome, def.Categoria, def.Unidade, request.Ano, valor?.Objetivo, acoes,
            await utilizadores.TemPermissaoAsync(Permissoes.Kpis.Objetivos, cancellationToken));
    }
}

[Authorize(Permissao = Permissoes.Kpis.Objetivos)]
public record GuardarObjetivoKpiCommand(int Numero, int Ano, string? Objetivo) : IRequest;

public class GuardarObjetivoKpiCommandValidator : AbstractValidator<GuardarObjetivoKpiCommand>
{
    public GuardarObjetivoKpiCommandValidator()
    {
        RuleFor(c => c.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Objetivo).MaximumLength(50);
    }
}

public class GuardarObjetivoKpiCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarObjetivoKpiCommand>
{
    public async Task Handle(GuardarObjetivoKpiCommand request, CancellationToken cancellationToken)
    {
        var valor = await PlanosKpi.ObterOuCriarAsync(context, request.Numero, request.Ano, cancellationToken);
        valor.Objetivo = string.IsNullOrWhiteSpace(request.Objetivo) ? null : request.Objetivo.Trim();
        await context.SaveChangesAsync(cancellationToken);
    }
}

[Authorize(Permissao = Permissoes.Kpis.Objetivos)]
public record GuardarAcaoKpiCommand(int Numero, int Ano, int? Id, AcaoKpiDados Dados) : IRequest<int>;

public class GuardarAcaoKpiCommandValidator : AbstractValidator<GuardarAcaoKpiCommand>
{
    public GuardarAcaoKpiCommandValidator()
    {
        RuleFor(c => c.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Dados.Tipo).IsInEnum();
        RuleFor(c => c.Dados.Descricao).NotEmpty().WithMessage("Descreva a ação.").MaximumLength(2000);
        RuleFor(c => c.Dados.AnaliseResultados).MaximumLength(4000);
        RuleFor(c => c.Dados.Recursos).MaximumLength(1000);
        RuleFor(c => c.Dados.AvaliacaoAnual).MaximumLength(4000);
    }
}

public class GuardarAcaoKpiCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<GuardarAcaoKpiCommand, int>
{
    public async Task<int> Handle(GuardarAcaoKpiCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dados;
        if (d.ResponsavelId is { } responsavel && !await context.Utilizadores.AnyAsync(u => u.Id == responsavel, cancellationToken))
        {
            throw new RegraDeNegocioException("O responsável escolhido já não existe.");
        }

        var valor = await PlanosKpi.ObterOuCriarAsync(context, request.Numero, request.Ano, cancellationToken);
        AcaoKpi acao;
        if (request.Id is null)
        {
            acao = new AcaoKpi { ValorKpi = valor };
            valor.Acoes.Add(acao);
        }
        else
        {
            var existente = valor.Acoes.FirstOrDefault(a => a.Id == request.Id.Value);
            Guard.Against.NotFound(request.Id.Value, existente);
            acao = existente;
        }

        acao.Tipo = d.Tipo;
        acao.DataCriacao = d.DataCriacao ?? (acao.DataCriacao == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : acao.DataCriacao);
        acao.Descricao = d.Descricao.Trim();
        acao.AnaliseResultados = Limpar(d.AnaliseResultados);
        acao.Recursos = Limpar(d.Recursos);
        acao.ResponsavelId = d.ResponsavelId;
        acao.Prazo = d.Prazo;
        acao.ConcluidaEm = d.ConcluidaEm;
        acao.Eficaz = d.Eficaz;
        acao.AvaliacaoAnual = Limpar(d.AvaliacaoAnual);
        await context.SaveChangesAsync(cancellationToken);
        return acao.Id;
    }

    private static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
}

[Authorize(Permissao = Permissoes.Kpis.Objetivos)]
public record ApagarAcaoKpiCommand(int Numero, int Ano, int Id) : IRequest;

public class ApagarAcaoKpiCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarAcaoKpiCommand>
{
    public async Task Handle(ApagarAcaoKpiCommand request, CancellationToken cancellationToken)
    {
        var valor = await PlanosKpi.ObterOuCriarAsync(context, request.Numero, request.Ano, cancellationToken);
        var acao = valor.Acoes.FirstOrDefault(a => a.Id == request.Id);
        Guard.Against.NotFound(request.Id, acao);
        valor.Acoes.Remove(acao);
        await context.SaveChangesAsync(cancellationToken);
    }
}
