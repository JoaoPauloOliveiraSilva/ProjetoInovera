namespace Innovera.Application.Kpis;

// Indicadores de desempenho (Mod.239): os 41 KPIs da dstelecom, calculados a partir dos registos da plataforma
// e dos valores externos introduzidos à mão (RF08, RF09), e o dashboard inicial (secção 12 da especificação).

public enum UnidadeKpi
{
    Numero,
    Percentagem,
    Euros,
    Media
}

public record KpiDto(int Numero, string Nome, string Categoria, UnidadeKpi Unidade, decimal? Valor, decimal? ValorAnoAnterior, bool Disponivel,
    string? Nota, string? Objetivo = null, bool? Atingido = null, int AcoesAbertas = 0);

public record ContagemEstadoDto(TipoIniciativa Tipo, EstadoIniciativa Estado, int Quantidade);

public record ProximaAtividadeDto(int Id, string Titulo, DateOnly Data, string? Cor);

public record AlertaDashboardDto(string Tipo, string Titulo, string Detalhe, DateOnly? Data, string Link);

public record IndicadoresDto(int Ano, PeriodoKpi Periodo, IReadOnlyList<KpiDto> Kpis, IReadOnlyList<ContagemEstadoDto> IniciativasPorEstado,
    IReadOnlyList<ProximaAtividadeDto> ProximasAtividades, IReadOnlyList<AlertaDashboardDto> Alertas);

/// <summary>
/// Os 41 indicadores de um período (1.º semestre, 2.º semestre ou ano completo), com o mesmo período do ano anterior
/// para comparar, o objetivo anual de cada um e o resumo para o dashboard.
/// </summary>
[Authorize]
public record ObterIndicadoresQuery(int Ano, PeriodoKpi Periodo = PeriodoKpi.Anual) : IRequest<IndicadoresDto>;

