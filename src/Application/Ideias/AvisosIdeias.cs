namespace Innovera.Application.Ideias;

/// <summary>Os avisos (sino e email) de cada passo do ciclo de uma ideia.</summary>
public static class AvisosIdeias
{
    public static string Link(Ideia ideia) => $"/inovacao/ideias/{ideia.Id}";

    /// <summary>Autor, coautores e quem registou a ideia (mesmo anónima).</summary>
    public static async Task<List<int>> AutoresAsync(IApplicationDbContext context, Ideia ideia, CancellationToken cancellationToken)
    {
        var ids = ideia.Coautores.Select(c => c.UtilizadorId).ToList();
        if (ideia.AutorId is not null)
        {
            ids.Add(ideia.AutorId.Value);
        }

        if (ideia.CreatedBy is not null)
        {
            var criador = await context.Utilizadores
                .Where(u => u.IdentityId == ideia.CreatedBy)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync(cancellationToken);
            if (criador is not null)
            {
                ids.Add(criador.Value);
            }
        }

        return ids.Distinct().ToList();
    }

    public static Task CoautoresIndicadosAsync(INotificacoes notificacoes, Ideia ideia, IEnumerable<int> coautores, CancellationToken cancellationToken)
        => notificacoes.NotificarAsync(coautores,
            $"Confirme a coautoria da ideia {ideia.Codigo}",
            $"Foi indicado como autor da ideia \"{ideia.Titulo}\". Confirme a autoria para a ideia seguir para a equipa de Inovação.",
            Link(ideia), enviarEmail: true, cancellationToken);

    public static Task ParaValidarAsync(INotificacoes notificacoes, Ideia ideia, CancellationToken cancellationToken)
        => notificacoes.NotificarComPermissaoAsync(Permissoes.Ideias.Validar,
            $"Ideia {ideia.Codigo} para validar",
            $"\"{ideia.Titulo}\" está à espera da validação da equipa de Inovação.",
            Link(ideia), cancellationToken);

    public static async Task PublicadaAsync(IApplicationDbContext context, INotificacoes notificacoes, Ideia ideia, CancellationToken cancellationToken)
        => await notificacoes.NotificarAsync(await AutoresAsync(context, ideia, cancellationToken),
            $"A ideia {ideia.Codigo} está em discussão",
            $"A equipa de Inovação validou \"{ideia.Titulo}\". Fica em discussão pública até {ideia.FimDiscussao:dd/MM/yyyy}.",
            Link(ideia), enviarEmail: true, cancellationToken);

    public static async Task DiscussaoFechadaAsync(IApplicationDbContext context, INotificacoes notificacoes, Ideia ideia, CancellationToken cancellationToken)
    {
        await notificacoes.NotificarAsync(await AutoresAsync(context, ideia, cancellationToken),
            $"Terminou a discussão da ideia {ideia.Codigo}",
            $"\"{ideia.Titulo}\" vai agora ser avaliada pela Comissão Executiva. Recebe um aviso com a decisão.",
            Link(ideia), enviarEmail: true, cancellationToken);

        await notificacoes.NotificarComPermissaoAsync(Permissoes.Ideias.Avaliar,
            $"Ideia {ideia.Codigo} para avaliar",
            $"Terminou a discussão de \"{ideia.Titulo}\". Já está na página de avaliação.",
            "/inovacao/avaliacao", cancellationToken);
    }

    public static async Task DecisaoAsync(IApplicationDbContext context, INotificacoes notificacoes, Ideia ideia, CancellationToken cancellationToken)
    {
        var aprovada = ideia.Estado == EstadoIdeia.Aprovada;
        var classe = ideia.Classe is null ? string.Empty : $" Classe: {NomeClasse(ideia.Classe.Value)}.";
        await notificacoes.NotificarAsync(await AutoresAsync(context, ideia, cancellationToken),
            aprovada ? $"A ideia {ideia.Codigo} foi aprovada" : $"A ideia {ideia.Codigo} não foi aprovada",
            $"Decisão sobre \"{ideia.Titulo}\": {(aprovada ? "aprovada" : "não aprovada")}.{classe} Obrigado pela sua participação.",
            Link(ideia), enviarEmail: true, cancellationToken);
    }

    public static async Task ComentarioAsync(IApplicationDbContext context, INotificacoes notificacoes, Ideia ideia, Utilizador quem,
        CancellationToken cancellationToken)
    {
        var autores = (await AutoresAsync(context, ideia, cancellationToken)).Where(id => id != quem.Id);
        await notificacoes.NotificarAsync(autores,
            $"Novo comentário na ideia {ideia.Codigo}",
            $"{quem.Nome} comentou \"{ideia.Titulo}\".",
            Link(ideia), enviarEmail: false, cancellationToken);
    }

    private static string NomeClasse(ClasseIdeia classe) => classe switch
    {
        ClasseIdeia.MelhoriaContinua => "Melhoria Contínua",
        ClasseIdeia.Projeto => "Projeto",
        ClasseIdeia.Desafio => "Desafio",
        ClasseIdeia.IdeiaDuplicada => "Ideia Duplicada",
        ClasseIdeia.Arquivada => "Arquivada",
        _ => "N.A.",
    };
}
