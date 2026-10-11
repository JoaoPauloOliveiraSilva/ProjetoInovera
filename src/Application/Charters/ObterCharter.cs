namespace Innovera.Application.Charters;

/// <summary>Project Charter de um projeto/desafio (vazio se ainda não foi preenchido).</summary>
[Authorize]
public record ObterCharterQuery(int IniciativaId) : IRequest<ProjectCharterDto>;

public class ObterCharterQueryHandler(IApplicationDbContext context, IUtilizadores utilizadores, TimeProvider relogio)
    : IRequestHandler<ObterCharterQuery, ProjectCharterDto>
{
    public async Task<ProjectCharterDto> Handle(ObterCharterQuery request, CancellationToken cancellationToken)
    {
        var iniciativa = await context.Iniciativas.AsNoTracking()
            .Include(i => i.Responsavel)
            .FirstOrDefaultAsync(i => i.Id == request.IniciativaId, cancellationToken);
        Guard.Against.NotFound(request.IniciativaId, iniciativa);
        if (iniciativa is not IniciativaComCharter)
        {
            throw new RegraDeNegocioException("Só os projetos e os desafios têm Project Charter.");
        }

        var c = await context.ProjectCharters.AsNoTracking()
            .Include(x => x.Tarefas)
            .Include(x => x.Entregas)
            .Include(x => x.Riscos)
            .Include(x => x.LinhasOrcamento)
            .Include(x => x.RequisitosConformidade)
            .Include(x => x.LicoesAprendidas)
            .FirstOrDefaultAsync(x => x.IniciativaId == request.IniciativaId, cancellationToken);

        var hoje = DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);
        var geral = c is null
            ? new CharterGeralDto(null, null, null, null, null, null, null, null)
            : new CharterGeralDto(c.SupervisorNome, c.Fase, c.Classe, c.ContextoJustificacao, c.BusinessCase, c.DecisaoAprovacao, c.Objetivos,
                c.ResultadosEsperados);

        // O orçamento só aparece a quem tem permissão (por omissão, só o Administrador).
        var podeEditarOrcamento = await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Gerir, cancellationToken);
        var podeVerOrcamento = podeEditarOrcamento || await utilizadores.TemPermissaoAsync(Permissoes.Orcamento.Ver, cancellationToken);
        var orcamento = podeVerOrcamento
            ? c?.LinhasOrcamento.OrderBy(l => l.Ano).ThenBy(l => l.Categoria).ToList() ?? []
            : [];

        return new ProjectCharterDto(
            iniciativa.Id, iniciativa.Codigo, iniciativa.Titulo, iniciativa.Tipo, iniciativa.Estado,
            iniciativa.Responsavel?.Nome ?? iniciativa.ResponsavelNome, iniciativa.DataInicio, iniciativa.DataFim,
            geral,
            c?.Tarefas.OrderBy(t => t.CodigoWbs, StringComparer.Ordinal).ThenBy(t => t.Id)
                .Select(t => new TarefaDto(t.Id, t.CodigoWbs, t.Nome, t.Tipo, t.Estado, t.InicioPlaneado, t.FimPlaneado, t.InicioReal, t.FimReal,
                    t.ResponsavelNome, t.MilestoneId, t.EstaAtrasada(hoje)))
                .ToList() ?? [],
            c?.Entregas.OrderBy(e => e.Prazo).ThenBy(e => e.Id)
                .Select(e => new EntregaDto(e.Id, e.Tipo, e.Codigo, e.Descricao, e.Prazo, e.Estado, e.MeioVerificacao, e.Controlo,
                    e.DataConclusao, e.LinkRelatorio))
                .ToList() ?? [],
            c?.Riscos.OrderBy(r => r.Numero)
                .Select(r => new RiscoDto(r.Id, r.Numero, r.Data, r.Descricao, r.Impacto, r.Probabilidade, r.Severidade, r.ExigeMitigacao,
                    r.FaltaPlanoMitigacao, r.Resposta, r.PlanoAcao, r.ResponsavelNome, r.Prazo, r.DataConclusao, r.AvaliacaoEficacia))
                .ToList() ?? [],
            orcamento.Select(l => new LinhaOrcamentoDto(l.Id, l.Categoria, l.Descricao, l.Ano, l.ValorPrevisto, l.ValorReal, l.Variacao,
                l.Observacoes)).ToList(),
            orcamento.GroupBy(l => l.Ano)
                .Select(g => new TotalOrcamentoDto(g.Key, g.Sum(l => l.ValorPrevisto), g.Sum(l => l.ValorReal ?? 0m)))
                .ToList(),
            c?.RequisitosConformidade.OrderBy(r => r.Tipo).ThenBy(r => r.Id)
                .Select(r => new RequisitoDto(r.Id, r.Tipo, r.Requisito, r.Descricao, r.Entidade, r.Resposta, r.ResponsavelNome, r.DataPrevista,
                    r.DataConfirmacao, r.Estado, r.Observacoes))
                .ToList() ?? [],
            c?.LicoesAprendidas.OrderByDescending(l => l.Created)
                .Select(l => new LicaoDto(l.Id, l.Categoria, l.Licao, l.Descricao, l.Impacto, l.Created))
                .ToList() ?? [],
            await utilizadores.TemPermissaoAsync(Permissoes.Iniciativas.Gerir, cancellationToken),
            podeVerOrcamento,
            podeEditarOrcamento);
    }
}
