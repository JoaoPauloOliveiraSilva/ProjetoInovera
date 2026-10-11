namespace Innovera.Application.Charters;

internal static class CarregarCharter
{
    /// <summary>O Charter da iniciativa (criado na primeira gravação). Só projetos e desafios têm Charter.</summary>
    public static async Task<ProjectCharter> ObterOuCriarAsync(IApplicationDbContext context, int iniciativaId, CancellationToken cancellationToken)
    {
        var iniciativa = await context.Iniciativas.FirstOrDefaultAsync(i => i.Id == iniciativaId, cancellationToken);
        Guard.Against.NotFound(iniciativaId, iniciativa);
        if (iniciativa is not IniciativaComCharter comCharter)
        {
            throw new RegraDeNegocioException("Só os projetos e os desafios têm Project Charter.");
        }

        var charter = await context.ProjectCharters.FirstOrDefaultAsync(c => c.IniciativaId == iniciativaId, cancellationToken);
        if (charter is null)
        {
            charter = new ProjectCharter { Iniciativa = comCharter };
            context.ProjectCharters.Add(charter);
        }

        charter.Iniciativa = comCharter;
        return charter;
    }

    public static string? Limpar(string? texto) => string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();

    public static async Task<T> ItemAsync<T>(IQueryable<T> conjunto, ProjectCharter charter, int id, CancellationToken cancellationToken)
        where T : BaseEntity
    {
        var item = await conjunto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        Guard.Against.NotFound(id, item);
        return item;
    }
}

// ---------- 1. Informação geral ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarCharterGeralCommand(int IniciativaId, CharterGeralDto Dados) : IRequest;

public class GuardarCharterGeralCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarCharterGeralCommand>
{
    public async Task Handle(GuardarCharterGeralCommand request, CancellationToken cancellationToken)
    {
        var c = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var d = request.Dados;
        c.SupervisorNome = CarregarCharter.Limpar(d.SupervisorNome);
        c.Fase = CarregarCharter.Limpar(d.Fase);
        c.Classe = CarregarCharter.Limpar(d.Classe);
        c.ContextoJustificacao = CarregarCharter.Limpar(d.ContextoJustificacao);
        c.BusinessCase = CarregarCharter.Limpar(d.BusinessCase);
        c.DecisaoAprovacao = CarregarCharter.Limpar(d.DecisaoAprovacao);
        c.Objetivos = CarregarCharter.Limpar(d.Objetivos);
        c.ResultadosEsperados = CarregarCharter.Limpar(d.ResultadosEsperados);
        await context.SaveChangesAsync(cancellationToken);
    }
}

public class GuardarCharterGeralCommandValidator : AbstractValidator<GuardarCharterGeralCommand>
{
    public GuardarCharterGeralCommandValidator()
    {
        RuleFor(c => c.Dados.SupervisorNome).MaximumLength(200);
        RuleFor(c => c.Dados.Fase).MaximumLength(150);
        RuleFor(c => c.Dados.Classe).MaximumLength(150);
    }
}

// ---------- 2. Planeamento: tarefas (WBS) ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarTarefaCommand(int IniciativaId, int? Id, TarefaDados Dados) : IRequest<int>;

public class GuardarTarefaCommandValidator : AbstractValidator<GuardarTarefaCommand>
{
    public GuardarTarefaCommandValidator()
    {
        RuleFor(c => c.Dados.Nome).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Dados.CodigoWbs).MaximumLength(20);
        RuleFor(c => c.Dados.Responsavel).MaximumLength(200);
        RuleFor(c => c.Dados.FimPlaneado).GreaterThanOrEqualTo(c => c.Dados.InicioPlaneado)
            .When(c => c.Dados.InicioPlaneado is not null && c.Dados.FimPlaneado is not null)
            .WithMessage("O fim planeado não pode ser antes do início.");
    }
}