public class ObterIndicadoresQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<ObterIndicadoresQuery, IndicadoresDto>
{
    public async Task<IndicadoresDto> Handle(ObterIndicadoresQuery request, CancellationToken cancellationToken)
    {
        var dados = await DadosKpi.CarregarAsync(context, cancellationToken);
        var veOrcamento = await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Ver, cancellationToken)
            || await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Gerir, cancellationToken);

        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);
        var atual = CalculoKpi.Calcular(dados, request.Ano, request.Periodo);
        var anterior = CalculoKpi.Calcular(dados, request.Ano - 1, request.Periodo);

        // Valores oficiais importados do Mod.239: nos períodos já fechados prevalecem sobre o cálculo (que só conhece o que está na plataforma).
        var importados = (await context.ValoresKpi.AsNoTracking()
                .Where(v => (v.Ano == request.Ano || v.Ano == request.Ano - 1) && v.Periodo == request.Periodo && v.ValorAbsoluto != null)
                .Select(v => new { v.Kpi.Numero, v.Ano, v.ValorAbsoluto })
                .ToListAsync(cancellationToken))
            .Where(v => CalculoKpi.Intervalo(v.Ano, request.Periodo).Fim < hoje)
            .ToDictionary(v => (v.Numero, v.Ano), v => v.ValorAbsoluto);
        var objetivos = await context.ValoresKpi.AsNoTracking()
            .Where(v => v.Ano == request.Ano && v.Periodo == PeriodoKpi.Anual)
            .Select(v => new { v.Kpi.Numero, v.Objetivo, Abertas = v.Acoes.Count(a => a.ConcluidaEm == null) })
            .ToDictionaryAsync(v => v.Numero, cancellationToken);
        var kpis = CatalogoKpi.Todos.Select(k =>
        {
            var (valor, nota) = atual.GetValueOrDefault(k.Numero);
            var (valorAnterior, _) = anterior.GetValueOrDefault(k.Numero);
            if (importados.TryGetValue((k.Numero, request.Ano), out var oficial))
            {
                (valor, nota) = (oficial, "Valor do Mod.239 (importado).");
            }

            if (importados.TryGetValue((k.Numero, request.Ano - 1), out var oficialAnterior))
            {
                valorAnterior = oficialAnterior;
            }

            if (k.Financeiro && !veOrcamento)
            {
                return new KpiDto(k.Numero, k.Nome, k.Categoria, k.Unidade, null, null, false, "Só para quem pode ver o orçamento.");
            }

            var plano = objetivos.GetValueOrDefault(k.Numero);
            // O objetivo é anual: só se avalia com o ano completo.
            var atingido = request.Periodo == PeriodoKpi.Anual ? ObjetivoKpi.Atingido(plano?.Objetivo, valor) : null;
            return new KpiDto(k.Numero, k.Nome, k.Categoria, k.Unidade, valor, valorAnterior, valor is not null, nota ?? k.Nota,
                plano?.Objetivo, atingido, plano?.Abertas ?? 0);
        }).ToList();

        return new IndicadoresDto(request.Ano, request.Periodo, kpis, PorEstado(dados), await ProximasAsync(hoje, cancellationToken),
            await AlertasAsync(dados, hoje, cancellationToken));
    }

    private static List<ContagemEstadoDto> PorEstado(DadosKpi dados)
        => dados.Iniciativas
            .GroupBy(i => (i.Tipo, i.Estado))
            .Select(g => new ContagemEstadoDto(g.Key.Tipo, g.Key.Estado, g.Count()))
            .OrderBy(c => c.Tipo).ThenBy(c => c.Estado)
            .ToList();

    private async Task<List<ProximaAtividadeDto>> ProximasAsync(DateOnly hoje, CancellationToken cancellationToken)
    {
        var atividades = await context.AtividadesPlanoAnual.AsNoTracking().ToListAsync(cancellationToken);
        return atividades
            .SelectMany(a => a.Ocorrencias(hoje, hoje.AddDays(30)).Select(d => new ProximaAtividadeDto(a.Id, a.Titulo, d, a.Cor)))
            .OrderBy(a => a.Data)
            .Take(8)
            .ToList();
    }

    private async Task<List<AlertaDashboardDto>> AlertasAsync(DadosKpi dados, DateOnly hoje, CancellationToken cancellationToken)
    {
        var alertas = dados.Ativos
            .Where(a => a.DataValidade is { } v && v >= hoje && v <= hoje.AddDays(90))
            .OrderBy(a => a.DataValidade)
            .Select(a => new AlertaDashboardDto("validade", a.Designacao, $"A validade termina a {a.DataValidade:dd/MM/yyyy}.", a.DataValidade,
                "/inovacao/ativos"))
            .ToList();

        var tarefas = await context.TarefasWbs.AsNoTracking()
            .Where(t => t.FimReal == null && t.FimPlaneado != null && t.FimPlaneado < hoje)
            .Select(t => new { t.Nome, t.FimPlaneado, t.Charter.IniciativaId, t.Charter.Iniciativa.Codigo })
            .OrderBy(t => t.FimPlaneado)
            .Take(10)
            .ToListAsync(cancellationToken);
        alertas.AddRange(tarefas.Select(t => new AlertaDashboardDto("atraso", $"{t.Codigo} · {t.Nome}",
            $"Devia ter terminado a {t.FimPlaneado:dd/MM/yyyy}.", t.FimPlaneado, $"/inovacao/iniciativas/{t.IniciativaId}/charter?separador=planeamento")));

        var riscos = await context.Riscos.AsNoTracking()
            .Where(r => (int)r.Impacto * (int)r.Probabilidade > Risco.SeveridadeMaximaSemMitigacao && (r.PlanoAcao == null || r.PlanoAcao == ""))
            .Select(r => new { r.Descricao, r.Charter.IniciativaId, r.Charter.Iniciativa.Codigo })
            .Take(10)
            .ToListAsync(cancellationToken);
        alertas.AddRange(riscos.Select(r => new AlertaDashboardDto("risco", $"{r.Codigo} · {r.Descricao}", "Risco elevado sem plano de ação.", null,
            $"/inovacao/iniciativas/{r.IniciativaId}/charter?separador=riscos")));

        var vigilancias = await context.Iniciativas.OfType<Vigilancia>().AsNoTracking()
            .Where(v => v.Estado != EstadoIniciativa.Cancelada && v.Estado != EstadoIniciativa.Concluida)
            .SelectMany(v => v.Execucoes.Where(e => e.DataRealizada == null && e.DataPrevista < hoje)
                .Select(e => new { e.DataPrevista, v.Id, v.Codigo, v.Titulo }))
            .OrderBy(e => e.DataPrevista)
            .Take(10)
            .ToListAsync(cancellationToken);
        alertas.AddRange(vigilancias.Select(v => new AlertaDashboardDto("vigilancia", $"{v.Codigo} · {v.Titulo}",
            $"Vigilância prevista para {v.DataPrevista:dd/MM/yyyy} ainda sem resultado.", v.DataPrevista, $"/inovacao/iniciativas/{v.Id}")));

        return alertas;
    }
}

