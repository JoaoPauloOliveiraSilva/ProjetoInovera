using System.Reflection;
using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Entities;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Innovera.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<TodoList> TodoLists => Set<TodoList>();

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    public DbSet<AcaoInovacao> AcoesInovacao => Set<AcaoInovacao>();
    public DbSet<Acordo> Acordos => Set<Acordo>();
    public DbSet<AlocacaoEquipaMensal> AlocacoesEquipaMensal => Set<AlocacaoEquipaMensal>();
    public DbSet<AnaliseFerramentaAno> AnalisesFerramentaAno => Set<AnaliseFerramentaAno>();
    public DbSet<Atividade> Atividades => Set<Atividade>();
    public DbSet<AtividadeResponsavel> AtividadesResponsaveis => Set<AtividadeResponsavel>();
    public DbSet<AtivoIntangivel> AtivosIntangiveis => Set<AtivoIntangivel>();
    public DbSet<Conhecimento> Conhecimentos => Set<Conhecimento>();
    public DbSet<Desafio> Desafios => Set<Desafio>();
    public DbSet<Documento> Documentos => Set<Documento>();
    public DbSet<EstrategiaAnual> EstrategiasAnuais => Set<EstrategiaAnual>();
    public DbSet<ExecucaoVigilancia> ExecucoesVigilancia => Set<ExecucaoVigilancia>();
    public DbSet<FerramentaMetodo> FerramentasMetodos => Set<FerramentaMetodo>();
    public DbSet<Ideia> Ideias => Set<Ideia>();
    public DbSet<IdeiaComentario> IdeiasComentarios => Set<IdeiaComentario>();
    public DbSet<IdeiaVoto> IdeiasVotos => Set<IdeiaVoto>();
    public DbSet<Indicador> Indicadores => Set<Indicador>();
    public DbSet<IndicadorValor> IndicadoresValores => Set<IndicadorValor>();
    public DbSet<LicaoAprendida> LicoesAprendidas => Set<LicaoAprendida>();
    public DbSet<MelhoriaContinua> MelhoriasContinuas => Set<MelhoriaContinua>();
    public DbSet<Mestrado> Mestrados => Set<Mestrado>();
    public DbSet<Oportunidade> Oportunidades => Set<Oportunidade>();
    public DbSet<Orcamento> Orcamentos => Set<Orcamento>();
    public DbSet<Parceiro> Parceiros => Set<Parceiro>();
    public DbSet<Projeto> Projetos => Set<Projeto>();
    public DbSet<ProjectCharter> ProjectCharters => Set<ProjectCharter>();
    public DbSet<ProjectCharterEntregavel> ProjectCharterEntregaveis => Set<ProjectCharterEntregavel>();
    public DbSet<ProjectCharterMilestone> ProjectCharterMilestones => Set<ProjectCharterMilestone>();
    public DbSet<ProjectCharterRisco> ProjectCharterRiscos => Set<ProjectCharterRisco>();
    public DbSet<ProjectCharterTarefa> ProjectCharterTarefas => Set<ProjectCharterTarefa>();
    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<Vigilancia> Vigilancias => Set<Vigilancia>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        DomainModelConfiguration.ConfigureAll(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
