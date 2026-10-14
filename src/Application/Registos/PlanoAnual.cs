using Innovera.Application.Utilizadores;

namespace Innovera.Application.Registos;

// Plano anual de atividades (Mod.69): atividades com datas, recorrência, cor e responsáveis.
// A plataforma avisa os responsáveis com a antecedência escolhida (RF16) — ver GerarAlertasCommand.

public record AtividadePlanoDto(int Id, string Titulo, string? Descricao, string? Cor, DateOnly DataInicio, DateOnly? DataFim,
    Periodicidade Recorrencia, int DiasAntecedenciaAviso, IReadOnlyList<UtilizadorResumoDto> Responsaveis, IReadOnlyList<DateOnly> Ocorrencias,
    int DuracaoDias, DateOnly? ProximaOcorrencia);

public record AtividadePlanoDados(string Titulo, string? Descricao, string? Cor, DateOnly DataInicio, DateOnly? DataFim, Periodicidade Recorrencia,
    int DiasAntecedenciaAviso, IReadOnlyList<int>? ResponsaveisIds);

/// <summary>Atividades com ocorrências no ano pedido (para o calendário).</summary>
[Authorize]
public record ListarPlanoAnualQuery(int Ano) : IRequest<IReadOnlyList<AtividadePlanoDto>>;

public class ListarPlanoAnualQueryHandler(IApplicationDbContext context, TimeProvider relogio)
    : IRequestHandler<ListarPlanoAnualQuery, IReadOnlyList<AtividadePlanoDto>>
{
    public async Task<IReadOnlyList<AtividadePlanoDto>> Handle(ListarPlanoAnualQuery request, CancellationToken cancellationToken)
    {
        var inicioAno = new DateOnly(request.Ano, 1, 1);
        var fimAno = new DateOnly(request.Ano, 12, 31);
        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

        var atividades = await context.AtividadesPlanoAnual.AsNoTracking()
            .Include(a => a.Responsaveis)
            .Where(a => a.DataInicio <= fimAno)
            .OrderBy(a => a.DataInicio).ThenBy(a => a.Titulo)
            .ToListAsync(cancellationToken);

        return atividades
            .Select(a => new
            {
                Atividade = a,
                // Inclui as que começaram no ano anterior e ainda decorrem em janeiro.
                Ocorrencias = a.Ocorrencias(inicioAno.AddDays(-a.DuracaoDias), fimAno).ToList(),
            })
            .Where(x => x.Ocorrencias.Count > 0)
            .Select(x => new AtividadePlanoDto(x.Atividade.Id, x.Atividade.Titulo, x.Atividade.Descricao, x.Atividade.Cor, x.Atividade.DataInicio,
                x.Atividade.DataFim, x.Atividade.Recorrencia, x.Atividade.DiasAntecedenciaAviso,
                x.Atividade.Responsaveis.OrderBy(u => u.Nome).Select(u => new UtilizadorResumoDto(u.Id, u.Nome, u.Empresa)).ToList(),
                x.Ocorrencias, x.Atividade.DuracaoDias,
                x.Atividade.Ocorrencias(hoje, hoje.AddYears(2)).Cast<DateOnly?>().FirstOrDefault()))
            .ToList();
    }
}

[Authorize(Permissao = Permissoes.Registos.GerirPlano)]
public record GuardarAtividadePlanoCommand(int? Id, AtividadePlanoDados Dados) : IRequest<int>;

public class GuardarAtividadePlanoCommandValidator : AbstractValidator<GuardarAtividadePlanoCommand>
{
    public GuardarAtividadePlanoCommandValidator()
    {
        RuleFor(c => c.Dados.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Dados.Cor).Matches("^#[0-9a-fA-F]{6}$").When(c => !string.IsNullOrWhiteSpace(c.Dados.Cor))
            .WithMessage("A cor tem de ser do tipo #RRGGBB.");
        RuleFor(c => c.Dados.Recorrencia).IsInEnum();
        RuleFor(c => c.Dados.DiasAntecedenciaAviso).InclusiveBetween(0, 90);
        RuleFor(c => c.Dados.DataFim).GreaterThanOrEqualTo(c => c.Dados.DataInicio).When(c => c.Dados.DataFim is not null)
            .WithMessage("A data de fim não pode ser anterior à de início.");
    }
}

