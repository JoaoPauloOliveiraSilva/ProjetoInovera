namespace Innovera.Application.Iniciativas;

public record IniciativaResumoDto(
    int Id,
    string Codigo,
    TipoIniciativa Tipo,
    string Titulo,
    EstadoIniciativa Estado,
    string? Responsavel,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    string? Origem,
    string? PontoSituacao,
    int? IdeiaOrigemId,
    string? IdeiaOrigemCodigo,
    PilarEstrategico? Pilar = null);

public record CampoDto(string Nome, string Valor);

public record NotaPontoSituacaoDto(DateTimeOffset Data, string Texto, string? Autor);

public record IniciativaDetalheDto(
    IniciativaResumoDto Resumo,
    string? Descricao,
    Horizonte? Horizonte,
    IReadOnlyList<CampoDto> Detalhes,
    IReadOnlyList<NotaPontoSituacaoDto> Historico,
    IReadOnlyList<LinkDto> Links,
    bool TemCharter,
    CamposTipoDto Campos,
    bool PodeEditar);

/// <summary>Campos próprios de cada tipo de iniciativa (só os do tipo em causa interessam).</summary>
public record CamposTipoDto(
    TipoProjeto? TipoProjeto = null,
    AmbitoProjeto? Ambito = null,
    string? Resultado = null,
    string? Descritivo = null,
    Periodicidade? Periodicidade = null,
    string? NomeAluno = null,
    string? Curso = null,
    TipoAcaoInovacao? TipoAcao = null,
    int? NumeroParticipantes = null)
{
    public static CamposTipoDto De(Iniciativa iniciativa) => iniciativa switch
    {
        Projeto p => new CamposTipoDto(TipoProjeto: p.TipoProjeto, Ambito: p.Ambito),
        MelhoriaContinua m => new CamposTipoDto(Resultado: m.Resultado),
        Oportunidade o => new CamposTipoDto(Resultado: o.Resultado),
        Vigilancia v => new CamposTipoDto(Descritivo: v.Descritivo, Periodicidade: v.Periodicidade),
        Mestrado m => new CamposTipoDto(NomeAluno: m.NomeAluno, Curso: m.Curso),
        AcaoInovacao a => new CamposTipoDto(TipoAcao: a.TipoAcao, NumeroParticipantes: a.NumeroParticipantes),
        _ => new CamposTipoDto(),
    };
}

/// <summary>Link para um documento no OneDrive (RF15).</summary>
public record LinkDto(int Id, string Titulo, string Url);

internal static class Tipos
{
    /// <summary>A coluna "Discriminador" (TPH) guarda o nome da classe, igual ao nome do tipo.</summary>
    public static TipoIniciativa DoDiscriminador(string discriminador) => Enum.Parse<TipoIniciativa>(discriminador);
}

/// <summary>Portefólio de iniciativas (Mod.246): projetos, desafios, melhorias, oportunidades, vigilâncias, mestrados, ações.</summary>
[Authorize]
public record ListarIniciativasQuery(TipoIniciativa? Tipo, EstadoIniciativa? Estado, string? Texto, PilarEstrategico? Pilar = null, int? Ano = null)
    : IRequest<IReadOnlyList<IniciativaResumoDto>>;

public class ListarIniciativasQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarIniciativasQuery, IReadOnlyList<IniciativaResumoDto>>
{
    public async Task<IReadOnlyList<IniciativaResumoDto>> Handle(ListarIniciativasQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.Iniciativas.AsNoTracking();
        if (request.Tipo is not null)
        {
            var discriminador = request.Tipo.Value.ToString();
            consulta = consulta.Where(i => EF.Property<string>(i, "Discriminador") == discriminador);
        }

        if (request.Estado is not null)
        {
            consulta = consulta.Where(i => i.Estado == request.Estado);
        }

        if (request.Pilar is not null)
        {
            consulta = consulta.Where(i => i.PilarEstrategico == request.Pilar);
        }

        if (request.Ano is { } ano)
        {
            // Ativas nesse ano: começaram até ao fim do ano e não terminaram antes de ele começar.
            var inicioAno = new DateOnly(ano, 1, 1);
            var fimAno = new DateOnly(ano, 12, 31);
            consulta = consulta.Where(i => (i.DataInicio == null || i.DataInicio <= fimAno) && (i.DataFim == null || i.DataFim >= inicioAno)
                && (i.DataInicio != null || i.DataFim != null));
        }

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(i => i.Titulo.ToLower().Contains(texto) || i.Codigo.ToLower().Contains(texto)
                || (i.ResponsavelNome != null && i.ResponsavelNome.ToLower().Contains(texto))
                || (i.Responsavel != null && i.Responsavel.Nome.ToLower().Contains(texto)));
        }