/// <summary>Definição dos 41 indicadores (número, nome e categoria como no Mod.239).</summary>
public record DefinicaoKpi(int Numero, string Nome, string Categoria, UnidadeKpi Unidade, bool Financeiro = false, string? Nota = null);

public static class CatalogoKpi
{
    public static readonly IReadOnlyList<DefinicaoKpi> Todos =
    [
        new(1, "N.º de ideias submetidas", "Ideias", UnidadeKpi.Numero),
        new(2, "N.º de ideias aprovadas", "Ideias", UnidadeKpi.Numero),
        new(3, "N.º de ideias não aprovadas", "Ideias", UnidadeKpi.Numero),
        new(4, "N.º de ideias que passaram a projeto", "Ideias", UnidadeKpi.Numero),
        new(5, "N.º de ideias que passaram a desafios", "Ideias", UnidadeKpi.Numero),
        new(6, "N.º de ideias que passaram a melhoria contínua", "Ideias", UnidadeKpi.Numero),
        new(7, "N.º de ideias arquivadas", "Ideias", UnidadeKpi.Numero),
        new(8, "N.º de trabalhadores a postar ideias", "Ideias", UnidadeKpi.Numero),
        new(9, "N.º total de trabalhadores dstelecom", "Ideias", UnidadeKpi.Numero, Nota: "Valor externo (introduzido pelo Administrador)."),
        new(10, "Desafios novos", "Desafios", UnidadeKpi.Numero),
        new(11, "Desafios em curso", "Desafios", UnidadeKpi.Numero, Nota: "Estado atual."),
        new(12, "Desafios terminados", "Desafios", UnidadeKpi.Numero),
        new(13, "Projetos novos", "Projetos", UnidadeKpi.Numero),
        new(14, "Projetos em curso", "Projetos", UnidadeKpi.Numero, Nota: "Estado atual."),
        new(15, "Projetos terminados", "Projetos", UnidadeKpi.Numero),
        new(16, "Melhorias contínuas novas", "Melhoria contínua", UnidadeKpi.Numero),
        new(17, "Melhorias contínuas em curso", "Melhoria contínua", UnidadeKpi.Numero, Nota: "Estado atual."),
        new(18, "Melhorias contínuas terminadas", "Melhoria contínua", UnidadeKpi.Numero),
        new(19, "Projetos de mestrado novos", "Mestrados", UnidadeKpi.Numero),
        new(20, "Projetos de mestrado em curso", "Mestrados", UnidadeKpi.Numero, Nota: "Estado atual."),
        new(21, "Projetos de mestrado terminados", "Mestrados", UnidadeKpi.Numero),
        new(22, "Entradas no Repositório de Conhecimento", "Conhecimento", UnidadeKpi.Numero),
        new(23, "Vigilâncias ativas", "Conhecimento", UnidadeKpi.Numero, Nota: "Estado atual."),
        new(24, "% de vigilâncias dentro do prazo", "Conhecimento", UnidadeKpi.Percentagem),
        new(25, "N.º de parceiros com que trabalhamos", "Parcerias", UnidadeKpi.Numero),
        new(26, "% de parceiros empresariais", "Parcerias", UnidadeKpi.Percentagem),
        new(27, "% de parceiros académicos", "Parcerias", UnidadeKpi.Percentagem),
        new(28, "Propriedade criada", "Propriedade intelectual", UnidadeKpi.Numero),
        new(29, "% patentes", "Propriedade intelectual", UnidadeKpi.Percentagem),
        new(30, "% registo de design", "Propriedade intelectual", UnidadeKpi.Percentagem),
        new(31, "% registo de marca", "Propriedade intelectual", UnidadeKpi.Percentagem),
        new(32, "N.º de eventos criados da inovação", "Cultura", UnidadeKpi.Numero),
        new(33, "Média de participantes por evento", "Cultura", UnidadeKpi.Media),
        new(34, "N.º de eventos externos em que participamos", "Cultura", UnidadeKpi.Numero),
        new(35, "N.º de projetos e desafios do pilar 1 (Modelo de Negócio Core)", "Estratégia", UnidadeKpi.Numero),
        new(36, "N.º de projetos e desafios do pilar 2 (Exportação do Modelo de Negócio)", "Estratégia", UnidadeKpi.Numero),
        new(37, "N.º de projetos e desafios do pilar 3 (Ambiente e Prosperidade do Planeta)", "Estratégia", UnidadeKpi.Numero),
        new(38, "Investimento em atividades de IDI", "Orçamento", UnidadeKpi.Euros, true, "Soma do orçamento real dos Project Charters."),
        new(39, "Proveito das atividades de IDI", "Orçamento", UnidadeKpi.Euros, true, "Valor externo (introduzido pelo Administrador)."),
        new(40, "Investimento IDI / Receitas", "Orçamento", UnidadeKpi.Percentagem, true),
        new(41, "Proveito IDI / Investimento IDI", "Orçamento", UnidadeKpi.Percentagem, true),
    ];
}

