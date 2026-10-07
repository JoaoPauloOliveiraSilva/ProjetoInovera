using System.Reflection;
using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Entities;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Innovera.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IApplicationDbContext
{
    /// <summary>SEQUENCE que gera o número das ideias (convertido para o código AA01).</summary>
    public const string SequenciaIdeias = "ideia_numero_seq";

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // Transversais
    public DbSet<Utilizador> Utilizadores => Set<Utilizador>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<OutputGerado> OutputsGerados => Set<OutputGerado>();
    public DbSet<RegistoAuditoria> RegistosAuditoria => Set<RegistoAuditoria>();

    // Ideias
    public DbSet<Ideia> Ideias => Set<Ideia>();
    public DbSet<ComentarioIdeia> ComentariosIdeia => Set<ComentarioIdeia>();
    public DbSet<AnexoIdeia> AnexosIdeia => Set<AnexoIdeia>();

    // Iniciativas e Project Charter
    public DbSet<Iniciativa> Iniciativas => Set<Iniciativa>();
    public DbSet<ProjectCharter> ProjectCharters => Set<ProjectCharter>();

    // Registos
    public DbSet<Parceiro> Parceiros => Set<Parceiro>();
    public DbSet<AcordoParceria> AcordosParceria => Set<AcordoParceria>();
    public DbSet<AtivoIntangivel> AtivosIntangiveis => Set<AtivoIntangivel>();
    public DbSet<ConhecimentoCodificado> ConhecimentosCodificados => Set<ConhecimentoCodificado>();
    public DbSet<ConhecimentoTacito> ConhecimentosTacitos => Set<ConhecimentoTacito>();
    public DbSet<FerramentaMetodo> FerramentasMetodos => Set<FerramentaMetodo>();
    public DbSet<LinhaEstrategia> LinhasEstrategia => Set<LinhaEstrategia>();
    public DbSet<AtividadePlanoAnual> AtividadesPlanoAnual => Set<AtividadePlanoAnual>();

    // KPIs
    public DbSet<KpiDefinicao> Kpis => Set<KpiDefinicao>();
    public DbSet<ValorKpi> ValoresKpi => Set<ValorKpi>();
    public DbSet<ValorExterno> ValoresExternos => Set<ValorExterno>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasSequence<int>(SequenciaIdeias).StartsAt(1).IncrementsBy(1);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
