using Innovera.Domain.Entities;

namespace Innovera.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TodoList> TodoLists { get; }

    DbSet<TodoItem> TodoItems { get; }

    DbSet<AcaoInovacao> AcoesInovacao { get; }
    DbSet<Acordo> Acordos { get; }
    DbSet<AlocacaoEquipaMensal> AlocacoesEquipaMensal { get; }
    DbSet<AnaliseFerramentaAno> AnalisesFerramentaAno { get; }
    DbSet<Atividade> Atividades { get; }
    DbSet<AtividadeResponsavel> AtividadesResponsaveis { get; }
    DbSet<AtivoIntangivel> AtivosIntangiveis { get; }
    DbSet<Conhecimento> Conhecimentos { get; }
    DbSet<Desafio> Desafios { get; }
    DbSet<Documento> Documentos { get; }
    DbSet<EstrategiaAnual> EstrategiasAnuais { get; }
    DbSet<ExecucaoVigilancia> ExecucoesVigilancia { get; }
    DbSet<FerramentaMetodo> FerramentasMetodos { get; }
    DbSet<Ideia> Ideias { get; }
    DbSet<IdeiaComentario> IdeiasComentarios { get; }
    DbSet<IdeiaVoto> IdeiasVotos { get; }
    DbSet<Indicador> Indicadores { get; }
    DbSet<IndicadorValor> IndicadoresValores { get; }
    DbSet<LicaoAprendida> LicoesAprendidas { get; }
    DbSet<MelhoriaContinua> MelhoriasContinuas { get; }
    DbSet<Mestrado> Mestrados { get; }
    DbSet<Oportunidade> Oportunidades { get; }
    DbSet<Orcamento> Orcamentos { get; }
    DbSet<Parceiro> Parceiros { get; }
    DbSet<Projeto> Projetos { get; }
    DbSet<ProjectCharter> ProjectCharters { get; }
    DbSet<ProjectCharterEntregavel> ProjectCharterEntregaveis { get; }
    DbSet<ProjectCharterMilestone> ProjectCharterMilestones { get; }
    DbSet<ProjectCharterRisco> ProjectCharterRiscos { get; }
    DbSet<ProjectCharterTarefa> ProjectCharterTarefas { get; }
    DbSet<Utilizador> Utilizadores { get; }
    DbSet<Vigilancia> Vigilancias { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
