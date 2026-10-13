namespace Innovera.Application.Registos;

// Registos manuais da equipa de Inovação (RF14): parcerias (Mod.235), ativos intangíveis (Mod.236),
// conhecimento tácito (Mod.238) e ferramentas e métodos (Mod.241). Todos consultam; quem tem
// "registos.gerir" cria, altera e apaga. O conhecimento tácito confidencial só aparece a quem gere.

public enum TipoRegisto
{
    Parceiro,
    Acordo,
    Ativo,
    ConhecimentoTacito,
    Ferramenta
}

internal static class Registo
{
    /// <summary>A entidade a alterar (ou uma nova, já acrescentada ao contexto).</summary>
    public static async Task<T> ObterOuNovoAsync<T>(DbSet<T> conjunto, int? id, CancellationToken cancellationToken)
        where T : BaseEntity, new()
    {
        if (id is null)
        {
            var novo = new T();
            conjunto.Add(novo);
            return novo;
        }

        var existente = await conjunto.FirstOrDefaultAsync(x => x.Id == id.Value, cancellationToken);
        Guard.Against.NotFound(id.Value, existente);
        return existente;
    }

    public static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public static List<AtividadeGestaoDto> Atividades(IEnumerable<AtividadeGestao> atividades)
        => atividades.OrderBy(a => a.Data == null).ThenBy(a => a.Data)
            .Select(a => new AtividadeGestaoDto(a.Descricao, a.Responsavel, a.Data, a.Concluida)).ToList();

    /// <summary>Substitui as atividades (Atividades · Quem · Quando). Null = não mexer.</summary>
    public static void DefinirAtividades(ICollection<AtividadeGestao> atuais, IReadOnlyList<AtividadeGestaoDto>? novas)
    {
        if (novas is null)
        {
            return;
        }

        atuais.Clear();
        foreach (var a in novas)
        {
            atuais.Add(new AtividadeGestao { Descricao = a.Descricao.Trim(), Responsavel = Limpar(a.Responsavel), Data = a.Data, Concluida = a.Concluida });
        }
    }
}

/// <summary>Uma linha do modelo de gestão/partilha (Atividades · Quem · Quando) dos Mod.236, 237 e 238.</summary>
public record AtividadeGestaoDto(string Descricao, string? Responsavel, DateOnly? Data, bool Concluida);

public class AtividadeGestaoValidator : AbstractValidator<AtividadeGestaoDto>
{
    public AtividadeGestaoValidator()
    {
        RuleFor(a => a.Descricao).NotEmpty().WithMessage("Cada atividade precisa de uma descrição.").MaximumLength(500);
        RuleFor(a => a.Responsavel).MaximumLength(200);
    }
}

/// <summary>Iniciativa ligada a um parceiro.</summary>
public record IniciativaDoParceiroDto(int Id, string Codigo, string Titulo);

// ---------- Parceiros e acordos de parceria (Mod.235) ----------

public record ParceiroDto(int Id, string Nome, string? Nif, TipoParceiro Tipo, string? Descricao, int Acordos, int AcordosEmCurso,
    IReadOnlyList<IniciativaDoParceiroDto> Iniciativas);

public record ParceiroDados(string Nome, string? Nif, TipoParceiro Tipo, string? Descricao);

public record AcordoDto(int Id, int ParceiroId, string Parceiro, TipoParceiro TipoParceiro, string Descricao, bool ProtocoloEstabelecido,
    TipoProtocolo? TipoProtocolo, DateOnly? DataInicio, DateOnly? DataFim, bool IncluiGestaoPropriedadeIntelectual, EstadoAcordo Estado,
    string? Observacoes, string? LinkDocumento);

public record AcordoDados(int ParceiroId, string Descricao, bool ProtocoloEstabelecido, TipoProtocolo? TipoProtocolo, DateOnly? DataInicio,
    DateOnly? DataFim, bool IncluiGestaoPropriedadeIntelectual, EstadoAcordo Estado, string? Observacoes, string? LinkDocumento);

[Authorize]
public record ListarParceirosQuery : IRequest<IReadOnlyList<ParceiroDto>>;

