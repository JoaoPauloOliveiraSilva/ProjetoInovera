using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

public class UtilizadorConfiguration : IEntityTypeConfiguration<Utilizador>
{
    public void Configure(EntityTypeBuilder<Utilizador> builder)
    {
        builder.HasIndex(u => u.IdentityId).IsUnique();
        builder.HasIndex(u => u.Email);
        builder.Property(u => u.IdentityId).HasMaxLength(450).IsRequired();
        builder.Property(u => u.Nome).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.NumeroColaborador).HasMaxLength(50);
        builder.Property(u => u.Departamento).HasMaxLength(150);
        builder.Property(u => u.Empresa).HasMaxLength(150);
        builder.Property(u => u.FotoUrl).HasMaxLength(500);
    }
}

public class NotificacaoConfiguration : IEntityTypeConfiguration<Notificacao>
{
    public void Configure(EntityTypeBuilder<Notificacao> builder)
    {
        builder.Property(n => n.Titulo).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Mensagem).HasMaxLength(1000).IsRequired();
        builder.Property(n => n.Link).HasMaxLength(300);
        builder.HasIndex(n => new { n.DestinatarioId, n.Lida });
        builder.HasOne(n => n.Destinatario).WithMany().HasForeignKey(n => n.DestinatarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.Property(a => a.Mensagem).HasMaxLength(1000);
        builder.HasIndex(a => new { a.Alvo, a.AlvoId, a.Tipo, a.DataPrevista });
        builder.HasOne(a => a.Destinatario).WithMany().HasForeignKey(a => a.DestinatarioId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class OutputGeradoConfiguration : IEntityTypeConfiguration<OutputGerado>
{
    public void Configure(EntityTypeBuilder<OutputGerado> builder)
    {
        builder.Property(o => o.NomeFicheiro).HasMaxLength(260);
        builder.Property(o => o.Periodo).HasMaxLength(20);
        builder.Property(o => o.Url).HasMaxLength(1000);
        builder.HasOne(o => o.Iniciativa).WithMany().HasForeignKey(o => o.IniciativaId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(o => o.GeradoPor).WithMany().HasForeignKey(o => o.GeradoPorId).OnDelete(DeleteBehavior.SetNull);
    }
}

public class RegistoAuditoriaConfiguration : IEntityTypeConfiguration<RegistoAuditoria>
{
    public void Configure(EntityTypeBuilder<RegistoAuditoria> builder)
    {
        builder.Property(r => r.Entidade).HasMaxLength(100);
        builder.Property(r => r.EntidadeId).HasMaxLength(100);
        builder.Property(r => r.UtilizadorId).HasMaxLength(450);
        // Texto (JSON) e não jsonb: assim a procura por texto no registo funciona com lower()/LIKE.
        builder.Property(r => r.ValoresAntes).HasColumnType("text");
        builder.Property(r => r.ValoresDepois).HasColumnType("text");
        builder.HasIndex(r => new { r.Entidade, r.EntidadeId });
    }
}

public class LinkDocumentoConfiguration : IEntityTypeConfiguration<LinkDocumento>
{
    public void Configure(EntityTypeBuilder<LinkDocumento> builder)
    {
        builder.Property(l => l.Titulo).HasMaxLength(200);
        builder.Property(l => l.Url).HasMaxLength(2000).IsRequired();
    }
}
