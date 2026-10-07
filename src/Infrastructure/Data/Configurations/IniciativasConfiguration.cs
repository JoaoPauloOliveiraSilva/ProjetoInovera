using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

/// <summary>
/// As 7 especializações ficam numa só tabela (TPH), com uma coluna "Discriminador" com o tipo.
/// </summary>
public class IniciativaConfiguration : IEntityTypeConfiguration<Iniciativa>
{
    public void Configure(EntityTypeBuilder<Iniciativa> builder)
    {
        builder.ToTable("Iniciativas");
        builder.Ignore(i => i.Tipo);

        builder.HasDiscriminator<string>("Discriminador")
            .HasValue<Projeto>(nameof(Projeto))
            .HasValue<Desafio>(nameof(Desafio))
            .HasValue<MelhoriaContinua>(nameof(MelhoriaContinua))
            .HasValue<Oportunidade>(nameof(Oportunidade))
            .HasValue<Vigilancia>(nameof(Vigilancia))
            .HasValue<Mestrado>(nameof(Mestrado))
            .HasValue<AcaoInovacao>(nameof(AcaoInovacao));
        builder.Property<string>("Discriminador").HasMaxLength(30);

        builder.Property(i => i.Codigo).HasMaxLength(20);
        builder.Property(i => i.Titulo).HasMaxLength(300).IsRequired();
        builder.Property(i => i.Origem).HasMaxLength(200);
        builder.HasIndex(i => i.Codigo);

        builder.HasOne(i => i.Responsavel).WithMany().HasForeignKey(i => i.ResponsavelId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.IdeiaOrigem).WithOne(i => i.IniciativaResultante)
            .HasForeignKey<Iniciativa>(i => i.IdeiaOrigemId).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Links).WithOne(l => l.Iniciativa).HasForeignKey(l => l.IniciativaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.NotasPontoSituacao).WithOne(n => n.Iniciativa).HasForeignKey(n => n.IniciativaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.MembrosEquipa).WithOne(m => m.Iniciativa).HasForeignKey(m => m.IniciativaId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Parceiros).WithMany(p => p.Iniciativas)
            .UsingEntity(j => j.ToTable("IniciativaParceiros"));
    }
}

public class IniciativaComCharterConfiguration : IEntityTypeConfiguration<IniciativaComCharter>
{
    public void Configure(EntityTypeBuilder<IniciativaComCharter> builder)
    {
        builder.HasOne(i => i.Charter).WithOne(c => c.Iniciativa)
            .HasForeignKey<ProjectCharter>(c => c.IniciativaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class MestradoConfiguration : IEntityTypeConfiguration<Mestrado>
{
    public void Configure(EntityTypeBuilder<Mestrado> builder)
    {
        builder.Property(m => m.NomeAluno).HasMaxLength(200);
        builder.Property(m => m.Curso).HasMaxLength(200);
        builder.HasOne(m => m.Orientador).WithMany().HasForeignKey(m => m.OrientadorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class VigilanciaConfiguration : IEntityTypeConfiguration<Vigilancia>
{
    public void Configure(EntityTypeBuilder<Vigilancia> builder)
    {
        builder.HasMany(v => v.Execucoes).WithOne(e => e.Vigilancia).HasForeignKey(e => e.VigilanciaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ExecucaoVigilanciaConfiguration : IEntityTypeConfiguration<ExecucaoVigilancia>
{
    public void Configure(EntityTypeBuilder<ExecucaoVigilancia> builder)
    {
        builder.Property(e => e.LinkResultado).HasMaxLength(2000);
    }
}

public class MembroEquipaConfiguration : IEntityTypeConfiguration<MembroEquipa>
{
    public void Configure(EntityTypeBuilder<MembroEquipa> builder)
    {
        builder.Property(m => m.Funcao).HasMaxLength(150);
        builder.HasIndex(m => new { m.IniciativaId, m.UtilizadorId }).IsUnique();
        builder.HasOne(m => m.Utilizador).WithMany().HasForeignKey(m => m.UtilizadorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(m => m.Alocacoes).WithOne(a => a.MembroEquipa).HasForeignKey(a => a.MembroEquipaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AlocacaoMensalConfiguration : IEntityTypeConfiguration<AlocacaoMensal>
{
    public void Configure(EntityTypeBuilder<AlocacaoMensal> builder)
    {
        builder.Property(a => a.Percentagem).HasPrecision(5, 2);
        builder.HasIndex(a => new { a.MembroEquipaId, a.Ano, a.Mes }).IsUnique();
    }
}

public class NotaPontoSituacaoConfiguration : IEntityTypeConfiguration<NotaPontoSituacao>
{
    public void Configure(EntityTypeBuilder<NotaPontoSituacao> builder)
    {
        builder.HasOne(n => n.Autor).WithMany().HasForeignKey(n => n.AutorId).OnDelete(DeleteBehavior.SetNull);
    }
}