public class ListarParceirosQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarParceirosQuery, IReadOnlyList<ParceiroDto>>
{
    public async Task<IReadOnlyList<ParceiroDto>> Handle(ListarParceirosQuery request, CancellationToken cancellationToken)
        => await context.Parceiros.AsNoTracking()
            .OrderBy(p => p.Nome)
            .Select(p => new ParceiroDto(p.Id, p.Nome, p.Nif, p.Tipo, p.Descricao, p.Acordos.Count,
                p.Acordos.Count(a => a.Estado == EstadoAcordo.EmCurso),
                p.Iniciativas.OrderBy(i => i.Codigo).Select(i => new IniciativaDoParceiroDto(i.Id, i.Codigo, i.Titulo)).ToList()))
            .ToListAsync(cancellationToken);
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarParceiroCommand(int? Id, ParceiroDados Dados) : IRequest<int>;

public class GuardarParceiroCommandValidator : AbstractValidator<GuardarParceiroCommand>
{
    public GuardarParceiroCommandValidator()
    {
        RuleFor(c => c.Dados.Nome).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Dados.Nif).MaximumLength(20);
        RuleFor(c => c.Dados.Tipo).IsInEnum();
    }
}

public class GuardarParceiroCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarParceiroCommand, int>
{
    public async Task<int> Handle(GuardarParceiroCommand request, CancellationToken cancellationToken)
    {
        var p = await Registo.ObterOuNovoAsync(context.Parceiros, request.Id, cancellationToken);
        var d = request.Dados;
        p.Nome = d.Nome.Trim();
        p.Nif = Registo.Limpar(d.Nif);
        p.Tipo = d.Tipo;
        p.Descricao = Registo.Limpar(d.Descricao);
        await context.SaveChangesAsync(cancellationToken);
        return p.Id;
    }
}

[Authorize]
public record ListarAcordosQuery(string? Texto) : IRequest<IReadOnlyList<AcordoDto>>;

public class ListarAcordosQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarAcordosQuery, IReadOnlyList<AcordoDto>>
{
    public async Task<IReadOnlyList<AcordoDto>> Handle(ListarAcordosQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.AcordosParceria.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(a => a.Descricao.ToLower().Contains(texto) || a.Parceiro.Nome.ToLower().Contains(texto));
        }

        return await consulta
            .OrderBy(a => a.Estado).ThenByDescending(a => a.DataInicio).ThenBy(a => a.Parceiro.Nome)
            .Select(a => new AcordoDto(a.Id, a.ParceiroId, a.Parceiro.Nome, a.Parceiro.Tipo, a.Descricao, a.ProtocoloEstabelecido, a.TipoProtocolo,
                a.DataInicio, a.DataFim, a.IncluiGestaoPropriedadeIntelectual, a.Estado, a.Observacoes, a.LinkDocumento))
            .ToListAsync(cancellationToken);
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarAcordoCommand(int? Id, AcordoDados Dados) : IRequest<int>;

public class GuardarAcordoCommandValidator : AbstractValidator<GuardarAcordoCommand>
{
    public GuardarAcordoCommandValidator()
    {
        RuleFor(c => c.Dados.ParceiroId).GreaterThan(0).WithMessage("Escolha o parceiro.");
        RuleFor(c => c.Dados.Descricao).NotEmpty().MaximumLength(2000);
        RuleFor(c => c.Dados.Estado).IsInEnum();
        RuleFor(c => c.Dados.DataFim).GreaterThanOrEqualTo(c => c.Dados.DataInicio)
            .When(c => c.Dados.DataInicio is not null && c.Dados.DataFim is not null)
            .WithMessage("A data de fim não pode ser anterior à de início.");
        RuleFor(c => c.Dados.LinkDocumento).MaximumLength(2000).Must(ValidacaoLinks.EValido)
            .When(c => !string.IsNullOrWhiteSpace(c.Dados.LinkDocumento)).WithMessage("O link tem de ser um endereço https://… (ex.: OneDrive).");
    }
}

public class GuardarAcordoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarAcordoCommand, int>
{
    public async Task<int> Handle(GuardarAcordoCommand request, CancellationToken cancellationToken)
    {
        var d = request.Dados;
        if (!await context.Parceiros.AnyAsync(p => p.Id == d.ParceiroId, cancellationToken))
        {
            throw new RegraDeNegocioException("O parceiro escolhido já não existe.");
        }

        var a = await Registo.ObterOuNovoAsync(context.AcordosParceria, request.Id, cancellationToken);
        a.ParceiroId = d.ParceiroId;
        a.Descricao = d.Descricao.Trim();
        a.ProtocoloEstabelecido = d.ProtocoloEstabelecido;
        a.TipoProtocolo = d.TipoProtocolo;
        a.DataInicio = d.DataInicio;
        a.DataFim = d.DataFim;
        a.IncluiGestaoPropriedadeIntelectual = d.IncluiGestaoPropriedadeIntelectual;
        a.Estado = d.Estado;
        a.Observacoes = Registo.Limpar(d.Observacoes);
        a.LinkDocumento = Registo.Limpar(d.LinkDocumento);
        await context.SaveChangesAsync(cancellationToken);
        return a.Id;
    }
}

// ---------- Ativos intangíveis (Mod.236) ----------

public record AtivoDto(int Id, DateOnly Data, TipoAtivoIntangivel Tipo, string EntidadeRegisto, string Designacao, string? Descricao,
    string? NumeroPedido, string? Autores, DateOnly? DataRegisto, DateOnly? DataValidade, string? Ambito, string? Situacao, decimal? Custo,
    string? PerspetivaValor, string? NumeroProcesso, string? Observacoes, int? DiasParaValidade, IReadOnlyList<AtividadeGestaoDto> Atividades);

public record AtivoDados(DateOnly? Data, TipoAtivoIntangivel Tipo, string EntidadeRegisto, string Designacao, string? Descricao,
    string? NumeroPedido, string? Autores, DateOnly? DataRegisto, DateOnly? DataValidade, string? Ambito, string? Situacao, decimal? Custo,
    string? PerspetivaValor, string? NumeroProcesso, string? Observacoes, IReadOnlyList<AtividadeGestaoDto>? Atividades = null);

[Authorize]
public record ListarAtivosQuery : IRequest<IReadOnlyList<AtivoDto>>;

public class ListarAtivosQueryHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<ListarAtivosQuery, IReadOnlyList<AtivoDto>>
{
    public async Task<IReadOnlyList<AtivoDto>> Handle(ListarAtivosQuery request, CancellationToken cancellationToken)
    {
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);
        var ativos = await context.AtivosIntangiveis.AsNoTracking()
            .OrderBy(a => a.DataValidade == null).ThenBy(a => a.DataValidade).ThenBy(a => a.Designacao)
            .ToListAsync(cancellationToken);

