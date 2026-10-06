using Innovera.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Innovera.Infrastructure.Data.Configurations;

public sealed class DomainModelConfiguration : IEntityTypeConfiguration<Ideia>
{
    public void Configure(EntityTypeBuilder<Ideia> builder)
    {
        builder.HasMany(ideia => ideia.Comentarios)
            .WithOne(comentario => comentario.Ideia)
            .HasForeignKey(comentario => comentario.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ideia => ideia.Votos)
            .WithOne(voto => voto.Ideia)
            .HasForeignKey(voto => voto.IdeiaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ideia => ideia.IniciativaResultante)
            .WithMany()
            .HasForeignKey(ideia => ideia.IniciativaResultanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(ideia => ideia.Codigo).IsUnique();
    }

    public static void ConfigureAll(ModelBuilder builder)
    {
        builder.Entity<Iniciativa>()
            .HasOne(iniciativa => iniciativa.Charter)
            .WithOne(charter => charter.Iniciativa)
            .HasForeignKey<ProjectCharter>(charter => charter.IniciativaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Iniciativa>()
            .HasMany(iniciativa => iniciativa.Atividades)
            .WithOne(atividade => atividade.Iniciativa)
            .HasForeignKey(atividade => atividade.IniciativaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Atividade>()
            .HasMany(atividade => atividade.Responsaveis)
            .WithOne(responsavel => responsavel.Atividade)
            .HasForeignKey(responsavel => responsavel.AtividadeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Iniciativa>()
            .HasMany(iniciativa => iniciativa.Parceiros)
            .WithMany(parceiro => parceiro.Iniciativas);

        builder.Entity<Indicador>()
            .HasMany(indicador => indicador.Valores)
            .WithOne(valor => valor.Indicador)
            .HasForeignKey(valor => valor.IndicadorId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Indicador>().HasIndex(indicador => indicador.NumeroId).IsUnique();
        builder.Entity<IndicadorValor>()
            .HasIndex(valor => new { valor.IndicadorId, valor.InicioPeriodo, valor.FimPeriodo })
            .IsUnique();

        builder.Entity<EstrategiaAnual>()
            .HasMany(estrategia => estrategia.Indicadores)
            .WithOne(indicador => indicador.EstrategiaAnual)
            .HasForeignKey(indicador => indicador.EstrategiaAnualId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<EstrategiaAnual>()
            .HasMany(estrategia => estrategia.Atividades)
            .WithOne(atividade => atividade.EstrategiaAnual)
            .HasForeignKey(atividade => atividade.EstrategiaAnualId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.Tarefas)
            .WithOne(tarefa => tarefa.ProjectCharter)
            .HasForeignKey(tarefa => tarefa.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.Entregaveis)
            .WithOne(entregavel => entregavel.ProjectCharter)
            .HasForeignKey(entregavel => entregavel.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.Milestones)
            .WithOne(milestone => milestone.ProjectCharter)
            .HasForeignKey(milestone => milestone.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.Riscos)
            .WithOne(risco => risco.ProjectCharter)
            .HasForeignKey(risco => risco.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.AlocacoesEquipa)
            .WithOne(alocacao => alocacao.ProjectCharter)
            .HasForeignKey(alocacao => alocacao.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.Orcamentos)
            .WithOne(orcamento => orcamento.ProjectCharter)
            .HasForeignKey(orcamento => orcamento.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<ProjectCharter>()
            .HasMany(charter => charter.LicoesAprendidas)
            .WithOne(licao => licao.ProjectCharter)
            .HasForeignKey(licao => licao.ProjectCharterId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Vigilancia>()
            .HasMany(vigilancia => vigilancia.Execucoes)
            .WithOne(execucao => execucao.Vigilancia)
            .HasForeignKey(execucao => execucao.VigilanciaId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<Conhecimento>()
            .HasOne(conhecimento => conhecimento.ProjectCharterMilestone)
            .WithMany()
            .HasForeignKey(conhecimento => conhecimento.ProjectCharterMilestoneId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Conhecimento>()
            .HasOne(conhecimento => conhecimento.ExecucaoVigilancia)
            .WithMany()
            .HasForeignKey(conhecimento => conhecimento.ExecucaoVigilanciaId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<FerramentaMetodo>()
            .HasMany(ferramenta => ferramenta.AnalisesAnuais)
            .WithOne(analise => analise.FerramentaMetodo)
            .HasForeignKey(analise => analise.FerramentaMetodoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<IdeiaVoto>()
            .HasIndex(voto => new { voto.IdeiaId, voto.UtilizadorId })
            .IsUnique();
    }
}