public class GuardarTarefaCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarTarefaCommand, int>
{
    public async Task<int> Handle(GuardarTarefaCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var t = request.Id is null
            ? new TarefaWbs { Charter = charter }
            : await CarregarCharter.ItemAsync(context.TarefasWbs.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        if (request.Id is null)
        {
            charter.Tarefas.Add(t);
        }

        var d = request.Dados;
        t.CodigoWbs = d.CodigoWbs?.Trim() ?? string.Empty;
        t.Nome = d.Nome.Trim();
        t.Tipo = d.Tipo;
        t.Estado = d.Estado;
        t.InicioPlaneado = d.InicioPlaneado;
        t.FimPlaneado = d.FimPlaneado;
        t.InicioReal = d.InicioReal;
        t.FimReal = d.FimReal;
        t.ResponsavelNome = CarregarCharter.Limpar(d.Responsavel);
        t.MilestoneId = d.MilestoneId;
        await context.SaveChangesAsync(cancellationToken);
        return t.Id;
    }
}

// ---------- 2. Planeamento: entregáveis e milestones ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarEntregaCommand(int IniciativaId, int? Id, EntregaDados Dados) : IRequest<int>;

public class GuardarEntregaCommandValidator : AbstractValidator<GuardarEntregaCommand>
{
    public GuardarEntregaCommandValidator()
    {
        RuleFor(c => c.Dados.Descricao).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Dados.Codigo).MaximumLength(20);
        RuleFor(c => c.Dados.LinkRelatorio).MaximumLength(2000).Must(ValidacaoLinks.EValido)
            .When(c => !string.IsNullOrWhiteSpace(c.Dados.LinkRelatorio)).WithMessage("O link do relatório tem de ser um endereço https://… (ex.: OneDrive).");
    }
}

public class GuardarEntregaCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<GuardarEntregaCommand, int>
{
    public async Task<int> Handle(GuardarEntregaCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var e = request.Id is null
            ? new Entrega { Charter = charter }
            : await CarregarCharter.ItemAsync(context.Entregas.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        if (request.Id is null)
        {
            charter.Entregas.Add(e);
        }

        e.Charter = charter;
        var d = request.Dados;
        e.Tipo = d.Tipo;
        e.Codigo = d.Codigo?.Trim() ?? string.Empty;
        e.Descricao = d.Descricao.Trim();
        e.Prazo = d.Prazo;
        e.Estado = d.Estado;
        e.MeioVerificacao = CarregarCharter.Limpar(d.MeioVerificacao);
        e.Controlo = CarregarCharter.Limpar(d.Controlo);

        // Relatório novo → fica concluído e entra no conhecimento codificado (Mod.237), pelo evento de domínio.
        var link = CarregarCharter.Limpar(d.LinkRelatorio);
        if (link is not null && link != e.LinkRelatorio)
        {
            e.RegistarRelatorio(link, d.DataConclusao ?? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime));
        }
        else
        {
            e.LinkRelatorio = link;
            e.DataConclusao = d.DataConclusao;
        }

        await context.SaveChangesAsync(cancellationToken);
        return e.Id;
    }
}

// ---------- 3. Riscos ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarRiscoCommand(int IniciativaId, int? Id, RiscoDados Dados) : IRequest<int>;

public class GuardarRiscoCommandValidator : AbstractValidator<GuardarRiscoCommand>
{
    public GuardarRiscoCommandValidator()
    {
        RuleFor(c => c.Dados.Descricao).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Dados.Impacto).IsInEnum();
        RuleFor(c => c.Dados.Probabilidade).IsInEnum();
        RuleFor(c => c.Dados.Responsavel).MaximumLength(200);
    }
}

public class GuardarRiscoCommandHandler(IApplicationDbContext context, TimeProvider relogio) : IRequestHandler<GuardarRiscoCommand, int>
{
    public async Task<int> Handle(GuardarRiscoCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        Risco r;
        if (request.Id is null)
        {
            var maior = charter.Id == 0 ? 0 : await context.Riscos.Where(x => x.CharterId == charter.Id).MaxAsync(x => (int?)x.Numero, cancellationToken) ?? 0;
            r = new Risco { Charter = charter, Numero = maior + 1 };
            charter.Riscos.Add(r);
        }
        else
        {
            r = await CarregarCharter.ItemAsync(context.Riscos.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        }

        var d = request.Dados;
        r.Data = d.Data ?? (r.Data == default ? DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime) : r.Data);
        r.Descricao = d.Descricao.Trim();
        r.Impacto = d.Impacto;
        r.Probabilidade = d.Probabilidade;
        r.Resposta = d.Resposta;
        r.PlanoAcao = CarregarCharter.Limpar(d.PlanoAcao);
        r.ResponsavelNome = CarregarCharter.Limpar(d.Responsavel);
        r.Prazo = d.Prazo;
        r.DataConclusao = d.DataConclusao;
        r.AvaliacaoEficacia = CarregarCharter.Limpar(d.AvaliacaoEficacia);
        await context.SaveChangesAsync(cancellationToken);
        return r.Id;
    }
}