        return ativos.Select(a => new AtivoDto(a.Id, a.Data, a.Tipo, a.EntidadeRegisto, a.Designacao, a.Descricao, a.NumeroPedido, a.Autores,
                a.DataRegisto, a.DataValidade, a.Ambito, a.Situacao, a.Custo, a.PerspetivaValor, a.NumeroProcesso, a.Observacoes,
                a.DataValidade is { } v ? v.DayNumber - hoje.DayNumber : null, Registo.Atividades(a.Atividades)))
            .ToList();
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarAtivoCommand(int? Id, AtivoDados Dados) : IRequest<int>;

public class GuardarAtivoCommandValidator : AbstractValidator<GuardarAtivoCommand>
{
    public GuardarAtivoCommandValidator()
    {
        RuleFor(c => c.Dados.Designacao).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Dados.EntidadeRegisto).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Dados.Tipo).IsInEnum();
        RuleFor(c => c.Dados.Custo).GreaterThanOrEqualTo(0).When(c => c.Dados.Custo is not null);
        RuleFor(c => c.Dados.DataValidade).GreaterThanOrEqualTo(c => c.Dados.DataRegisto)
            .When(c => c.Dados.DataRegisto is not null && c.Dados.DataValidade is not null)
            .WithMessage("A validade não pode ser anterior à data de registo.");
        RuleForEach(c => c.Dados.Atividades).SetValidator(new AtividadeGestaoValidator());
    }
}

public class GuardarAtivoCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<GuardarAtivoCommand, int>
{
    public async Task<int> Handle(GuardarAtivoCommand request, CancellationToken cancellationToken)
    {
        var a = await Registo.ObterOuNovoAsync(context.AtivosIntangiveis, request.Id, cancellationToken);
        var d = request.Dados;
        a.Data = d.Data ?? (a.Data == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : a.Data);
        a.Tipo = d.Tipo;
        a.EntidadeRegisto = d.EntidadeRegisto.Trim();
        a.Designacao = d.Designacao.Trim();
        a.Descricao = Registo.Limpar(d.Descricao);
        a.NumeroPedido = Registo.Limpar(d.NumeroPedido);
        a.Autores = Registo.Limpar(d.Autores);
        a.DataRegisto = d.DataRegisto;
        a.DataValidade = d.DataValidade;
        a.Ambito = Registo.Limpar(d.Ambito);
        a.Situacao = Registo.Limpar(d.Situacao);
        a.Custo = d.Custo;
        a.PerspetivaValor = Registo.Limpar(d.PerspetivaValor);
        a.NumeroProcesso = Registo.Limpar(d.NumeroProcesso);
        a.Observacoes = Registo.Limpar(d.Observacoes);
        Registo.DefinirAtividades(a.Atividades, d.Atividades);
        await context.SaveChangesAsync(cancellationToken);
        return a.Id;
    }
}