public class GuardarAtividadePlanoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarAtividadePlanoCommand, int>
{
    public async Task<int> Handle(GuardarAtividadePlanoCommand request, CancellationToken cancellationToken)
    {
        AtividadePlanoAnual a;
        if (request.Id is null)
        {
            a = new AtividadePlanoAnual();
            context.AtividadesPlanoAnual.Add(a);
        }
        else
        {
            var existente = await context.AtividadesPlanoAnual.Include(x => x.Responsaveis)
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value, cancellationToken);
            Guard.Against.NotFound(request.Id.Value, existente);
            a = existente;
        }

        var d = request.Dados;
        a.Titulo = d.Titulo.Trim();
        a.Descricao = Registo.Limpar(d.Descricao);
        a.Cor = Registo.Limpar(d.Cor);
        a.DataInicio = d.DataInicio;
        a.DataFim = d.DataFim;
        a.Recorrencia = d.Recorrencia;
        a.DiasAntecedenciaAviso = d.DiasAntecedenciaAviso;

        var ids = (d.ResponsaveisIds ?? []).Distinct().ToList();
        var responsaveis = await context.Utilizadores.Where(u => ids.Contains(u.Id)).ToListAsync(cancellationToken);
        a.Responsaveis.Clear();
        foreach (var u in responsaveis)
        {
            a.Responsaveis.Add(u);
        }

        await context.SaveChangesAsync(cancellationToken);
        return a.Id;
    }
}

[Authorize(Permissao = Permissoes.Registos.GerirPlano)]
public record ApagarAtividadePlanoCommand(int Id) : IRequest;

public class ApagarAtividadePlanoCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarAtividadePlanoCommand>
{
    public async Task Handle(ApagarAtividadePlanoCommand request, CancellationToken cancellationToken)
    {
        var a = await context.AtividadesPlanoAnual.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);
        Guard.Against.NotFound(request.Id, a);
        context.AtividadesPlanoAnual.Remove(a);
        await context.SaveChangesAsync(cancellationToken);
    }
}

// ---------- Avisos automáticos (RF16): plano anual e validades dos ativos ----------

/// <summary>
/// Corre de hora a hora (tarefa em segundo plano). Avisa uma única vez cada pessoa:
/// - das atividades do plano anual, a partir de N dias antes de cada ocorrência (os responsáveis; sem responsáveis, quem gere o plano);
/// - dos ativos intangíveis cuja validade termina nos próximos 90 dias (quem gere os registos).
/// Devolve quantos avisos foram criados. Sem [Authorize]: é chamado pelo sistema, não por um utilizador.
/// </summary>
public record GerarAlertasCommand : IRequest<int>;

