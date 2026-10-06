namespace Innovera.Domain.Entities;

/// <summary>
/// Ideia submetida por um trabalhador (Mod.246, folha Ideias).
/// Tem ciclo próprio (discussão de 30 dias e avaliação pela CE) e, se aprovada,
/// dá origem a uma <see cref="Iniciativa"/> ligada a esta ideia. Por isso não herda de Iniciativa.
/// </summary>
public class Ideia : BaseAuditableEntity
{
    public const int DiasDeDiscussao = 30;

    /// <summary>Código interno sequencial (AA01 … ZZ99), ver GeradorCodigoIdeia.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>ID numérico da Caixa de Inovação (ex.: 2304), quando a ideia vem de lá.</summary>
    public int? IdCaixaInovacao { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public int? AutorId { get; set; }

    public Utilizador? Autor { get; set; }

    /// <summary>O autor pode pedir para não ser identificado.</summary>
    public bool Anonima { get; set; }

    /// <summary>Ex.: "Caixa de inovação", "Plataforma".</summary>
    public string? Origem { get; set; }

    public Responsabilidade Responsabilidade { get; set; } = Responsabilidade.Dstelecom;

    public DateTimeOffset DataSubmissao { get; set; }

    public DateTimeOffset? FimDiscussao { get; set; }

    public EstadoIdeia Estado { get; set; } = EstadoIdeia.Submetida;

    public ClassificacaoIdeia? Classificacao { get; set; }

    /// <summary>Destino da ideia (Mod.246: Projeto, Desafio, Melhoria Contínua, Arquivada…).</summary>
    public ClasseIdeia? Classe { get; set; }

    /// <summary>Texto livre do Mod.246 (ex.: "Em implementação").</summary>
    public string? Status { get; set; }

    // Estimativas usadas no relatório PDF da ideia
    public decimal? CustoEstimado { get; set; }

    public int? DuracaoEstimadaMeses { get; set; }

    public int? PessoasEstimadas { get; set; }

    public AvaliacaoIdeia? Avaliacao { get; set; }

    /// <summary>Iniciativa criada a partir desta ideia (0..1). A chave está em <see cref="Iniciativa.IdeiaOrigemId"/>.</summary>
    public Iniciativa? IniciativaResultante { get; set; }

    public ICollection<ComentarioIdeia> Comentarios { get; set; } = new List<ComentarioIdeia>();

    public ICollection<LikeIdeia> Likes { get; set; } = new List<LikeIdeia>();

    /// <summary>Abre o mês de discussão pública.</summary>
    public void IniciarDiscussao(DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.Submetida, "iniciar a discussão");

        FimDiscussao = agora.AddDays(DiasDeDiscussao);
        Estado = EstadoIdeia.EmDiscussao;
    }

    public bool DiscussaoTerminou(DateTimeOffset agora)
        => Estado == EstadoIdeia.EmDiscussao && FimDiscussao.HasValue && FimDiscussao.Value <= agora;

    /// <summary>Fecho automático da discussão: a ideia passa para a fila de avaliação da CE.</summary>
    public void FecharDiscussao()
    {
        GarantirEstado(EstadoIdeia.EmDiscussao, "fechar a discussão");

        Estado = EstadoIdeia.EmAvaliacao;
    }

    /// <summary>Um like por pessoa, só durante a discussão.</summary>
    public void AdicionarLike(int utilizadorId, DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.EmDiscussao, "dar like");

        if (Likes.Any(l => l.UtilizadorId == utilizadorId))
        {
            return;
        }

        Likes.Add(new LikeIdeia { UtilizadorId = utilizadorId, Data = agora });
    }

    public void AdicionarComentario(int autorId, string texto, DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.EmDiscussao, "comentar");

        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new RegraDeNegocioException("O comentário não pode estar vazio.");
        }

        Comentarios.Add(new ComentarioIdeia { AutorId = autorId, Texto = texto.Trim(), Data = agora });
    }

    /// <summary>
    /// Regista a avaliação da CE: calcula a nota com os pesos configurados e decide
    /// automaticamente (nota ≥ 2 aprova).
    /// </summary>
    public void RegistarAvaliacao(AvaliacaoIdeia avaliacao, PesosAvaliacao pesos)
    {
        ArgumentNullException.ThrowIfNull(avaliacao);
        ArgumentNullException.ThrowIfNull(pesos);
        GarantirEstado(EstadoIdeia.EmAvaliacao, "registar a avaliação");

        var nota = avaliacao.CalcularNota(pesos);
        if (nota is null)
        {
            throw new RegraDeNegocioException("A avaliação precisa das 5 variáveis preenchidas (0 a 4).");
        }

        Avaliacao = avaliacao;

        if (avaliacao.Aprovada == true)
        {
            Estado = EstadoIdeia.Aprovada;
            Classificacao = ClassificacaoIdeia.Aprovada;
        }
        else
        {
            Estado = EstadoIdeia.NaoAprovada;
            Classificacao = ClassificacaoIdeia.NaoAprovada;
        }

        AddDomainEvent(new IdeiaAvaliadaEvent(this));
    }

    /// <summary>Ideia duplicada ou da responsabilidade "dst": não passa pela avaliação da CE.</summary>
    public void MarcarSemAvaliacao(ClasseIdeia classe)
    {
        if (classe != ClasseIdeia.IdeiaDuplicada && classe != ClasseIdeia.NaoAplicavel)
        {
            throw new RegraDeNegocioException("Só ideias duplicadas ou N.A. ficam sem avaliação.");
        }

        if (Estado == EstadoIdeia.Aprovada || Estado == EstadoIdeia.NaoAprovada)
        {
            throw new RegraDeNegocioException("A ideia já foi avaliada.");
        }

        Classe = classe;
        Estado = EstadoIdeia.SemAvaliacao;
    }

    /// <summary>A equipa de Inovação define o destino de uma ideia aprovada.</summary>
    public void DefinirDestino(ClasseIdeia destino)
    {
        GarantirEstado(EstadoIdeia.Aprovada, "definir o destino");

        if (destino != ClasseIdeia.Projeto && destino != ClasseIdeia.Desafio
            && destino != ClasseIdeia.MelhoriaContinua && destino != ClasseIdeia.Arquivada)
        {
            throw new RegraDeNegocioException("O destino tem de ser Projeto, Desafio, Melhoria Contínua ou Arquivada.");
        }

        Classe = destino;
    }

    private void GarantirEstado(EstadoIdeia esperado, string operacao)
    {
        if (Estado != esperado)
        {
            throw new RegraDeNegocioException($"Não é possível {operacao}: a ideia está no estado {Estado}.");
        }
    }
}