/// <summary>Os registos de que os indicadores precisam, lidos uma vez (são poucas centenas de linhas).</summary>
public class DadosKpi
{
    public required List<Ideia> Ideias { get; init; }
    public required List<Iniciativa> Iniciativas { get; init; }
    public required List<DateOnly> ConhecimentoDatas { get; init; }
    public required List<ExecucaoVigilancia> Execucoes { get; init; }
    public required List<AcordoParceria> Acordos { get; init; }
    public required List<AtivoIntangivel> Ativos { get; init; }
    public required List<LinhaOrcamento> Orcamento { get; init; }
    public required List<ValorExterno> Externos { get; init; }

    public static async Task<DadosKpi> CarregarAsync(IApplicationDbContext context, CancellationToken cancellationToken) => new()
    {
        Ideias = await context.Ideias.AsNoTracking().Include(i => i.Coautores).ToListAsync(cancellationToken),
        Iniciativas = await context.Iniciativas.AsNoTracking().ToListAsync(cancellationToken),
        ConhecimentoDatas = await context.ConhecimentosCodificados.AsNoTracking().Select(c => c.Data).ToListAsync(cancellationToken),
        Execucoes = await context.Iniciativas.OfType<Vigilancia>().AsNoTracking().SelectMany(v => v.Execucoes).ToListAsync(cancellationToken),
        Acordos = await context.AcordosParceria.AsNoTracking().Include(a => a.Parceiro).ToListAsync(cancellationToken),
        Ativos = await context.AtivosIntangiveis.AsNoTracking().ToListAsync(cancellationToken),
        Orcamento = await context.LinhasOrcamento.AsNoTracking().ToListAsync(cancellationToken),
        Externos = await context.ValoresExternos.AsNoTracking().ToListAsync(cancellationToken),
    };
}

/// <summary>Regras de cálculo de cada indicador para um ano. Devolve (valor, nota) por número de KPI.</summary>
public static class CalculoKpi
{
    private static readonly EstadoIniciativa[] EmCurso = [EstadoIniciativa.EmCurso, EstadoIniciativa.EmImplementacao];

