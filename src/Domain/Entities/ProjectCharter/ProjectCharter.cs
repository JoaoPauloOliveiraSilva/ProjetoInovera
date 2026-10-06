namespace Innovera.Domain.Entities;

/// <summary>
/// Project Charter digital de um projeto ou desafio (6 separadores: informação geral, planeamento,
/// riscos, orçamento, conformidade e lições aprendidas). A equipa e a alocação mensal ficam na
/// iniciativa (<see cref="Iniciativa.MembrosEquipa"/>) e o histórico do ponto de situação também.
/// </summary>
public class ProjectCharter : BaseAuditableEntity
{
    public int IniciativaId { get; set; }

    public IniciativaComCharter Iniciativa { get; set; } = null!;

    // 1. Informação geral (nome, gestor, datas, estado e pilar estão na iniciativa)
    public int? SupervisorId { get; set; }

    public Utilizador? Supervisor { get; set; }

    /// <summary>Fase atual (ex.: "Fase de desenvolvimento").</summary>
    public string? Fase { get; set; }

    /// <summary>Classe do Charter (ex.: "Inovação interna / Projeto").</summary>
    public string? Classe { get; set; }

    public string? ContextoJustificacao { get; set; }

    public string? BusinessCase { get; set; }

    /// <summary>Ex.: "Aprovado pela CE a 4 de outubro de 2025".</summary>
    public string? DecisaoAprovacao { get; set; }

    /// <summary>Objetivos (um por linha).</summary>
    public string? Objetivos { get; set; }

    /// <summary>Resultados esperados (um por linha).</summary>
    public string? ResultadosEsperados { get; set; }

    // 2. Planeamento
    public ICollection<TarefaWbs> Tarefas { get; set; } = new List<TarefaWbs>();

    public ICollection<Entrega> Entregas { get; set; } = new List<Entrega>();

    // 3. Riscos
    public ICollection<Risco> Riscos { get; set; } = new List<Risco>();

    // 4. Orçamento
    public ICollection<LinhaOrcamento> LinhasOrcamento { get; set; } = new List<LinhaOrcamento>();

    // 5. Conformidade (legal + qualidade/ambiente/segurança)
    public ICollection<RequisitoConformidade> RequisitosConformidade { get; set; } = new List<RequisitoConformidade>();

    // 6. Lições aprendidas
    public ICollection<LicaoAprendida> LicoesAprendidas { get; set; } = new List<LicaoAprendida>();

    /// <summary>Regista uma lição; o evento faz com que entre no repositório global (Mod.252).</summary>
    public void RegistarLicao(LicaoAprendida licao)
    {
        ArgumentNullException.ThrowIfNull(licao);

        if (string.IsNullOrWhiteSpace(licao.Licao))
        {
            throw new RegraDeNegocioException("A lição não pode estar vazia.");
        }

        LicoesAprendidas.Add(licao);
        AddDomainEvent(new LicaoRegistadaEvent(licao));
    }

    public decimal OrcamentoPrevisto(int ano)
        => LinhasOrcamento.Where(l => l.Ano == ano).Sum(l => l.ValorPrevisto);

    public decimal OrcamentoReal(int ano)
        => LinhasOrcamento.Where(l => l.Ano == ano).Sum(l => l.ValorReal ?? 0m);
}
