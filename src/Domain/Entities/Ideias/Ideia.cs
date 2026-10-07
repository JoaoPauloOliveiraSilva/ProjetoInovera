namespace Innovera.Domain.Entities;

/// <summary>
/// Ideia submetida por um trabalhador (formulário de 4 passos e Mod.246, folha Ideias).
/// Tem ciclo próprio (validações, discussão de 30 dias e avaliação) e, se aprovada como projeto,
/// desafio ou melhoria, dá origem a uma <see cref="Iniciativa"/>. Por isso não herda de Iniciativa.
/// </summary>
public class Ideia : BaseAuditableEntity
{
    public const int DiasDeDiscussao = 30;

    /// <summary>Código interno sequencial (AA01 … ZZ99), ver GeradorCodigoIdeia.</summary>
    public string Codigo { get; set; } = string.Empty;

    /// <summary>ID numérico da Caixa de Inovação (ex.: 3036), quando a ideia vem de lá.</summary>
    public int? IdCaixaInovacao { get; set; }

    // Passo 1 — tipo de ideia
    public TipoIdeia Tipo { get; set; } = TipoIdeia.Melhoria;

    // Passo 2 — informação geral
    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    /// <summary>Contrário de "visível para todos": só a equipa de Inovação e os autores a veem.</summary>
    public bool Privada { get; set; }

    /// <summary>Autor principal (quem regista, se for autor).</summary>
    public int? AutorId { get; set; }

    public Utilizador? Autor { get; set; }

    /// <summary>"Autor é anónimo".</summary>
    public bool Anonima { get; set; }

    /// <summary>"Outro autor ou autores": têm de confirmar a autoria.</summary>
    public ICollection<AutorIdeia> Coautores { get; set; } = new List<AutorIdeia>();

    public ICollection<AnexoIdeia> Anexos { get; set; } = new List<AnexoIdeia>();

    // Passo 3 — informação detalhada
    public string? Vantagens { get; set; }

    public string? Requisitos { get; set; }

    public string? ComoEFeitoAtualmente { get; set; }

    /// <summary>Só no tipo "novo produto/serviço detalhado".</summary>
    public string? ModeloNegocio { get; set; }

    /// <summary>Só no tipo "novo produto/serviço detalhado".</summary>
    public string? Competidores { get; set; }

    /// <summary>Só no tipo "novo produto/serviço detalhado".</summary>
    public string? Custos { get; set; }

    // Passo 4 — termos e condições (cedência de propriedade intelectual ao dstgroup)
    public DateTimeOffset? TermosAceitesEm { get; set; }

    /// <summary>Ex.: "Caixa de inovação", "Plataforma".</summary>
    public string? Origem { get; set; }

    /// <summary>dst (grupo) ou dstelecom: definido depois do mês de discussão.</summary>
    public Responsabilidade Responsabilidade { get; set; } = Responsabilidade.Dstelecom;

    public DateTimeOffset DataSubmissao { get; set; }

    public DateTimeOffset? FimDiscussao { get; set; }

    public EstadoIdeia Estado { get; set; } = EstadoIdeia.ValidacaoAutores;

    public ClassificacaoIdeia? Classificacao { get; set; }

    /// <summary>Classe depois da decisão (Mod.246: Melhoria Contínua, Projeto, Desafio, Duplicada, Arquivada, N.A.).</summary>
    public ClasseIdeia? Classe { get; set; }

    /// <summary>Texto livre do Mod.246 (ex.: "Será enquadrada em Projeto de Mestrado").</summary>
    public string? Status { get; set; }

    public AvaliacaoIdeia? Avaliacao { get; set; }

    /// <summary>Iniciativa criada a partir desta ideia (0..1). A chave está em <see cref="Iniciativa.IdeiaOrigemId"/>.</summary>
    public Iniciativa? IniciativaResultante { get; set; }

    public ICollection<ComentarioIdeia> Comentarios { get; set; } = new List<ComentarioIdeia>();

    public ICollection<LikeIdeia> Likes { get; set; } = new List<LikeIdeia>();

    /// <summary>Passo 4: aceitar os termos e condições.</summary>
    public void AceitarTermos(DateTimeOffset agora) => TermosAceitesEm = agora;

    public void AdicionarAnexo(AnexoIdeia anexo)
    {
        ArgumentNullException.ThrowIfNull(anexo);

        if (anexo.TamanhoBytes > AnexoIdeia.TamanhoMaximoBytes)
        {
            throw new RegraDeNegocioException("O anexo ultrapassa o limite de 25 MB.");
        }

        Anexos.Add(anexo);
    }

    /// <summary>
    /// "Registar ideia": exige os termos aceites. Se houver coautores por confirmar fica em validação
    /// dos autores; senão passa logo para a validação da equipa de Inovação.
    /// </summary>
    public void Submeter(DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.ValidacaoAutores, "submeter");