// ---------- Conhecimento tácito (Mod.238) ----------

public record ConhecimentoTacitoDto(int Id, DateOnly Data, string? Empresa, string Detentor, string Descricao, NivelConfidencialidade Confidencialidade,
    IReadOnlyList<AtividadeGestaoDto> Atividades);

public record ConhecimentoTacitoDados(DateOnly? Data, string? Empresa, string Detentor, string Descricao, NivelConfidencialidade Confidencialidade,
    IReadOnlyList<AtividadeGestaoDto>? Atividades = null);

[Authorize]
public record ListarConhecimentoTacitoQuery : IRequest<IReadOnlyList<ConhecimentoTacitoDto>>;

public class ListarConhecimentoTacitoQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    : IRequestHandler<ListarConhecimentoTacitoQuery, IReadOnlyList<ConhecimentoTacitoDto>>
{
    public async Task<IReadOnlyList<ConhecimentoTacitoDto>> Handle(ListarConhecimentoTacitoQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.ConhecimentosTacitos.AsNoTracking();
        if (!await utilizadores.TemPermissaoAsync(Permissoes.Registos.Gerir, cancellationToken))
        {
            consulta = consulta.Where(c => c.Confidencialidade != NivelConfidencialidade.Elevado);
        }

        var lista = await consulta
            .OrderByDescending(c => c.Data).ThenBy(c => c.Detentor)
            .ToListAsync(cancellationToken);
        return lista.Select(c => new ConhecimentoTacitoDto(c.Id, c.Data, c.Empresa, c.Detentor, c.Descricao, c.Confidencialidade,
                Registo.Atividades(c.Atividades)))
            .ToList();
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarConhecimentoTacitoCommand(int? Id, ConhecimentoTacitoDados Dados) : IRequest<int>;

public class GuardarConhecimentoTacitoCommandValidator : AbstractValidator<GuardarConhecimentoTacitoCommand>
{
    public GuardarConhecimentoTacitoCommandValidator()
    {
        RuleFor(c => c.Dados.Detentor).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Dados.Descricao).NotEmpty().MaximumLength(4000);
        RuleFor(c => c.Dados.Empresa).MaximumLength(200);
        RuleFor(c => c.Dados.Confidencialidade).IsInEnum();
        RuleForEach(c => c.Dados.Atividades).SetValidator(new AtividadeGestaoValidator());
    }
}

public class GuardarConhecimentoTacitoCommandHandler(IApplicationDbContext context, TimeProvider relogio)
    : IRequestHandler<GuardarConhecimentoTacitoCommand, int>
{
    public async Task<int> Handle(GuardarConhecimentoTacitoCommand request, CancellationToken cancellationToken)
    {
        var c = await Registo.ObterOuNovoAsync(context.ConhecimentosTacitos, request.Id, cancellationToken);
        var d = request.Dados;
        c.Data = d.Data ?? (c.Data == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : c.Data);
        c.Empresa = Registo.Limpar(d.Empresa);
        c.Detentor = d.Detentor.Trim();
        c.Descricao = d.Descricao.Trim();
        c.Confidencialidade = d.Confidencialidade;
        Registo.DefinirAtividades(c.Atividades, d.Atividades);
        await context.SaveChangesAsync(cancellationToken);
        return c.Id;
    }
}

// ---------- Ferramentas e métodos (Mod.241) ----------

public record FerramentaDto(int Id, DateOnly Data, string Nome, string? SumarioExecutivo, string? Potencial, decimal? CustoAquisicao,
    string? Divulgacao, IReadOnlyList<AnaliseAnualDto> Analises);

public record FerramentaDados(DateOnly? Data, string Nome, string? SumarioExecutivo, string? Potencial, decimal? CustoAquisicao, string? Divulgacao,
    IReadOnlyList<AnaliseAnualDto>? Analises = null);

/// <summary>Coluna "Análise &lt;ano&gt;" do Mod.241: como correu a utilização da ferramenta nesse ano.</summary>
public record AnaliseAnualDto(int Ano, string Analise);

[Authorize]
public record ListarFerramentasQuery : IRequest<IReadOnlyList<FerramentaDto>>;

public class ListarFerramentasQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarFerramentasQuery, IReadOnlyList<FerramentaDto>>
{
    public async Task<IReadOnlyList<FerramentaDto>> Handle(ListarFerramentasQuery request, CancellationToken cancellationToken)
    {
        var lista = await context.FerramentasMetodos.AsNoTracking()
            .OrderByDescending(f => f.Data).ThenBy(f => f.Nome)
            .ToListAsync(cancellationToken);
        return lista.Select(f => new FerramentaDto(f.Id, f.Data, f.Nome, f.SumarioExecutivo, f.Potencial, f.CustoAquisicao, f.Divulgacao,
                f.Analises.OrderByDescending(a => a.Ano).Select(a => new AnaliseAnualDto(a.Ano, a.Analise)).ToList()))
            .ToList();
    }
}

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record GuardarFerramentaCommand(int? Id, FerramentaDados Dados) : IRequest<int>;

public class GuardarFerramentaCommandValidator : AbstractValidator<GuardarFerramentaCommand>
{
    public GuardarFerramentaCommandValidator()
    {
        RuleFor(c => c.Dados.Nome).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Dados.CustoAquisicao).GreaterThanOrEqualTo(0).When(c => c.Dados.CustoAquisicao is not null);
        RuleForEach(c => c.Dados.Analises).ChildRules(a =>
        {
            a.RuleFor(x => x.Ano).InclusiveBetween(2000, 2100);
            a.RuleFor(x => x.Analise).NotEmpty().WithMessage("Escreva a análise do ano.").MaximumLength(4000);
        });
        RuleFor(c => c.Dados.Analises).Must(l => l!.Select(a => a.Ano).Distinct().Count() == l!.Count)
            .When(c => c.Dados.Analises is not null).WithMessage("Só pode haver uma análise por ano.");
    }
}

