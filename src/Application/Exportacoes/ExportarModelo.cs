using Innovera.Application.Charters;
using Innovera.Application.Ideias.Queries;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Application.Registos;

namespace Innovera.Application.Exportacoes;

// Outputs em Excel dos modelos da dstelecom (decisão de 13/10: "exportar outputs em excel").
// Cada exportação usa as mesmas consultas das páginas, por isso respeita as mesmas permissões
// (ex.: o orçamento e o conhecimento tácito confidencial só saem a quem os pode ver).

/// <summary>Modelos que se podem exportar.</summary>
public static class ModelosExportacao
{
    public static readonly IReadOnlyDictionary<string, string> Nomes = new Dictionary<string, string>
    {
        ["mod246"] = "Mod.246 Portefólio de iniciativas",
        ["mod239"] = "Mod.239 Indicadores de desempenho",
        ["mod252"] = "Mod.252 Lições aprendidas",
        ["mod237"] = "Mod.237 Conhecimento codificado",
        ["mod238"] = "Mod.238 Conhecimento tácito",
        ["mod235"] = "Mod.235 Acordos de parceria",
        ["mod236"] = "Mod.236 Ativos intangíveis",
        ["mod241"] = "Mod.241 Ferramentas e métodos",
        ["mod52"] = "Mod.52 Estratégia de inovação",
        ["mod69"] = "Mod.69 Plano anual de atividades",
    };
}

[Authorize]
public record ExportarModeloQuery(string Modelo, int? Ano = null, PeriodoKpi? Periodo = null) : IRequest<FicheiroDto>;

public class ExportarModeloQueryHandler(ISender sender, IGeradorExcel excel, TimeProvider relogio) : IRequestHandler<ExportarModeloQuery, FicheiroDto>
{
    private const string TipoXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public async Task<FicheiroDto> Handle(ExportarModeloQuery request, CancellationToken cancellationToken)
    {
        var modelo = request.Modelo.Trim().ToLowerInvariant();
        if (!ModelosExportacao.Nomes.TryGetValue(modelo, out var nome))
        {
            throw new RegraDeNegocioException($"Não há exportação para \"{request.Modelo}\".");
        }

        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);
        var ano = request.Ano ?? hoje.Year;
        var titulo = $"{nome} · exportado a {hoje:dd/MM/yyyy}";

        var folhas = modelo switch
        {
            "mod246" => await PortefolioAsync(titulo, cancellationToken),
            "mod239" => await IndicadoresAsync(titulo, ano, request.Periodo ?? PeriodoKpi.Anual, cancellationToken),
            "mod252" => await LicoesAsync(titulo, cancellationToken),
            "mod237" => await ConhecimentoAsync(titulo, cancellationToken),
            "mod238" => await TacitoAsync(titulo, cancellationToken),
            "mod235" => await ParceriasAsync(titulo, cancellationToken),
            "mod236" => await AtivosAsync(titulo, cancellationToken),
            "mod241" => await FerramentasAsync(titulo, cancellationToken),
            "mod52" => await EstrategiaAsync(titulo, ano, cancellationToken),
            _ => await PlanoAnualAsync(titulo, ano, cancellationToken),
        };

