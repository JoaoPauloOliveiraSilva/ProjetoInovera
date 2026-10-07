using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

public class IdeiaConfiguration : IEntityTypeConfiguration<Ideia>
{
    public void Configure(EntityTypeBuilder<Ideia> builder)
    {
        builder.HasIndex(i => i.Numero).IsUnique();
        builder.HasIndex(i => i.Codigo);
        builder.HasIndex(i => i.Estado);
        builder.Property(i => i.Codigo).HasMaxLength(10);
        builder.Property(i => i.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(i => i.Origem).HasMaxLength(100);
        builder.Property(i => i.Status).HasMaxLength(500);

        builder.HasOne(i => i.Autor).WithMany().HasForeignKey(i => i.AutorId).OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(i => i.Avaliacao).WithOne(a => a.Ideia)
            .HasForeignKey<AvaliacaoIdeia>(a => a.IdeiaId).OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Coautores).WithOne(c => c.Ideia).HasForeignKey(c => c.IdeiaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.Anexos).WithOne(a => a.Ideia).HasForeignKey(a => a.IdeiaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.Comentarios).WithOne(c => c.Ideia).HasForeignKey(c => c.IdeiaId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(i => i.Likes).WithOne(l => l.Ideia).HasForeignKey(l => l.IdeiaId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AvaliacaoIdeiaConfiguration : IEntityTypeConfiguration<AvaliacaoIdeia>
{
    public void Configure(EntityTypeBuilder<AvaliacaoIdeia> builder)
    {
        builder.Property(a => a.Nota).HasPrecision(4, 2);
        builder.Property(a => a.Observacoes).HasMaxLength(2000);
    }
}

public class AutorIdeiaConfiguration : IEntityTypeConfiguration<AutorIdeia>
{
    public void Configure(EntityTypeBuilder<AutorIdeia> builder)
    {
        builder.HasIndex(a => new { a.IdeiaId, a.UtilizadorId }).IsUnique();
        builder.HasOne(a => a.Utilizador).WithMany().HasForeignKey(a => a.UtilizadorId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AnexoIdeiaConfiguration : IEntityTypeConfiguration<AnexoIdeia>
{
    public void Configure(EntityTypeBuilder<AnexoIdeia> builder)
    {
        builder.Property(a => a.NomeFicheiro).HasMaxLength(260).IsRequired();
        builder.Property(a => a.TipoConteudo).HasMaxLength(150);
        builder.Property(a => a.ChaveArmazenamento).HasMaxLength(500).IsRequired();
    }
}

public class LikeIdeiaConfiguration : IEntityTypeConfiguration<LikeIdeia>
{
    public void Configure(EntityTypeBuilder<LikeIdeia> builder)
    {
        builder.HasIndex(l => new { l.IdeiaId, l.UtilizadorId }).IsUnique();
        builder.HasOne(l => l.Utilizador).WithMany().HasForeignKey(l => l.UtilizadorId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ComentarioIdeiaConfiguration : IEntityTypeConfiguration<ComentarioIdeia>
{
    public void Configure(EntityTypeBuilder<ComentarioIdeia> builder)
    {
        builder.Property(c => c.Texto).HasMaxLength(2000).IsRequired();
        builder.HasOne(c => c.Autor).WithMany().HasForeignKey(c => c.AutorId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(c => c.Gostos).WithOne(g => g.Comentario).HasForeignKey(g => g.ComentarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class GostoComentarioConfiguration : IEntityTypeConfiguration<GostoComentario>
{
    public void Configure(EntityTypeBuilder<GostoComentario> builder)
    {
        builder.HasIndex(g => new { g.ComentarioId, g.UtilizadorId }).IsUnique();
        builder.HasOne(g => g.Utilizador).WithMany().HasForeignKey(g => g.UtilizadorId).OnDelete(DeleteBehavior.Cascade);
    }
}