public class GuardarFerramentaCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<GuardarFerramentaCommand, int>
{
    public async Task<int> Handle(GuardarFerramentaCommand request, CancellationToken cancellationToken)
    {
        var f = await Registo.ObterOuNovoAsync(context.FerramentasMetodos, request.Id, cancellationToken);
        var d = request.Dados;
        f.Data = d.Data ?? (f.Data == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : f.Data);
        f.Nome = d.Nome.Trim();
        f.SumarioExecutivo = Registo.Limpar(d.SumarioExecutivo);
        f.Potencial = Registo.Limpar(d.Potencial);
        f.CustoAquisicao = d.CustoAquisicao;
        f.Divulgacao = Registo.Limpar(d.Divulgacao);
        if (d.Analises is not null)
        {
            f.Analises.Clear();
            foreach (var a in d.Analises)
            {
                f.Analises.Add(new AnaliseAnual { Ano = a.Ano, Analise = a.Analise.Trim() });
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        return f.Id;
    }
}

// ---------- Apagar qualquer registo ----------

[Authorize(Permissao = Permissoes.Registos.Gerir)]
public record ApagarRegistoCommand(TipoRegisto Tipo, int Id) : IRequest;

public class ApagarRegistoCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarRegistoCommand>
{
    public async Task Handle(ApagarRegistoCommand request, CancellationToken cancellationToken)
    {
        switch (request.Tipo)
        {
            case TipoRegisto.Parceiro:
                context.Parceiros.Remove(await Obter(context.Parceiros, request.Id, cancellationToken));
                break;
            case TipoRegisto.Acordo:
                context.AcordosParceria.Remove(await Obter(context.AcordosParceria, request.Id, cancellationToken));
                break;
            case TipoRegisto.Ativo:
                context.AtivosIntangiveis.Remove(await Obter(context.AtivosIntangiveis, request.Id, cancellationToken));
                break;
            case TipoRegisto.ConhecimentoTacito:
                context.ConhecimentosTacitos.Remove(await Obter(context.ConhecimentosTacitos, request.Id, cancellationToken));
                break;
            case TipoRegisto.Ferramenta:
                context.FerramentasMetodos.Remove(await Obter(context.FerramentasMetodos, request.Id, cancellationToken));
                break;
            default:
                throw new RegraDeNegocioException("Tipo de registo desconhecido.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<T> Obter<T>(DbSet<T> conjunto, int id, CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var item = await conjunto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        Guard.Against.NotFound(id, item);
        return item;
    }
}