        var sufixo = modelo is "mod239" or "mod52" or "mod69" ? $"_{ano}" : string.Empty;
        var ficheiro = $"{nome.Replace(' ', '_').Replace(".", string.Empty, StringComparison.Ordinal)}{sufixo}.xlsx";
        return new FicheiroDto(new MemoryStream(excel.Gerar(folhas)), ficheiro, TipoXlsx);
    }

    private static IReadOnlyList<object?> L(params object?[] valores) => valores;

    private async Task<IReadOnlyList<FolhaExcel>> PortefolioAsync(string titulo, CancellationToken ct)
    {
        var iniciativas = await sender.Send(new ListarIniciativasQuery(null, null, null), ct);
        var folhas = Enum.GetValues<TipoIniciativa>()
            .Select(tipo => new FolhaExcel(Nomes.TipoIniciativaPlural[tipo], $"{titulo} · {Nomes.TipoIniciativaPlural[tipo]}",
                ["Código", "Título", "Responsável", "Estado", "Início", "Fim", "Origem", "Pilar estratégico", "Ideia de origem", "Ponto de situação"],
                iniciativas.Where(i => i.Tipo == tipo)
                    .Select(i => L(i.Codigo, i.Titulo, i.Responsavel, Nomes.EstadoIniciativa[i.Estado], i.DataInicio, i.DataFim, i.Origem,
                        i.Pilar is { } p ? Nomes.Pilar[p] : null, i.IdeiaOrigemCodigo, i.PontoSituacao))
                    .ToList()))
            .ToList();

        var ideias = await sender.Send(new ListarIdeiasQuery(), ct);
        folhas.Insert(0, new FolhaExcel("Ideias", $"{titulo} · Ideias",
            ["ID Caixa de Inovação", "Código", "Título", "Autor(es)", "Data de submissão", "Responsabilidade", "Nota", "Estado", "Classe"],
            ideias.OrderByDescending(i => i.Data)
                .Select(i => L(i.IdCaixaInovacao, i.Codigo, i.Titulo, i.Autores, i.Data, i.Responsabilidade == Responsabilidade.Dst ? "dst" : "dstelecom",
                    i.Nota, Nomes.EstadoIdeia(i.Estado, i.Classe), i.Classe is { } c ? Nomes.Classe[c] : null))
                .ToList()));
        return folhas;
    }

    private async Task<IReadOnlyList<FolhaExcel>> IndicadoresAsync(string titulo, int ano, PeriodoKpi periodo, CancellationToken ct)
    {
        var d = await sender.Send(new ObterIndicadoresQuery(ano, periodo), ct);
        var nomePeriodo = Nomes.Periodo[periodo];
        return
        [
            new FolhaExcel($"KPIs {ano}", $"{titulo} · {nomePeriodo} de {ano}",
                ["N.º", "Indicador", "Categoria", $"{nomePeriodo} {ano}", $"{nomePeriodo} {ano - 1}", "Objetivo anual", "Objetivo atingido", "Nota"],
                d.Kpis.Select(k => L(k.Numero, k.Nome, k.Categoria, k.Valor, k.ValorAnoAnterior, k.Objetivo, k.Atingido, k.Nota)).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> LicoesAsync(string titulo, CancellationToken ct)
    {
        var licoes = await sender.Send(new ListarLicoesQuery(null, null), ct);
        return
        [
            new FolhaExcel("Lições aprendidas", titulo, ["Data", "Iniciativa", "Título da iniciativa", "Categoria", "Lição", "Descrição", "Impacto"],
                licoes.Select(l => L(l.Data, l.IniciativaCodigo, l.IniciativaTitulo, Nomes.CategoriaLicao[l.Categoria], l.Licao, l.Descricao, l.Impacto))
                    .ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> ConhecimentoAsync(string titulo, CancellationToken ct)
    {
        var itens = await sender.Send(new ListarConhecimentoCodificadoQuery(null, null), ct);
        return
        [
            new FolhaExcel("Conhecimento codificado", titulo,
                ["Data", "Documento", "Arquivado em", "Modo de consulta", "Link", "Descrição do conhecimento", "Atividades de partilha (quem, quando)",
                    "Resultado obtido", "Origem", "Iniciativa"],
                itens.Select(c => L(c.Data, c.Documento, c.ArquivadoEm, c.ModoConsulta, c.Url, c.Descricao, Atividades(c.Atividades), c.Resultado,
                    Nomes.Origem[c.Origem], c.IniciativaCodigo)).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> TacitoAsync(string titulo, CancellationToken ct)
    {
        var itens = await sender.Send(new ListarConhecimentoTacitoQuery(), ct);
        return
        [
            new FolhaExcel("Conhecimento tácito", titulo, ["Data", "Empresa", "Detentor", "Descrição do conhecimento tácito", "Confidencialidade"],
                itens.Select(c => L(c.Data, c.Empresa, c.Detentor, c.Descricao, c.Confidencialidade == NivelConfidencialidade.Elevado ? "elevada" : "baixa"))
                    .ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> ParceriasAsync(string titulo, CancellationToken ct)
    {
        var acordos = await sender.Send(new ListarAcordosQuery(null), ct);
        var parceiros = await sender.Send(new ListarParceirosQuery(), ct);
        return
        [
            new FolhaExcel("Acordos", $"{titulo} · Acordos",
                ["Entidade", "Tipo de parceiro", "Descrição da parceria", "Protocolo estabelecido", "Tipo de protocolo", "Início", "Fim",
                    "Inclui gestão de PI", "Estado", "Observações", "Documento"],
                acordos.Select(a => L(a.Parceiro, Nomes.TipoParceiro[a.TipoParceiro], a.Descricao, a.ProtocoloEstabelecido,
                    a.TipoProtocolo is { } t ? Nomes.TipoProtocolo[t] : null, a.DataInicio, a.DataFim ?? (object)"sem fim",
                    a.IncluiGestaoPropriedadeIntelectual, a.Estado == EstadoAcordo.EmCurso ? "em curso" : "terminado", a.Observacoes, a.LinkDocumento))
                    .ToList()),
            new FolhaExcel("Parceiros", $"{titulo} · Parceiros", ["Parceiro", "NIF", "Tipo", "Acordos", "Acordos em curso", "Iniciativas"],
                parceiros.Select(p => L(p.Nome, p.Nif, Nomes.TipoParceiro[p.Tipo], p.Acordos, p.AcordosEmCurso,
                    string.Join(", ", p.Iniciativas.Select(i => i.Codigo)))).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> AtivosAsync(string titulo, CancellationToken ct)
    {
        var ativos = await sender.Send(new ListarAtivosQuery(), ct);
        return
        [
            new FolhaExcel("Ativos intangíveis", titulo,
                ["Data", "Tipo", "Entidade de registo", "Designação", "Descrição", "N.º do pedido", "Inventores / autores", "Data de registo",
                    "Data de validade", "Âmbito", "Situação atual", "Modelo de gestão (atividades, quem, quando)", "Custo (€)", "Perspetiva de valor",
                    "N.º do processo", "Observações"],
                ativos.Select(a => L(a.Data, Nomes.TipoAtivo[a.Tipo], a.EntidadeRegisto, a.Designacao, a.Descricao, a.NumeroPedido, a.Autores,
                    a.DataRegisto, a.DataValidade, a.Ambito, a.Situacao, Atividades(a.Atividades), a.Custo, a.PerspetivaValor, a.NumeroProcesso,
                    a.Observacoes)).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> FerramentasAsync(string titulo, CancellationToken ct)
    {
        var ferramentas = await sender.Send(new ListarFerramentasQuery(), ct);
        var anos = ferramentas.SelectMany(f => f.Analises).Select(a => a.Ano).Distinct().Order().ToList();
        return
        [
            new FolhaExcel("Ferramentas e métodos", titulo,
                ["Data", "Identificação", "Sumário executivo", "Potencial de utilização", "Custos de aquisição (€)", "Divulgação",
                    .. anos.Select(a => $"Análise {a}")],
                ferramentas.Select(f => L([f.Data, f.Nome, f.SumarioExecutivo, f.Potencial, f.CustoAquisicao, f.Divulgacao,
                    .. anos.Select(a => f.Analises.FirstOrDefault(x => x.Ano == a)?.Analise)])).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> EstrategiaAsync(string titulo, int ano, CancellationToken ct)
    {
        var linhas = await sender.Send(new ListarEstrategiaQuery(ano), ct);
        return
        [
            new FolhaExcel($"Estratégia {ano}", $"{titulo} · {ano}",
                ["Área de atuação", "Abordagem / aplicação específica", "Plano de ação", "KPI associado", "Duração (meses)", "N.º RH",
                    "Horas/mês dos RH", "Dedicação horária total", "Recursos extra", "Horizonte", "Iniciativa"],
                linhas.Select(l => L(l.AreaAtuacao, l.Abordagem, l.PlanoAcao, string.Join(" e ", l.Kpis.Select(k => k.Numero)), l.DuracaoMeses,
                    l.NumeroRecursosHumanos, l.HorasMesPorRecurso, l.DedicacaoTotalHoras, l.RecursosExtra, l.Horizonte?.ToString(),
                    l.IniciativaCodigo)).ToList()),
        ];
    }

    private async Task<IReadOnlyList<FolhaExcel>> PlanoAnualAsync(string titulo, int ano, CancellationToken ct)
    {
        var atividades = await sender.Send(new ListarPlanoAnualQuery(ano), ct);
        return
        [
            new FolhaExcel($"Plano {ano}", $"{titulo} · {ano}", ["Atividade", "Descrição", "Responsáveis", "Repete-se", "Duração (dias)", "Datas em " + ano],
                atividades.Select(a => L(a.Titulo, a.Descricao, string.Join(", ", a.Responsaveis.Select(r => r.Nome)), Nomes.Periodicidade[a.Recorrencia],
                    a.DuracaoDias + 1, string.Join(", ", a.Ocorrencias.Select(o => o.ToString("dd/MM", System.Globalization.CultureInfo.InvariantCulture)))))
                    .ToList()),
        ];
    }

    private static string? Atividades(IReadOnlyList<AtividadeGestaoDto>? atividades)
        => atividades is null || atividades.Count == 0
            ? null
            : string.Join("\n", atividades.Select(a => string.Join(" · ",
                new[] { a.Descricao, a.Responsavel, a.Data?.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture), a.Concluida ? "feita" : null }
                    .Where(x => !string.IsNullOrWhiteSpace(x)))));
}

/// <summary>Nomes em português dos valores (os mesmos da plataforma).</summary>
internal static class Nomes
{
    public static readonly IReadOnlyDictionary<TipoIniciativa, string> TipoIniciativaPlural = new Dictionary<TipoIniciativa, string>
    {
        [TipoIniciativa.Projeto] = "Projetos",
        [TipoIniciativa.Desafio] = "Desafios",
        [TipoIniciativa.MelhoriaContinua] = "Melhoria contínua",
        [TipoIniciativa.Oportunidade] = "Oportunidades",
        [TipoIniciativa.Vigilancia] = "Vigilâncias",
        [TipoIniciativa.Mestrado] = "Mestrados",
        [TipoIniciativa.AcaoInovacao] = "Ações de inovação",
    };

    public static readonly IReadOnlyDictionary<EstadoIniciativa, string> EstadoIniciativa = new Dictionary<EstadoIniciativa, string>
    {
        [Domain.Enums.EstadoIniciativa.Pendente] = "pendente",
        [Domain.Enums.EstadoIniciativa.EmAnalise] = "em análise",
        [Domain.Enums.EstadoIniciativa.EmCurso] = "em curso",
        [Domain.Enums.EstadoIniciativa.EmImplementacao] = "em implementação",
        [Domain.Enums.EstadoIniciativa.Concluida] = "concluída",
        [Domain.Enums.EstadoIniciativa.Rejeitada] = "rejeitada",
        [Domain.Enums.EstadoIniciativa.Cancelada] = "cancelada",
    };

    public static readonly IReadOnlyDictionary<ClasseIdeia, string> Classe = new Dictionary<ClasseIdeia, string>
    {
        [ClasseIdeia.MelhoriaContinua] = "Melhoria Contínua",
        [ClasseIdeia.Projeto] = "Projeto",
        [ClasseIdeia.Desafio] = "Desafio",
        [ClasseIdeia.IdeiaDuplicada] = "Ideia Duplicada",
        [ClasseIdeia.Arquivada] = "Arquivada",
        [ClasseIdeia.NaoAplicavel] = "N.A.",
    };

    /// <summary>Os estados das ideias com os nomes que a dstelecom usa (reunião de 13/10).</summary>
    public static string EstadoIdeia(EstadoIdeia estado, ClasseIdeia? classe)
    {
        if (estado == Domain.Enums.EstadoIdeia.Aprovada && classe is ClasseIdeia.Projeto or ClasseIdeia.Desafio)
        {
            return classe == ClasseIdeia.Projeto ? "passada a projeto" : "passada a desafio";
        }

        if (classe is ClasseIdeia.Arquivada or ClasseIdeia.IdeiaDuplicada)
        {
            return "arquivada";
        }

        return estado switch
        {
            Domain.Enums.EstadoIdeia.ValidacaoAutores => "validação dos autores",
            Domain.Enums.EstadoIdeia.ValidacaoEquipa => "validação da equipa de inovação",
            Domain.Enums.EstadoIdeia.EmDiscussao => "em discussão pública",
            Domain.Enums.EstadoIdeia.EmAvaliacao => "em análise pela equipa",
            Domain.Enums.EstadoIdeia.Aprovada => "aprovada",
            _ => "rejeitada",
        };
    }

    public static readonly IReadOnlyDictionary<PilarEstrategico, string> Pilar = new Dictionary<PilarEstrategico, string>
    {
        [PilarEstrategico.Pilar1] = "Pilar 1 · Modelo de Negócio Core",
        [PilarEstrategico.Pilar2] = "Pilar 2 · Exportação do Modelo de Negócio",
        [PilarEstrategico.Pilar3] = "Pilar 3 · Ambiente e Prosperidade do Planeta",
    };

    public static readonly IReadOnlyDictionary<PeriodoKpi, string> Periodo = new Dictionary<PeriodoKpi, string>
    {
        [PeriodoKpi.PrimeiroSemestre] = "1.º semestre",
        [PeriodoKpi.SegundoSemestre] = "2.º semestre",
        [PeriodoKpi.Anual] = "Ano",
    };

    public static readonly IReadOnlyDictionary<CategoriaLicao, string> CategoriaLicao = new Dictionary<CategoriaLicao, string>
    {
        [Domain.Enums.CategoriaLicao.Estrategica] = "estratégica",
        [Domain.Enums.CategoriaLicao.Gestao] = "gestão",
        [Domain.Enums.CategoriaLicao.Operacional] = "operacional",
        [Domain.Enums.CategoriaLicao.Tecnica] = "técnica",
    };

    public static readonly IReadOnlyDictionary<OrigemConhecimento, string> Origem = new Dictionary<OrigemConhecimento, string>
    {
        [OrigemConhecimento.Manual] = "manual",
        [OrigemConhecimento.RelatorioIniciativa] = "relatório de iniciativa",
        [OrigemConhecimento.Vigilancia] = "vigilância",
    };

    public static readonly IReadOnlyDictionary<TipoParceiro, string> TipoParceiro = new Dictionary<TipoParceiro, string>
    {
        [Domain.Enums.TipoParceiro.Empresarial] = "empresarial",
        [Domain.Enums.TipoParceiro.Academico] = "académico (ENESI)",
    };

    public static readonly IReadOnlyDictionary<TipoProtocolo, string> TipoProtocolo = new Dictionary<TipoProtocolo, string>
    {
        [Domain.Enums.TipoProtocolo.InvestigacaoPartilhada] = "investigação partilhada",
        [Domain.Enums.TipoProtocolo.CoPromocao] = "co-promoção",
        [Domain.Enums.TipoProtocolo.Confidencialidade] = "acordo de confidencialidade (NDA)",
        [Domain.Enums.TipoProtocolo.Inovacao] = "inovação",
        [Domain.Enums.TipoProtocolo.Investigacao] = "investigação",
    };

    public static readonly IReadOnlyDictionary<TipoAtivoIntangivel, string> TipoAtivo = new Dictionary<TipoAtivoIntangivel, string>
    {
        [TipoAtivoIntangivel.Patente] = "patente",
        [TipoAtivoIntangivel.Design] = "design",
        [TipoAtivoIntangivel.Marca] = "marca",
    };

    public static readonly IReadOnlyDictionary<Periodicidade, string> Periodicidade = new Dictionary<Periodicidade, string>
    {
        [Domain.Enums.Periodicidade.Nenhuma] = "não se repete",
        [Domain.Enums.Periodicidade.Semanal] = "semanal",
        [Domain.Enums.Periodicidade.Mensal] = "mensal",
        [Domain.Enums.Periodicidade.Trimestral] = "trimestral",
        [Domain.Enums.Periodicidade.Semestral] = "semestral",
        [Domain.Enums.Periodicidade.Anual] = "anual",
    };
}
