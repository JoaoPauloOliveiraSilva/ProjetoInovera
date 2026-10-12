namespace Innovera.Application.Charters;

// ---------- Relatório de entregável/milestone → conhecimento codificado (Mod.237) ----------

/// <summary>
/// Quando é registado o relatório de uma entrega, cria (ou atualiza) a entrada no repositório de conhecimento codificado.
/// Corre dentro do SaveChanges (eventos de domínio), por isso só acrescenta ao contexto.
/// </summary>
public class RelatorioParaConhecimentoCodificado(IApplicationDbContext context) : INotificationHandler<RelatorioDeMilestoneRegistadoEvent>
{
    public const string ModoConsultaOneDrive = "OneDrive dstelecom inov";

    public async Task Handle(RelatorioDeMilestoneRegistadoEvent notification, CancellationToken cancellationToken)
    {
        var entrega = notification.Entrega;
        var iniciativa = entrega.Charter.Iniciativa;

        var existente = entrega.Id == 0
            ? null
            : await context.ConhecimentosCodificados.FirstOrDefaultAsync(c => c.EntregaId == entrega.Id, cancellationToken);
        var registo = existente ?? new ConhecimentoCodificado
        {
            Origem = OrigemConhecimento.RelatorioIniciativa,
            Entrega = entrega,
            Iniciativa = iniciativa,
            ModoConsulta = ModoConsultaOneDrive,
            Resultado = "Síntese e partilha do conhecimento gerado",
        };

        var tipo = entrega.Tipo == TipoEntrega.Milestone ? "Milestone" : "Entregável";
        var documento = $"Relatório {iniciativa.Titulo} — {tipo} {entrega.Codigo}".Trim();
        registo.Documento = documento.Length > 300 ? documento[..300] : documento;
        registo.Descricao = entrega.Descricao;
        registo.Url = entrega.LinkRelatorio;
        registo.Data = entrega.DataConclusao ?? DateOnly.FromDateTime(DateTime.UtcNow);
        registo.ArquivadoEm = registo.Data;

        if (existente is null)
        {
            context.ConhecimentosCodificados.Add(registo);
        }
    }
}

// ---------- Repositório de lições aprendidas (Mod.252) ----------

public record LicaoRepositorioDto(int Id, DateTimeOffset Data, int IniciativaId, string IniciativaCodigo, string IniciativaTitulo,
    CategoriaLicao Categoria, string Licao, string? Descricao, string? Impacto);

/// <summary>Todas as lições aprendidas dos Project Charters (Mod.252), as mais recentes primeiro.</summary>
[Authorize]
public record ListarLicoesQuery(CategoriaLicao? Categoria, string? Texto) : IRequest<IReadOnlyList<LicaoRepositorioDto>>;

public class ListarLicoesQueryHandler(IApplicationDbContext context) : IRequestHandler<ListarLicoesQuery, IReadOnlyList<LicaoRepositorioDto>>
{
    public async Task<IReadOnlyList<LicaoRepositorioDto>> Handle(ListarLicoesQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.LicoesAprendidas.AsNoTracking();
        if (request.Categoria is not null)
        {
            consulta = consulta.Where(l => l.Categoria == request.Categoria);
        }

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(l => l.Licao.ToLower().Contains(texto)
                || (l.Descricao != null && l.Descricao.ToLower().Contains(texto))
                || l.Charter.Iniciativa.Codigo.ToLower().Contains(texto)
                || l.Charter.Iniciativa.Titulo.ToLower().Contains(texto));
        }

        return await consulta
            .OrderByDescending(l => l.Created)
            .Select(l => new LicaoRepositorioDto(l.Id, l.Created, l.Charter.IniciativaId, l.Charter.Iniciativa.Codigo, l.Charter.Iniciativa.Titulo,
                l.Categoria, l.Licao, l.Descricao, l.Impacto))
            .ToListAsync(cancellationToken);
    }
}

// ---------- Repositório de conhecimento codificado (Mod.237) ----------

public record ConhecimentoCodificadoDto(int Id, DateOnly Data, string Documento, string? Url, string? ModoConsulta, string? Descricao,
    string? Resultado, OrigemConhecimento Origem, int? IniciativaId, string? IniciativaCodigo, string? EntregaCodigo);

/// <summary>Conhecimento codificado (Mod.237): relatórios de entregáveis/milestones, resultados de vigilâncias e entradas manuais.</summary>
[Authorize]
public record ListarConhecimentoCodificadoQuery(OrigemConhecimento? Origem, string? Texto) : IRequest<IReadOnlyList<ConhecimentoCodificadoDto>>;

public class ListarConhecimentoCodificadoQueryHandler(IApplicationDbContext context)
    : IRequestHandler<ListarConhecimentoCodificadoQuery, IReadOnlyList<ConhecimentoCodificadoDto>>
{
    public async Task<IReadOnlyList<ConhecimentoCodificadoDto>> Handle(ListarConhecimentoCodificadoQuery request, CancellationToken cancellationToken)
    {
        var consulta = context.ConhecimentosCodificados.AsNoTracking();
        if (request.Origem is not null)
        {
            consulta = consulta.Where(c => c.Origem == request.Origem);
        }

        if (!string.IsNullOrWhiteSpace(request.Texto))
        {
            var texto = request.Texto.Trim().ToLower();
            consulta = consulta.Where(c => c.Documento.ToLower().Contains(texto)
                || (c.Descricao != null && c.Descricao.ToLower().Contains(texto))
                || (c.Iniciativa != null && c.Iniciativa.Codigo.ToLower().Contains(texto)));
        }

        return await consulta
            .OrderByDescending(c => c.Data).ThenByDescending(c => c.Id)
            .Select(c => new ConhecimentoCodificadoDto(c.Id, c.Data, c.Documento, c.Url, c.ModoConsulta, c.Descricao, c.Resultado, c.Origem,
                c.IniciativaId, c.Iniciativa != null ? c.Iniciativa.Codigo : null, c.Entrega != null ? c.Entrega.Codigo : null))
            .ToListAsync(cancellationToken);
    }
}


