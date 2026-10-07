using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

public class ParceiroConfiguration : IEntityTypeConfiguration<Parceiro>
{
    public void Configure(EntityTypeBuilder<Parceiro> builder)
    {
        builder.Property(p => p.Nome).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Nif).HasMaxLength(20);
        builder.HasMany(p => p.Acordos).WithOne(a => a.Parceiro).HasForeignKey(a => a.ParceiroId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AcordoParceriaConfiguration : IEntityTypeConfiguration<AcordoParceria>
{
    public void Configure(EntityTypeBuilder<AcordoParceria> builder)
    {
        builder.Property(a => a.LinkDocumento).HasMaxLength(2000);
    }
}

public class AtivoIntangivelConfiguration : IEntityTypeConfiguration<AtivoIntangivel>
{
    public void Configure(EntityTypeBuilder<AtivoIntangivel> builder)
    {
        builder.Property(a => a.Designacao).HasMaxLength(300).IsRequired();
        builder.Property(a => a.EntidadeRegisto).HasMaxLength(100);
        builder.Property(a => a.Custo).HasPrecision(18, 2);
        builder.OwnsMany(a => a.Atividades, b => b.ToTable("AtivoIntangivelAtividades"));
    }
}

public class ConhecimentoCodificadoConfiguration : IEntityTypeConfiguration<ConhecimentoCodificado>
{
    public void Configure(EntityTypeBuilder<ConhecimentoCodificado> builder)
    {
        builder.Property(c => c.Documento).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Url).HasMaxLength(2000);
        builder.HasOne(c => c.Iniciativa).WithMany().HasForeignKey(c => c.IniciativaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.Entrega).WithMany().HasForeignKey(c => c.EntregaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(c => c.ExecucaoVigilancia).WithMany().HasForeignKey(c => c.ExecucaoVigilanciaId).OnDelete(DeleteBehavior.SetNull);
        builder.OwnsMany(c => c.Atividades, b => b.ToTable("ConhecimentoCodificadoAtividades"));
    }
}

public class ConhecimentoTacitoConfiguration : IEntityTypeConfiguration<ConhecimentoTacito>
{
    public void Configure(EntityTypeBuilder<ConhecimentoTacito> builder)
    {
        builder.Property(c => c.Detentor).HasMaxLength(200);
        builder.OwnsMany(c => c.Atividades, b => b.ToTable("ConhecimentoTacitoAtividades"));
    }
}

public class FerramentaMetodoConfiguration : IEntityTypeConfiguration<FerramentaMetodo>
{
    public void Configure(EntityTypeBuilder<FerramentaMetodo> builder)
    {
        builder.Property(f => f.Nome).HasMaxLength(200).IsRequired();
        builder.Property(f => f.CustoAquisicao).HasPrecision(18, 2);
        builder.OwnsMany(f => f.Analises, b => b.ToTable("FerramentaMetodoAnalises"));
    }
}

public class LinhaEstrategiaConfiguration : IEntityTypeConfiguration<LinhaEstrategia>
{
    public void Configure(EntityTypeBuilder<LinhaEstrategia> builder)
    {
        builder.Property(l => l.AreaAtuacao).HasMaxLength(200);
        builder.Property(l => l.HorasMesPorRecurso).HasPrecision(8, 2);
        builder.HasOne(l => l.Iniciativa).WithMany().HasForeignKey(l => l.IniciativaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(l => l.Kpis).WithMany().UsingEntity(j => j.ToTable("LinhaEstrategiaKpis"));
    }
}

public class AtividadePlanoAnualConfiguration : IEntityTypeConfiguration<AtividadePlanoAnual>
{
    public void Configure(EntityTypeBuilder<AtividadePlanoAnual> builder)
    {
        builder.Property(a => a.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Cor).HasMaxLength(9);
        builder.HasMany(a => a.Responsaveis).WithMany().UsingEntity(j => j.ToTable("AtividadePlanoAnualResponsaveis"));
    }
}

public class KpiDefinicaoConfiguration : IEntityTypeConfiguration<KpiDefinicao>
{
    public void Configure(EntityTypeBuilder<KpiDefinicao> builder)
    {
        builder.HasIndex(k => k.Numero).IsUnique();
        builder.Property(k => k.Nome).HasMaxLength(300).IsRequired();
        builder.Property(k => k.ObjetivoAnual).HasMaxLength(50);
        builder.HasMany(k => k.Valores).WithOne(v => v.Kpi).HasForeignKey(v => v.KpiId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ValorKpiConfiguration : IEntityTypeConfiguration<ValorKpi>
{
    public void Configure(EntityTypeBuilder<ValorKpi> builder)
    {
        builder.HasIndex(v => new { v.KpiId, v.Ano, v.Periodo }).IsUnique();
        builder.Property(v => v.ValorAbsoluto).HasPrecision(18, 4);
        builder.Property(v => v.ValorRelativo).HasPrecision(18, 4);
        builder.Property(v => v.Objetivo).HasMaxLength(50);
        builder.HasMany(v => v.Acoes).WithOne(a => a.ValorKpi).HasForeignKey(a => a.ValorKpiId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AcaoKpiConfiguration : IEntityTypeConfiguration<AcaoKpi>
{
    public void Configure(EntityTypeBuilder<AcaoKpi> builder)
    {
        builder.HasOne(a => a.Responsavel).WithMany().HasForeignKey(a => a.ResponsavelId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ValorExternoConfiguration : IEntityTypeConfiguration<ValorExterno>
{
    public void Configure(EntityTypeBuilder<ValorExterno> builder)
    {
        builder.HasIndex(v => new { v.Tipo, v.Ano, v.Periodo }).IsUnique();
        builder.Property(v => v.Valor).HasPrecision(18, 2);
    }
}
