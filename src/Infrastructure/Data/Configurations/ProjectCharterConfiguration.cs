using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

public class ProjectCharterConfiguration : IEntityTypeConfiguration<ProjectCharter>
{
    public void Configure(EntityTypeBuilder<ProjectCharter> builder)
    {
        builder.HasIndex(c => c.IniciativaId).IsUnique();
        builder.Property(c => c.Fase).HasMaxLength(150);
        builder.Property(c => c.Classe).HasMaxLength(150);
        builder.HasOne(c => c.Supervisor).WithMany().HasForeignKey(c => c.SupervisorId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Tarefas).WithOne(t => t.Charter).HasForeignKey(t => t.CharterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Entregas).WithOne(e => e.Charter).HasForeignKey(e => e.CharterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.Riscos).WithOne(r => r.Charter).HasForeignKey(r => r.CharterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.LinhasOrcamento).WithOne(l => l.Charter).HasForeignKey(l => l.CharterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.RequisitosConformidade).WithOne(r => r.Charter).HasForeignKey(r => r.CharterId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(c => c.LicoesAprendidas).WithOne(l => l.Charter).HasForeignKey(l => l.CharterId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class TarefaWbsConfiguration : IEntityTypeConfiguration<TarefaWbs>
{
    public void Configure(EntityTypeBuilder<TarefaWbs> builder)
    {
        builder.Property(t => t.CodigoWbs).HasMaxLength(20);
        builder.Property(t => t.Nome).HasMaxLength(300).IsRequired();
        builder.HasOne(t => t.DependeDe).WithMany().HasForeignKey(t => t.DependeDeId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.Milestone).WithMany().HasForeignKey(t => t.MilestoneId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(t => t.Responsavel).WithMany().HasForeignKey(t => t.ResponsavelId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class EntregaConfiguration : IEntityTypeConfiguration<Entrega>
{
    public void Configure(EntityTypeBuilder<Entrega> builder)
    {
        builder.Property(e => e.Codigo).HasMaxLength(20);
        builder.Property(e => e.LinkRelatorio).HasMaxLength(2000);
    }
}

public class RiscoConfiguration : IEntityTypeConfiguration<Risco>
{
    public void Configure(EntityTypeBuilder<Risco> builder)
    {
        builder.HasOne(r => r.Responsavel).WithMany().HasForeignKey(r => r.ResponsavelId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class LinhaOrcamentoConfiguration : IEntityTypeConfiguration<LinhaOrcamento>
{
    public void Configure(EntityTypeBuilder<LinhaOrcamento> builder)
    {
        builder.Property(l => l.Categoria).HasMaxLength(100);
        builder.Property(l => l.ValorPrevisto).HasPrecision(18, 2);
        builder.Property(l => l.ValorReal).HasPrecision(18, 2);
    }
}

public class RequisitoConformidadeConfiguration : IEntityTypeConfiguration<RequisitoConformidade>
{
    public void Configure(EntityTypeBuilder<RequisitoConformidade> builder)
    {
        builder.Property(r => r.Requisito).HasMaxLength(300);
        builder.Property(r => r.Entidade).HasMaxLength(200);
        builder.HasOne(r => r.Responsavel).WithMany().HasForeignKey(r => r.ResponsavelId).OnDelete(DeleteBehavior.SetNull);
    }
}