        var linhas = await consulta
            .OrderByDescending(i => i.DataInicio)
            .ThenBy(i => i.Codigo)
            .Select(i => new
            {
                i.Id,
                i.Codigo,
                Discriminador = EF.Property<string>(i, "Discriminador"),
                i.Titulo,
                i.Estado,
                Responsavel = i.Responsavel != null ? i.Responsavel.Nome : i.ResponsavelNome,
                i.DataInicio,
                i.DataFim,
                i.Origem,
                i.PontoSituacao,
                i.IdeiaOrigemId,
                IdeiaCodigo = i.IdeiaOrigem != null ? i.IdeiaOrigem.Codigo : null,
                i.PilarEstrategico,
            })
            .ToListAsync(cancellationToken);

        return linhas
            .Select(l => new IniciativaResumoDto(l.Id, l.Codigo, Tipos.DoDiscriminador(l.Discriminador), l.Titulo, l.Estado, l.Responsavel,
                l.DataInicio, l.DataFim, l.Origem, l.PontoSituacao, l.IdeiaOrigemId, l.IdeiaCodigo, l.PilarEstrategico))
            .ToList();
    }
}

[Authorize]
public record ObterIniciativaQuery(int Id) : IRequest<IniciativaDetalheDto>;

public class ObterIniciativaQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores)
    : IRequestHandler<ObterIniciativaQuery, IniciativaDetalheDto>
{
    public async Task<IniciativaDetalheDto> Handle(ObterIniciativaQuery request, CancellationToken cancellationToken)
    {
        var i = await context.Iniciativas
            .AsNoTracking()
            .Include(x => x.Responsavel)
            .Include(x => x.IdeiaOrigem)
            .Include(x => x.NotasPontoSituacao).ThenInclude(n => n.Autor)
            .Include(x => x.Links)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, i);

        var resumo = new IniciativaResumoDto(i.Id, i.Codigo, i.Tipo, i.Titulo, i.Estado, i.Responsavel?.Nome ?? i.ResponsavelNome,
            i.DataInicio, i.DataFim, i.Origem, i.PontoSituacao, i.IdeiaOrigemId, i.IdeiaOrigem?.Codigo, i.PilarEstrategico);

        var historico = i.NotasPontoSituacao
            .OrderByDescending(n => n.Data)
            .Select(n => new NotaPontoSituacaoDto(n.Data, n.Texto, n.Autor?.Nome))
            .ToList();

        var links = i.Links.OrderBy(l => l.Titulo).Select(l => new LinkDto(l.Id, l.Titulo, l.Url)).ToList();

        return new IniciativaDetalheDto(resumo, i.Descricao, i.Horizonte, Detalhes(i), historico, links, i is IniciativaComCharter,
            CamposTipoDto.De(i),
            await utilizadores.TemPermissaoAsync(Permissoes.Iniciativas.Gerir, cancellationToken));
    }

    /// <summary>Campos próprios de cada tipo (como nas folhas do Mod.246).</summary>
    private static List<CampoDto> Detalhes(Iniciativa iniciativa)
    {
        var campos = new List<CampoDto>();
        void Juntar(string nome, string? valor)
        {
            if (!string.IsNullOrWhiteSpace(valor))
            {
                campos.Add(new CampoDto(nome, valor));
            }
        }

        switch (iniciativa)
        {
            case Projeto p:
                Juntar("Tipo de projeto", p.TipoProjeto switch
                {
                    TipoProjeto.Investigacao => "Investigação",
                    TipoProjeto.DesenvolvimentoInovacao => "Desenvolvimento & Inovação",
                    TipoProjeto.Inovacao => "Inovação",
                    TipoProjeto.InvestigacaoDesenvolvimento => "Investigação & Desenvolvimento",
                    TipoProjeto.InvestigacaoDesenvolvimentoInovacao => "ID+I",
                    _ => null,
                });
                Juntar("Âmbito", p.Ambito == AmbitoProjeto.Interno ? "Interno" : "Externo");
                break;
            case MelhoriaContinua m:
                Juntar("Resultado", m.Resultado);
                break;
            case Oportunidade o:
                Juntar("Resultado", o.Resultado);
                break;
            case Mestrado m:
                Juntar("Aluno", m.NomeAluno);
                Juntar("Curso", m.Curso);
                break;
            case Vigilancia v:
                Juntar("Descritivo", v.Descritivo);
                Juntar("Periodicidade", v.Periodicidade.ToString());
                break;
            case AcaoInovacao a:
                Juntar("Tipo de ação", a.TipoAcao == TipoAcaoInovacao.Criada ? "Criada pela equipa" : "Externa");
                Juntar("Participantes", a.NumeroParticipantes?.ToString());
                break;
        }

        return campos;
    }
}