        if (string.IsNullOrWhiteSpace(Titulo))
        {
            throw new RegraDeNegocioException("A ideia precisa de um título.");
        }

        if (TermosAceitesEm is null)
        {
            throw new RegraDeNegocioException("É preciso aceitar os termos e condições.");
        }

        DataSubmissao = agora;
        AtualizarValidacaoAutores();
    }

    /// <summary>Um coautor confirma que é autor da ideia.</summary>
    public void ConfirmarAutoria(int utilizadorId, DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.ValidacaoAutores, "confirmar a autoria");

        var coautor = Coautores.FirstOrDefault(c => c.UtilizadorId == utilizadorId)
            ?? throw new RegraDeNegocioException("Este utilizador não é coautor da ideia.");

        coautor.Confirmado = true;
        coautor.ConfirmadoEm = agora;
        AtualizarValidacaoAutores();
    }

    /// <summary>A equipa de Inovação valida a ideia e abre o mês de discussão pública.</summary>
    public void IniciarDiscussao(DateTimeOffset agora)
    {
        GarantirEstado(EstadoIdeia.ValidacaoEquipa, "iniciar a discussão");

        FimDiscussao = agora.AddDays(DiasDeDiscussao);
        Estado = EstadoIdeia.EmDiscussao;
    }

    public bool DiscussaoTerminou(DateTimeOffset agora)
        => Estado == EstadoIdeia.EmDiscussao && FimDiscussao.HasValue && FimDiscussao.Value <= agora;

    /// <summary>Fecho automático ao fim de 30 dias: passa para "Em avaliação pelo manager".</summary>
    public void FecharDiscussao()
    {
        GarantirEstado(EstadoIdeia.EmDiscussao, "fechar a discussão");

        Estado = EstadoIdeia.EmAvaliacao;
        Classificacao = ClassificacaoIdeia.EmAvaliacaoPeloManager;
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
    /// Regista a avaliação (5 notas de 0 a 4): calcula a nota com os pesos configurados e decide
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
        RegistarDecisao(avaliacao.Aprovada == true);
    }

    /// <summary>
    /// Ideias da responsabilidade "dst" (grupo) não levam notas (N.A.): só se regista se foi aprovada ou não.
    /// </summary>
    public void RegistarDecisaoSemNotas(bool aprovada)
    {
        GarantirEstado(EstadoIdeia.EmAvaliacao, "registar a decisão");

        if (Responsabilidade != Responsabilidade.Dst)
        {
            throw new RegraDeNegocioException("As ideias da dstelecom são avaliadas com as 5 variáveis.");
        }

        RegistarDecisao(aprovada);
    }

    /// <summary>
    /// Define a classe. Aprovada: Melhoria Contínua, Projeto, Desafio ou Arquivada.
    /// Não aprovada: Ideia Duplicada ou N.A. (sem mérito).
    /// </summary>
    public void DefinirClasse(ClasseIdeia classe)
    {
        if (Estado == EstadoIdeia.Aprovada)
        {
            if (classe != ClasseIdeia.MelhoriaContinua && classe != ClasseIdeia.Projeto
                && classe != ClasseIdeia.Desafio && classe != ClasseIdeia.Arquivada)
            {
                throw new RegraDeNegocioException("Uma ideia aprovada passa a Melhoria Contínua, Projeto, Desafio ou Arquivada.");
            }
        }
        else if (Estado == EstadoIdeia.NaoAprovada)
        {
            if (classe != ClasseIdeia.IdeiaDuplicada && classe != ClasseIdeia.NaoAplicavel)
            {
                throw new RegraDeNegocioException("Uma ideia não aprovada fica como Ideia Duplicada ou N.A.");
            }
        }
        else
        {
            throw new RegraDeNegocioException($"Não é possível definir a classe: a ideia está no estado {Estado}.");
        }

        Classe = classe;
    }

    private void RegistarDecisao(bool aprovada)
    {
        Estado = aprovada ? EstadoIdeia.Aprovada : EstadoIdeia.NaoAprovada;
        Classificacao = aprovada ? ClassificacaoIdeia.Aprovada : ClassificacaoIdeia.NaoAprovada;

        AddDomainEvent(new IdeiaAvaliadaEvent(this));
    }

    private void AtualizarValidacaoAutores()
    {
        Estado = Coautores.All(c => c.Confirmado) ? EstadoIdeia.ValidacaoEquipa : EstadoIdeia.ValidacaoAutores;
    }

    private void GarantirEstado(EstadoIdeia esperado, string operacao)
    {
        if (Estado != esperado)
        {
            throw new RegraDeNegocioException($"Não é possível {operacao}: a ideia está no estado {Estado}.");
        }
    }
}