    public static Dictionary<int, (decimal? Valor, string? Nota)> Calcular(DadosKpi d, int ano, PeriodoKpi periodo = PeriodoKpi.Anual)
    {
        var (inicio, fim) = Intervalo(ano, periodo);
        bool NoAno(DateOnly? data) => data is { } x && x >= inicio && x <= fim;
        var r = new Dictionary<int, (decimal?, string?)>();
        void Por(int n, decimal? valor, string? nota = null) => r[n] = (valor, nota);
        static decimal? Pct(int parte, int total) => total == 0 ? null : Math.Round(100m * parte / total, 1);

        // 1–8 Ideias (pelo ano de submissão)
        var ideias = d.Ideias.Where(i => NoAno(DateOnly.FromDateTime(i.DataSubmissao.UtcDateTime))).ToList();
        Por(1, ideias.Count);
        Por(2, ideias.Count(i => i.Estado == EstadoIdeia.Aprovada));
        Por(3, ideias.Count(i => i.Estado == EstadoIdeia.NaoAprovada));
        Por(4, ideias.Count(i => i.Classe == ClasseIdeia.Projeto));
        Por(5, ideias.Count(i => i.Classe == ClasseIdeia.Desafio));
        Por(6, ideias.Count(i => i.Classe == ClasseIdeia.MelhoriaContinua));
        Por(7, ideias.Count(i => i.Classe == ClasseIdeia.Arquivada));
        var autores = new HashSet<string>();
        foreach (var i in ideias.Where(i => !i.Anonima))
        {
            if (i.AutorId is { } id)
            {
                autores.Add($"u{id}");
            }
            else if (!string.IsNullOrWhiteSpace(i.AutorNome) && !i.AutorNome.Contains("anónimo", StringComparison.OrdinalIgnoreCase))
            {
                autores.Add($"n{i.AutorNome.Trim().ToLowerInvariant()}");
            }

            foreach (var c in i.Coautores)
            {
                autores.Add($"u{c.UtilizadorId}");
            }
        }

        Por(8, autores.Count);
        var trabalhadores = Externo(d, TipoValorExterno.NumeroTrabalhadores, ano, periodo);
        Por(9, trabalhadores, trabalhadores is null ? "Falta o n.º de trabalhadores em valores externos." : null);

        // 10–21 Desafios, projetos, melhorias e mestrados
        void Tipo(TipoIniciativa tipo, int novos)
        {
            var lista = d.Iniciativas.Where(i => i.Tipo == tipo).ToList();
            Por(novos, lista.Count(i => NoAno(i.DataInicio)));
            Por(novos + 1, lista.Count(i => EmCurso.Contains(i.Estado) && (i.DataInicio is null || i.DataInicio <= fim)));
            Por(novos + 2, lista.Count(i => i.Estado == EstadoIniciativa.Concluida && NoAno(i.DataFim)));
        }

        Tipo(TipoIniciativa.Desafio, 10);
        Tipo(TipoIniciativa.Projeto, 13);
        Tipo(TipoIniciativa.MelhoriaContinua, 16);
        Tipo(TipoIniciativa.Mestrado, 19);

        // 22–24 Conhecimento e vigilâncias
        Por(22, d.ConhecimentoDatas.Count(x => NoAno(x)));
        Por(23, d.Iniciativas.Count(i => i.Tipo == TipoIniciativa.Vigilancia && EmCurso.Contains(i.Estado)));
        var execucoes = d.Execucoes.Where(e => NoAno(e.DataPrevista) && e.DataRealizada is not null).ToList();
        Por(24, Pct(execucoes.Count(e => e.NoPrazo), execucoes.Count), execucoes.Count == 0 ? "Ainda sem resultados de vigilâncias registados." : null);

        // 25–27 Parceiros com acordo vigente no ano
        var parceiros = d.Acordos
            .Where(a => (a.DataInicio ?? DateOnly.MinValue) <= fim
                && (a.DataFim ?? (a.Estado == EstadoAcordo.EmCurso ? DateOnly.MaxValue : DateOnly.MinValue)) >= inicio)
            .Select(a => a.Parceiro)
            .DistinctBy(p => p.Id)
            .ToList();
        Por(25, parceiros.Count);
        Por(26, Pct(parceiros.Count(p => p.Tipo == TipoParceiro.Empresarial), parceiros.Count));
        Por(27, Pct(parceiros.Count(p => p.Tipo == TipoParceiro.Academico), parceiros.Count));

        // 28–31 Propriedade intelectual registada no ano
        var ativos = d.Ativos.Where(a => NoAno(a.DataRegisto ?? a.Data)).ToList();
        Por(28, ativos.Count);
        Por(29, Pct(ativos.Count(a => a.Tipo == TipoAtivoIntangivel.Patente), ativos.Count));
        Por(30, Pct(ativos.Count(a => a.Tipo == TipoAtivoIntangivel.Design), ativos.Count));
        Por(31, Pct(ativos.Count(a => a.Tipo == TipoAtivoIntangivel.Marca), ativos.Count));

        // 32–34 Ações de inovação
        var acoes = d.Iniciativas.OfType<AcaoInovacao>().Where(a => NoAno(a.DataInicio)).ToList();
        var criadas = acoes.Where(a => a.TipoAcao == TipoAcaoInovacao.Criada).ToList();
        Por(32, criadas.Count);
        var comParticipantes = criadas.Where(a => a.NumeroParticipantes is not null).ToList();
        Por(33, comParticipantes.Count == 0 ? null : Math.Round((decimal)comParticipantes.Average(a => a.NumeroParticipantes!.Value), 1),
            comParticipantes.Count == 0 ? "Sem n.º de participantes registado nos eventos." : null);
        Por(34, acoes.Count(a => a.TipoAcao == TipoAcaoInovacao.Externa));

        // 35–37 Projetos e desafios ativos no ano, por pilar
        var ativas = d.Iniciativas
            .Where(i => i.Tipo is TipoIniciativa.Projeto or TipoIniciativa.Desafio)
            .Where(i => i.Estado is not (EstadoIniciativa.Rejeitada or EstadoIniciativa.Cancelada))
            .Where(i => (i.DataInicio ?? DateOnly.MinValue) <= fim && (i.DataFim ?? DateOnly.MaxValue) >= inicio)
            .ToList();
        Por(35, ativas.Count(i => i.PilarEstrategico == PilarEstrategico.Pilar1));
        Por(36, ativas.Count(i => i.PilarEstrategico == PilarEstrategico.Pilar2));
        Por(37, ativas.Count(i => i.PilarEstrategico == PilarEstrategico.Pilar3));

        // 38–41 Orçamento e valores externos
        // O orçamento dos Charters é anual: num semestre mostra-se o do ano.
        var linhas = d.Orcamento.Where(l => l.Ano == ano && l.ValorReal is not null).ToList();
        decimal? investimento = linhas.Count == 0 ? null : linhas.Sum(l => l.ValorReal!.Value);
        Por(38, investimento, investimento is null ? "Sem orçamento real registado nos Project Charters."
            : periodo == PeriodoKpi.Anual ? null : "Valor do ano (o orçamento dos Charters é anual).");
        var proveito = Externo(d, TipoValorExterno.ProveitosIdi, ano, periodo);
        Por(39, proveito, proveito is null ? "Falta o proveito de IDI em valores externos." : null);
        var receitas = Externo(d, TipoValorExterno.Receitas, ano, periodo);
        Por(40, investimento is not null && receitas is > 0m ? Math.Round(100m * investimento.Value / receitas.Value, 2) : null,
            receitas is null ? "Falta o valor das receitas em valores externos." : null);
        Por(41, proveito is not null && investimento is > 0m ? Math.Round(100m * proveito.Value / investimento.Value, 1) : null);

        return r;
    }

    /// <summary>Início e fim do período: 1.º semestre (jan–jun), 2.º semestre (jul–dez) ou o ano completo.</summary>
    public static (DateOnly Inicio, DateOnly Fim) Intervalo(int ano, PeriodoKpi periodo) => periodo switch
    {
        PeriodoKpi.PrimeiroSemestre => (new DateOnly(ano, 1, 1), new DateOnly(ano, 6, 30)),
        PeriodoKpi.SegundoSemestre => (new DateOnly(ano, 7, 1), new DateOnly(ano, 12, 31)),
        _ => (new DateOnly(ano, 1, 1), new DateOnly(ano, 12, 31)),
    };

    /// <summary>Valor externo do período; se não houver, o valor anual.</summary>
    private static decimal? Externo(DadosKpi d, TipoValorExterno tipo, int ano, PeriodoKpi periodo)
    {
        var doAno = d.Externos.Where(v => v.Tipo == tipo && v.Ano == ano).ToList();
        return (doAno.FirstOrDefault(v => v.Periodo == periodo) ?? doAno.FirstOrDefault(v => v.Periodo == PeriodoKpi.Anual))?.Valor;
    }
}
