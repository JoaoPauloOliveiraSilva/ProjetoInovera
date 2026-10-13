using Innovera.Domain.Entities;

namespace Innovera.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Utilizador> Utilizadores { get; }
    DbSet<Alerta> Alertas { get; }

    DbSet<Notificacao> Notificacoes { get; }
    DbSet<OutputGerado> OutputsGerados { get; }
    DbSet<RegistoAuditoria> RegistosAuditoria { get; }

    DbSet<Ideia> Ideias { get; }
    DbSet<ComentarioIdeia> ComentariosIdeia { get; }
    DbSet<AnexoIdeia> AnexosIdeia { get; }

    DbSet<Iniciativa> Iniciativas { get; }
    DbSet<ProjectCharter> ProjectCharters { get; }

    DbSet<TarefaWbs> TarefasWbs { get; }

    DbSet<Entrega> Entregas { get; }

    DbSet<Risco> Riscos { get; }

    DbSet<LinhaOrcamento> LinhasOrcamento { get; }

    DbSet<RequisitoConformidade> RequisitosConformidade { get; }

    DbSet<LicaoAprendida> LicoesAprendidas { get; }

    DbSet<LinkDocumento> LinksDocumento { get; }

    DbSet<MembroEquipa> MembrosEquipa { get; }

    DbSet<AlocacaoMensal> AlocacoesMensais { get; }

    DbSet<Parceiro> Parceiros { get; }
    DbSet<AcordoParceria> AcordosParceria { get; }
    DbSet<AtivoIntangivel> AtivosIntangiveis { get; }
    DbSet<ConhecimentoCodificado> ConhecimentosCodificados { get; }
    DbSet<ConhecimentoTacito> ConhecimentosTacitos { get; }
    DbSet<FerramentaMetodo> FerramentasMetodos { get; }
    DbSet<LinhaEstrategia> LinhasEstrategia { get; }
    DbSet<AtividadePlanoAnual> AtividadesPlanoAnual { get; }

    DbSet<KpiDefinicao> Kpis { get; }
    DbSet<ValorKpi> ValoresKpi { get; }
    DbSet<ValorExterno> ValoresExternos { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