/// <summary>Dados comuns editáveis de uma iniciativa.</summary>
public record DadosIniciativa
{
    public string Titulo { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public EstadoIniciativa Estado { get; init; } = EstadoIniciativa.Pendente;
    public string? Responsavel { get; init; }
    public DateOnly? DataInicio { get; init; }
    public DateOnly? DataFim { get; init; }
    public string? Origem { get; init; }
    public Horizonte? Horizonte { get; init; }

    public PilarEstrategico? Pilar { get; init; }

    /// <summary>Campos próprios do tipo (opcional: se vier vazio, não se mexe neles).</summary>
    public CamposTipoDto? Campos { get; init; }
}

public class DadosIniciativaValidator : AbstractValidator<DadosIniciativa>
{
    public DadosIniciativaValidator()
    {
        RuleFor(d => d.Titulo).NotEmpty().MaximumLength(300);
        RuleFor(d => d.Responsavel).MaximumLength(200);
        RuleFor(d => d.Origem).MaximumLength(200);
        RuleFor(d => d.Estado).IsInEnum();
        RuleFor(d => d.Pilar).IsInEnum().When(d => d.Pilar is not null);
        RuleFor(d => d.DataFim).GreaterThanOrEqualTo(d => d.DataInicio)
            .When(d => d.DataInicio is not null && d.DataFim is not null)
            .WithMessage("A data de fim não pode ser anterior à de início.");
        When(d => d.Campos is not null, () =>
        {
            RuleFor(d => d.Campos!.Resultado).MaximumLength(2000);
            RuleFor(d => d.Campos!.Descritivo).MaximumLength(4000);
            RuleFor(d => d.Campos!.NomeAluno).MaximumLength(200);
            RuleFor(d => d.Campos!.Curso).MaximumLength(200);
            RuleFor(d => d.Campos!.NumeroParticipantes).GreaterThanOrEqualTo(0).When(d => d.Campos!.NumeroParticipantes is not null);
            RuleFor(d => d.Campos!.TipoProjeto).IsInEnum().When(d => d.Campos!.TipoProjeto is not null);
            RuleFor(d => d.Campos!.Ambito).IsInEnum().When(d => d.Campos!.Ambito is not null);
            RuleFor(d => d.Campos!.Periodicidade).IsInEnum().When(d => d.Campos!.Periodicidade is not null);
            RuleFor(d => d.Campos!.TipoAcao).IsInEnum().When(d => d.Campos!.TipoAcao is not null);
        });
    }
}

internal static class AplicarDados
{
    public static void Em(Iniciativa iniciativa, DadosIniciativa dados)
    {
        iniciativa.Titulo = dados.Titulo.Trim();
        iniciativa.Descricao = string.IsNullOrWhiteSpace(dados.Descricao) ? null : dados.Descricao.Trim();
        iniciativa.Estado = dados.Estado;
        iniciativa.DataInicio = dados.DataInicio;
        iniciativa.DataFim = dados.DataFim;
        iniciativa.Origem = string.IsNullOrWhiteSpace(dados.Origem) ? null : dados.Origem.Trim();
        iniciativa.Horizonte = dados.Horizonte;
        iniciativa.PilarEstrategico = dados.Pilar;

        if (dados.Campos is { } c)
        {
            static string? Limpo(string? t) => string.IsNullOrWhiteSpace(t) ? null : t.Trim();
            switch (iniciativa)
            {
                case Projeto p:
                    p.TipoProjeto = c.TipoProjeto;
                    p.Ambito = c.Ambito ?? p.Ambito;
                    break;
                case MelhoriaContinua m:
                    m.Resultado = Limpo(c.Resultado);
                    break;
                case Oportunidade o:
                    o.Resultado = Limpo(c.Resultado);
                    break;
                case Vigilancia v:
                    v.Descritivo = Limpo(c.Descritivo);
                    v.Periodicidade = c.Periodicidade ?? v.Periodicidade;
                    break;
                case Mestrado m:
                    m.NomeAluno = Limpo(c.NomeAluno) ?? string.Empty;
                    m.Curso = Limpo(c.Curso);
                    break;
                case AcaoInovacao a:
                    a.TipoAcao = c.TipoAcao ?? a.TipoAcao;
                    a.NumeroParticipantes = c.NumeroParticipantes;
                    break;
            }
        }

        // Se o nome do responsável não mudou, mantém-se a ligação ao utilizador (ex.: autor da ideia de origem).
        var nome = string.IsNullOrWhiteSpace(dados.Responsavel) ? null : dados.Responsavel.Trim();
        if (nome != (iniciativa.Responsavel?.Nome ?? iniciativa.ResponsavelNome))
        {
            iniciativa.ResponsavelId = null;
            iniciativa.Responsavel = null;
            iniciativa.ResponsavelNome = nome;
        }
    }
}

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record CriarIniciativaCommand(TipoIniciativa Tipo, DadosIniciativa Dados) : IRequest<int>;

public class CriarIniciativaCommandValidator : AbstractValidator<CriarIniciativaCommand>
{
    public CriarIniciativaCommandValidator()
    {
        RuleFor(c => c.Tipo).IsInEnum();
        RuleFor(c => c.Dados).NotNull().SetValidator(new DadosIniciativaValidator());
    }
}

public class CriarIniciativaCommandHandler(IApplicationDbContext context) : IRequestHandler<CriarIniciativaCommand, int>
{
    public async Task<int> Handle(CriarIniciativaCommand request, CancellationToken cancellationToken)
    {
        var iniciativa = Iniciativa.Nova(request.Tipo);
        AplicarDados.Em(iniciativa, request.Dados);
        iniciativa.Codigo = await ProximoCodigoAsync(context, request.Tipo, cancellationToken);

        context.Iniciativas.Add(iniciativa);
        await context.SaveChangesAsync(cancellationToken);
        return iniciativa.Id;
    }