public class GerarAlertasCommandHandler(IApplicationDbContext context, INotificacoes notificacoes, IIdentityService identidade, TimeProvider relogio)
    : IRequestHandler<GerarAlertasCommand, int>
{
    public const int DiasAvisoValidade = 90;

    /// <summary>Antecedência dos avisos das vigilâncias e das atividades de gestão/partilha.</summary>
    public const int DiasAvisoAtividades = 7;

    public async Task<int> Handle(GerarAlertasCommand request, CancellationToken cancellationToken)
    {
        var agora = relogio.GetUtcNow();
        var hoje = DateOnly.FromDateTime(agora.UtcDateTime);
        var criados = 0;

        // 1. Plano anual
        var atividades = await context.AtividadesPlanoAnual.Include(a => a.Responsaveis).ToListAsync(cancellationToken);
        List<int>? gestoresPlano = null;
        foreach (var a in atividades)
        {
            foreach (var ocorrencia in a.Ocorrencias(hoje, hoje.AddDays(a.DiasAntecedenciaAviso)))
            {
                var destinatarios = a.Responsaveis.Count > 0
                    ? a.Responsaveis.Select(u => u.Id).ToList()
                    : gestoresPlano ??= await UtilizadoresComPermissaoAsync(Permissoes.Registos.GerirPlano, cancellationToken);
                var dias = ocorrencia.DayNumber - hoje.DayNumber;
                var quando = dias == 0 ? "hoje" : dias == 1 ? "amanhã" : $"daqui a {dias} dias ({ocorrencia:dd/MM/yyyy})";
                criados += await AvisarAsync(TipoAlerta.PlanoAnual, AlvoAlerta.AtividadePlanoAnual, a.Id, ocorrencia, destinatarios,
                    $"Plano anual: {a.Titulo}", $"\"{a.Titulo}\" é {quando}.", "/inovacao/plano-anual", agora, cancellationToken);
            }
        }

        // 2. Validade dos ativos intangíveis
        var limite = hoje.AddDays(DiasAvisoValidade);
        var ativos = await context.AtivosIntangiveis
            .Where(a => a.DataValidade != null && a.DataValidade >= hoje && a.DataValidade <= limite)
            .ToListAsync(cancellationToken);
        if (ativos.Count > 0)
        {
            var gestores = await UtilizadoresComPermissaoAsync(Permissoes.Registos.Gerir, cancellationToken);
            foreach (var a in ativos)
            {
                var validade = a.DataValidade!.Value;
                criados += await AvisarAsync(TipoAlerta.Validade, AlvoAlerta.AtivoIntangivel, a.Id, validade, gestores,
                    $"Validade a terminar: {a.Designacao}",
                    $"A validade de \"{a.Designacao}\" ({a.EntidadeRegisto}) termina a {validade:dd/MM/yyyy}. Veja se é preciso renovar.",
                    "/inovacao/ativos", agora, cancellationToken);
            }
        }

        // 3. Vigilâncias: execuções previstas para os próximos dias (avisa quem gere as iniciativas)
        var ateAtividades = hoje.AddDays(DiasAvisoAtividades);
        var pendentes = await context.Iniciativas.OfType<Vigilancia>()
            .SelectMany(v => v.Execucoes.Where(e => e.DataRealizada == null && e.DataPrevista >= hoje && e.DataPrevista <= ateAtividades)
                .Select(e => new { e.Id, e.DataPrevista, v.Titulo, VigilanciaId = v.Id }))
            .ToListAsync(cancellationToken);
        if (pendentes.Count > 0)
        {
            var gestores = await UtilizadoresComPermissaoAsync(Permissoes.Iniciativas.Gerir, cancellationToken);
            foreach (var e in pendentes)
            {
                criados += await AvisarAsync(TipoAlerta.Prazo, AlvoAlerta.ExecucaoVigilancia, e.Id, e.DataPrevista, gestores,
                    $"Vigilância: {e.Titulo}", $"A vigilância \"{e.Titulo}\" está prevista para {e.DataPrevista:dd/MM/yyyy}.",
                    $"/inovacao/iniciativas/{e.VigilanciaId}", agora, cancellationToken);
            }
        }

        // 4. Atividades de gestão/partilha (Atividades · Quem · Quando) dos ativos e do conhecimento codificado
        var deGestao = new List<(AlvoAlerta Alvo, int Id, string Registo, AtividadeGestao Atividade, string Link)>();
        foreach (var a in await context.AtivosIntangiveis.AsNoTracking().ToListAsync(cancellationToken))
        {
            deGestao.AddRange(a.Atividades.Select(x => (AlvoAlerta.AtivoIntangivel, a.Id, a.Designacao, x, "/inovacao/ativos")));
        }

        foreach (var c in await context.ConhecimentosCodificados.AsNoTracking().ToListAsync(cancellationToken))
        {
            deGestao.AddRange(c.Atividades.Select(x => (AlvoAlerta.ConhecimentoCodificado, c.Id, c.Documento, x, "/inovacao/conhecimento")));
        }

        var proximas = deGestao.Where(x => !x.Atividade.Concluida && x.Atividade.Data is { } d && d >= hoje && d <= ateAtividades).ToList();
        if (proximas.Count > 0)
        {
            var gestores = await UtilizadoresComPermissaoAsync(Permissoes.Registos.Gerir, cancellationToken);
            foreach (var x in proximas)
            {
                var data = x.Atividade.Data!.Value;
                var quem = x.Atividade.Responsavel is null ? string.Empty : $" ({x.Atividade.Responsavel})";
                criados += await AvisarAsync(TipoAlerta.Prazo, x.Alvo, x.Id, data, gestores, $"Atividade: {x.Atividade.Descricao}",
                    $"\"{x.Atividade.Descricao}\"{quem}, de \"{x.Registo}\", está marcada para {data:dd/MM/yyyy}.", x.Link, agora, cancellationToken);
            }
        }

        // 5. Project Charter: tarefas e entregas/milestones a chegar ao prazo ou atrasadas
        criados += await PrazosDoCharterAsync(hoje, agora, cancellationToken);

        return criados;
    }

    /// <summary>Atrasos de há mais tempo que isto já não geram aviso (só aparecem no dashboard), para não encher a caixa de emails.</summary>
    public const int DiasMaximoAtraso = 30;

    private async Task<int> PrazosDoCharterAsync(DateOnly hoje, DateTimeOffset agora, CancellationToken cancellationToken)
    {
        var de = hoje.AddDays(-DiasMaximoAtraso);
        var ate = hoje.AddDays(DiasAvisoAtividades);
        EstadoIniciativa[] fechadas = [EstadoIniciativa.Concluida, EstadoIniciativa.Cancelada, EstadoIniciativa.Rejeitada];
        EstadoTrabalho[] terminadas = [EstadoTrabalho.Concluido, EstadoTrabalho.Cancelado];

        var tarefas = await context.TarefasWbs.AsNoTracking()
            .Where(t => t.FimReal == null && t.FimPlaneado != null && t.FimPlaneado >= de && t.FimPlaneado <= ate)
            .Where(t => !terminadas.Contains(t.Estado) && !fechadas.Contains(t.Charter.Iniciativa.Estado))
            .Select(t => new PrazoCharter(AlvoAlerta.Tarefa, t.Id, string.IsNullOrEmpty(t.CodigoWbs) ? t.Nome : t.CodigoWbs + " " + t.Nome, "A tarefa",
                t.FimPlaneado!.Value, t.ResponsavelId, t.ResponsavelNome, t.Charter.IniciativaId, t.Charter.Iniciativa.Codigo,
                t.Charter.Iniciativa.Titulo, t.Charter.Iniciativa.ResponsavelId))
            .ToListAsync(cancellationToken);
        var entregas = await context.Entregas.AsNoTracking()
            .Where(e => e.DataConclusao == null && e.Prazo != null && e.Prazo >= de && e.Prazo <= ate)
            .Where(e => !terminadas.Contains(e.Estado) && !fechadas.Contains(e.Charter.Iniciativa.Estado))
            .Select(e => new PrazoCharter(AlvoAlerta.Entrega, e.Id, e.Codigo + " " + e.Descricao,
                e.Tipo == TipoEntrega.Milestone ? "O milestone" : "O entregável", e.Prazo!.Value, null, null, e.Charter.IniciativaId,
                e.Charter.Iniciativa.Codigo, e.Charter.Iniciativa.Titulo, e.Charter.Iniciativa.ResponsavelId))
            .ToListAsync(cancellationToken);
        if (tarefas.Count == 0 && entregas.Count == 0)
        {
            return 0;
        }

        var ativos = await context.Utilizadores.Where(u => u.Ativo).Select(u => new { u.Id, u.Nome }).ToListAsync(cancellationToken);
        var porNome = ativos
            .GroupBy(u => Innovera.Application.Iniciativas.ImportarPortefolioCommandHandler.Normalizar(u.Nome))
            .ToDictionary(g => g.Key, g => g.First().Id);
        var idsAtivos = ativos.Select(u => u.Id).ToHashSet();
        List<int>? gestores = null;
        var criados = 0;

        foreach (var p in tarefas.Concat(entregas))
        {
            // Avisa o responsável da tarefa (utilizador ou nome igual ao de um utilizador) e o responsável da iniciativa;
            // se não houver nenhum, quem gere as iniciativas.
            var destinatarios = new HashSet<int>();
            if (p.ResponsavelId is int r && idsAtivos.Contains(r))
            {
                destinatarios.Add(r);
            }
            else if (p.ResponsavelNome is not null
                && porNome.TryGetValue(Innovera.Application.Iniciativas.ImportarPortefolioCommandHandler.Normalizar(p.ResponsavelNome), out var porNomeId))
            {
                destinatarios.Add(porNomeId);
            }

            if (p.ResponsavelIniciativa is int ri && idsAtivos.Contains(ri))
            {
                destinatarios.Add(ri);
            }

            if (destinatarios.Count == 0)
            {
                destinatarios.UnionWith(gestores ??= await UtilizadoresComPermissaoAsync(Permissoes.Iniciativas.Gerir, cancellationToken));
            }

            var atrasada = p.Prazo < hoje;
            var dias = p.Prazo.DayNumber - hoje.DayNumber;
            var quando = atrasada ? $"devia ter terminado a {p.Prazo:dd/MM/yyyy}"
                : dias == 0 ? "termina hoje" : dias == 1 ? "termina amanhã" : $"termina a {p.Prazo:dd/MM/yyyy}";
            var quem = p.ResponsavelNome is null ? string.Empty : $" (responsável: {p.ResponsavelNome})";
            criados += await AvisarAsync(atrasada ? TipoAlerta.Atraso : TipoAlerta.Prazo, p.Alvo, p.Id, p.Prazo, destinatarios,
                atrasada ? $"Atrasado: {p.Nome}" : $"Prazo: {p.Nome}",
                $"{p.Tipo} \"{p.Nome}\"{quem} de {p.IniciativaCodigo} {p.IniciativaTitulo} {quando}.",
                $"/inovacao/iniciativas/{p.IniciativaId}/charter", agora, cancellationToken);
        }

        return criados;
    }

    private sealed record PrazoCharter(AlvoAlerta Alvo, int Id, string Nome, string Tipo, DateOnly Prazo, int? ResponsavelId, string? ResponsavelNome,
        int IniciativaId, string IniciativaCodigo, string IniciativaTitulo, int? ResponsavelIniciativa);

    private async Task<List<int>> UtilizadoresComPermissaoAsync(string permissao, CancellationToken cancellationToken)
    {
        var contas = await identidade.ContasComPermissaoAsync(permissao);
        return await context.Utilizadores.Where(u => u.Ativo && contas.Contains(u.IdentityId)).Select(u => u.Id).ToListAsync(cancellationToken);
    }

    /// <summary>Cria o registo do alerta e o aviso só para quem ainda não foi avisado desta ocorrência.</summary>
    private async Task<int> AvisarAsync(TipoAlerta tipo, AlvoAlerta alvo, int alvoId, DateOnly dataPrevista, IReadOnlyCollection<int> destinatarios,
        string titulo, string mensagem, string link, DateTimeOffset agora, CancellationToken cancellationToken)
    {
        if (destinatarios.Count == 0)
        {
            return 0;
        }

        var jaAvisados = await context.Alertas
            .Where(x => x.Tipo == tipo && x.Alvo == alvo && x.AlvoId == alvoId && x.DataPrevista == dataPrevista)
            .Select(x => x.DestinatarioId)
            .ToListAsync(cancellationToken);
        var novos = destinatarios.Except(jaAvisados).ToList();
        if (novos.Count == 0)
        {
            return 0;
        }

        foreach (var id in novos)
        {
            context.Alertas.Add(new Alerta
            {
                Tipo = tipo, Alvo = alvo, AlvoId = alvoId, DestinatarioId = id, Mensagem = mensagem, DataPrevista = dataPrevista,
                Enviado = true, EnviadoEm = agora,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
        await notificacoes.NotificarAsync(novos, titulo, mensagem, link, enviarEmail: true, cancellationToken);
        return novos.Count;
    }
}