// ---------- 4. Orçamento ----------

[Authorize(Permissao = Permissoes.Orcamento.Gerir)]
public record GuardarLinhaOrcamentoCommand(int IniciativaId, int? Id, LinhaOrcamentoDados Dados) : IRequest<int>;

public class GuardarLinhaOrcamentoCommandValidator : AbstractValidator<GuardarLinhaOrcamentoCommand>
{
    public GuardarLinhaOrcamentoCommandValidator()
    {
        RuleFor(c => c.Dados.Categoria).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Dados.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Dados.ValorPrevisto).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Dados.ValorReal).GreaterThanOrEqualTo(0).When(c => c.Dados.ValorReal is not null);
    }
}

public class GuardarLinhaOrcamentoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarLinhaOrcamentoCommand, int>
{
    public async Task<int> Handle(GuardarLinhaOrcamentoCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var l = request.Id is null
            ? new LinhaOrcamento { Charter = charter }
            : await CarregarCharter.ItemAsync(context.LinhasOrcamento.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        if (request.Id is null)
        {
            charter.LinhasOrcamento.Add(l);
        }

        var d = request.Dados;
        l.Categoria = d.Categoria.Trim();
        l.Descricao = CarregarCharter.Limpar(d.Descricao);
        l.Ano = d.Ano;
        l.ValorPrevisto = d.ValorPrevisto;
        l.ValorReal = d.ValorReal;
        l.Observacoes = CarregarCharter.Limpar(d.Observacoes);
        await context.SaveChangesAsync(cancellationToken);
        return l.Id;
    }
}

// ---------- 5. Requisitos de conformidade ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarRequisitoCommand(int IniciativaId, int? Id, RequisitoDados Dados) : IRequest<int>;

public class GuardarRequisitoCommandValidator : AbstractValidator<GuardarRequisitoCommand>
{
    public GuardarRequisitoCommandValidator()
    {
        RuleFor(c => c.Dados.Requisito).NotEmpty().MaximumLength(300);
        RuleFor(c => c.Dados.Entidade).MaximumLength(200);
        RuleFor(c => c.Dados.Responsavel).MaximumLength(200);
    }
}

public class GuardarRequisitoCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarRequisitoCommand, int>
{
    public async Task<int> Handle(GuardarRequisitoCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var r = request.Id is null
            ? new RequisitoConformidade { Charter = charter }
            : await CarregarCharter.ItemAsync(context.RequisitosConformidade.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        if (request.Id is null)
        {
            charter.RequisitosConformidade.Add(r);
        }

        var d = request.Dados;
        r.Tipo = d.Tipo;
        r.Requisito = d.Requisito.Trim();
        r.Descricao = CarregarCharter.Limpar(d.Descricao);
        r.Entidade = CarregarCharter.Limpar(d.Entidade);
        r.Resposta = CarregarCharter.Limpar(d.Resposta);
        r.ResponsavelNome = CarregarCharter.Limpar(d.Responsavel);
        r.DataPrevista = d.DataPrevista;
        r.DataConfirmacao = d.DataConfirmacao;
        r.Estado = d.Estado;
        r.Observacoes = CarregarCharter.Limpar(d.Observacoes);
        await context.SaveChangesAsync(cancellationToken);
        return r.Id;
    }
}

// ---------- 6. Lições aprendidas (→ repositório Mod.252) ----------

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarLicaoCommand(int IniciativaId, int? Id, LicaoDados Dados) : IRequest<int>;

public class GuardarLicaoCommandValidator : AbstractValidator<GuardarLicaoCommand>
{
    public GuardarLicaoCommandValidator()
    {
        RuleFor(c => c.Dados.Licao).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.Dados.Categoria).IsInEnum();
    }
}

public class GuardarLicaoCommandHandler(IApplicationDbContext context, INotificacoes notificacoes) : IRequestHandler<GuardarLicaoCommand, int>
{
    public async Task<int> Handle(GuardarLicaoCommand request, CancellationToken cancellationToken)
    {
        var charter = await CarregarCharter.ObterOuCriarAsync(context, request.IniciativaId, cancellationToken);
        var d = request.Dados;
        LicaoAprendida l;
        if (request.Id is null)
        {
            l = new LicaoAprendida { Charter = charter, Categoria = d.Categoria, Licao = d.Licao.Trim() };
            charter.RegistarLicao(l);
        }
        else
        {
            l = await CarregarCharter.ItemAsync(context.LicoesAprendidas.Where(x => x.CharterId == charter.Id), charter, request.Id.Value, cancellationToken);
        }

        l.Categoria = d.Categoria;
        l.Licao = d.Licao.Trim();
        l.Descricao = CarregarCharter.Limpar(d.Descricao);
        l.Impacto = CarregarCharter.Limpar(d.Impacto);
        await context.SaveChangesAsync(cancellationToken);

        if (request.Id is null)
        {
            await notificacoes.NotificarComPermissaoAsync(Permissoes.Iniciativas.Gerir,
                $"Nova lição aprendida em {charter.Iniciativa.Codigo}",
                $"\"{l.Licao}\" foi acrescentada ao repositório de lições aprendidas (Mod.252).",
                "/inovacao/licoes", cancellationToken);
        }

        return l.Id;
    }
}

// ---------- Apagar uma linha de qualquer separador ----------

/// <summary>A permissão depende do separador: orçamento → orcamento.gerir; os outros → iniciativas.gerir.</summary>
[Authorize]
public record ApagarItemCharterCommand(int IniciativaId, SeccaoCharter Seccao, int ItemId) : IRequest;

public class ApagarItemCharterCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores) : IRequestHandler<ApagarItemCharterCommand>
{
    public async Task Handle(ApagarItemCharterCommand request, CancellationToken cancellationToken)
    {
        var permissao = request.Seccao == SeccaoCharter.Orcamento ? Permissoes.Orcamento.Gerir : Permissoes.Iniciativas.Gerir;
        if (!await utilizadores.TemPermissaoAsync(permissao, cancellationToken))
        {
            throw new Innovera.Application.Common.Exceptions.ForbiddenAccessException();
        }

        var charterId = await context.ProjectCharters
            .Where(c => c.IniciativaId == request.IniciativaId)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(request.IniciativaId.ToString(System.Globalization.CultureInfo.InvariantCulture), nameof(ProjectCharter));

        // Entregas: as tarefas e o conhecimento codificado que apontam para ela ficam sem ligação (SetNull na BD).
        switch (request.Seccao)
        {
            case SeccaoCharter.Tarefa:
                context.TarefasWbs.Remove(await Obter(context.TarefasWbs, charterId, request.ItemId, cancellationToken));
                break;
            case SeccaoCharter.Entrega:
                context.Entregas.Remove(await Obter(context.Entregas, charterId, request.ItemId, cancellationToken));
                break;
            case SeccaoCharter.Risco:
                context.Riscos.Remove(await Obter(context.Riscos, charterId, request.ItemId, cancellationToken));
                break;
            case SeccaoCharter.Orcamento:
                context.LinhasOrcamento.Remove(await Obter(context.LinhasOrcamento, charterId, request.ItemId, cancellationToken));
                break;
            case SeccaoCharter.Conformidade:
                context.RequisitosConformidade.Remove(await Obter(context.RequisitosConformidade, charterId, request.ItemId, cancellationToken));
                break;
            case SeccaoCharter.Licao:
                context.LicoesAprendidas.Remove(await Obter(context.LicoesAprendidas, charterId, request.ItemId, cancellationToken));
                break;
            default:
                throw new RegraDeNegocioException("Separador desconhecido.");
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private static async Task<T> Obter<T>(DbSet<T> conjunto, int charterId, int id, CancellationToken cancellationToken)
        where T : BaseEntity, IItemCharter
    {
        var item = await conjunto.FirstOrDefaultAsync(x => x.Id == id && x.CharterId == charterId, cancellationToken);
        Guard.Against.NotFound(id, item);
        return item;
    }
}
