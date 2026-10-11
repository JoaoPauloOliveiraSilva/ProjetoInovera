namespace Innovera.Application.Charters;

public record CharterGeralDto(
    string? SupervisorNome,
    string? Fase,
    string? Classe,
    string? ContextoJustificacao,
    string? BusinessCase,
    string? DecisaoAprovacao,
    string? Objetivos,
    string? ResultadosEsperados);

public record TarefaDto(int Id, string CodigoWbs, string Nome, TipoTarefa Tipo, EstadoTrabalho Estado, DateOnly? InicioPlaneado,
    DateOnly? FimPlaneado, DateOnly? InicioReal, DateOnly? FimReal, string? Responsavel, int? MilestoneId, bool Atrasada);

public record EntregaDto(int Id, TipoEntrega Tipo, string Codigo, string Descricao, DateOnly? Prazo, EstadoTrabalho Estado,
    string? MeioVerificacao, string? Controlo, DateOnly? DataConclusao, string? LinkRelatorio);

public record RiscoDto(int Id, int Numero, DateOnly Data, string Descricao, NivelImpacto Impacto, NivelProbabilidade Probabilidade,
    int Severidade, bool ExigeMitigacao, bool FaltaPlanoMitigacao, RespostaRisco? Resposta, string? PlanoAcao, string? Responsavel,
    DateOnly? Prazo, DateOnly? DataConclusao, string? AvaliacaoEficacia);

public record LinhaOrcamentoDto(int Id, string Categoria, string? Descricao, int Ano, decimal ValorPrevisto, decimal? ValorReal,
    decimal? Variacao, string? Observacoes);

public record TotalOrcamentoDto(int Ano, decimal Previsto, decimal Real);

public record RequisitoDto(int Id, TipoRequisitoConformidade Tipo, string Requisito, string? Descricao, string? Entidade,
    string? Resposta, string? Responsavel, DateOnly? DataPrevista, DateOnly? DataConfirmacao, EstadoRequisito Estado, string? Observacoes);

public record LicaoDto(int Id, CategoriaLicao Categoria, string Licao, string? Descricao, string? Impacto, DateTimeOffset Data);

/// <summary>O Project Charter completo (os 6 separadores) de um projeto ou desafio.</summary>
public record ProjectCharterDto(
    int IniciativaId,
    string Codigo,
    string Titulo,
    TipoIniciativa Tipo,
    EstadoIniciativa Estado,
    string? Responsavel,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    CharterGeralDto Geral,
    IReadOnlyList<TarefaDto> Tarefas,
    IReadOnlyList<EntregaDto> Entregas,
    IReadOnlyList<RiscoDto> Riscos,
    IReadOnlyList<LinhaOrcamentoDto> Orcamento,
    IReadOnlyList<TotalOrcamentoDto> TotaisOrcamento,
    IReadOnlyList<RequisitoDto> Conformidade,
    IReadOnlyList<LicaoDto> Licoes,
    bool PodeEditar,
    bool PodeVerOrcamento,
    bool PodeEditarOrcamento);

// Dados enviados nos formulários de cada separador.

public record TarefaDados(string CodigoWbs, string Nome, TipoTarefa Tipo, EstadoTrabalho Estado, DateOnly? InicioPlaneado,
    DateOnly? FimPlaneado, DateOnly? InicioReal, DateOnly? FimReal, string? Responsavel, int? MilestoneId);

public record EntregaDados(TipoEntrega Tipo, string Codigo, string Descricao, DateOnly? Prazo, EstadoTrabalho Estado,
    string? MeioVerificacao, string? Controlo, DateOnly? DataConclusao, string? LinkRelatorio);

public record RiscoDados(DateOnly? Data, string Descricao, NivelImpacto Impacto, NivelProbabilidade Probabilidade, RespostaRisco? Resposta,
    string? PlanoAcao, string? Responsavel, DateOnly? Prazo, DateOnly? DataConclusao, string? AvaliacaoEficacia);

public record LinhaOrcamentoDados(string Categoria, string? Descricao, int Ano, decimal ValorPrevisto, decimal? ValorReal, string? Observacoes);

public record RequisitoDados(TipoRequisitoConformidade Tipo, string Requisito, string? Descricao, string? Entidade, string? Resposta,
    string? Responsavel, DateOnly? DataPrevista, DateOnly? DataConfirmacao, EstadoRequisito Estado, string? Observacoes);

public record LicaoDados(CategoriaLicao Categoria, string Licao, string? Descricao, string? Impacto);

public enum SeccaoCharter
{
    Tarefa,
    Entrega,
    Risco,
    Orcamento,
    Conformidade,
    Licao
}