    /// <summary>Ex.: PRJ-025 (o seguinte ao maior número já usado com esse prefixo).</summary>
    internal static async Task<string> ProximoCodigoAsync(IApplicationDbContext context, TipoIniciativa tipo, CancellationToken cancellationToken)
    {
        var prefixo = Iniciativa.PrefixoCodigo(tipo) + "-";
        var codigos = await context.Iniciativas.Where(i => i.Codigo.StartsWith(prefixo)).Select(i => i.Codigo).ToListAsync(cancellationToken);
        var maior = codigos.Select(c => int.TryParse(c[prefixo.Length..], out var n) ? n : 0).DefaultIfEmpty(0).Max();
        return $"{prefixo}{maior + 1:000}";
    }
}

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record AtualizarIniciativaCommand(int Id, DadosIniciativa Dados) : IRequest;

public class AtualizarIniciativaCommandValidator : AbstractValidator<AtualizarIniciativaCommand>
{
    public AtualizarIniciativaCommandValidator()
    {
        RuleFor(c => c.Dados).NotNull().SetValidator(new DadosIniciativaValidator());
    }
}

public class AtualizarIniciativaCommandHandler(IApplicationDbContext context) : IRequestHandler<AtualizarIniciativaCommand>
{
    public async Task Handle(AtualizarIniciativaCommand request, CancellationToken cancellationToken)
    {
        var iniciativa = await context.Iniciativas.Include(i => i.Responsavel).FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, iniciativa);

        AplicarDados.Em(iniciativa, request.Dados);
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Novo ponto de situação: fica no histórico e passa a ser o atual.</summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record RegistarPontoSituacaoCommand(int Id, string Texto) : IRequest;

public class RegistarPontoSituacaoCommandValidator : AbstractValidator<RegistarPontoSituacaoCommand>
{
    public RegistarPontoSituacaoCommandValidator()
    {
        RuleFor(c => c.Texto).NotEmpty().MaximumLength(2000);
    }
}

public class RegistarPontoSituacaoCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<RegistarPontoSituacaoCommand>
{
    public async Task Handle(RegistarPontoSituacaoCommand request, CancellationToken cancellationToken)
    {
        var iniciativa = await context.Iniciativas.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, iniciativa);

        var eu = await utilizadores.AtualAsync(cancellationToken);
        iniciativa.RegistarPontoSituacao(request.Texto, eu.Id, relogio.GetUtcNow());
        await context.SaveChangesAsync(cancellationToken);
    }
}
