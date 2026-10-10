namespace Innovera.Domain.Entities;

/// <summary>
/// Núcleo genérico partilhado pelas 7 especializações (Projeto, Desafio, Melhoria Contínua,
/// Oportunidade, Vigilância, Mestrado e Ação de Inovação). Alimenta o portefólio (Mod.246).
/// </summary>
public abstract class Iniciativa : BaseAuditableEntity
{
    public abstract TipoIniciativa Tipo { get; }

    public string Codigo { get; set; } = string.Empty;

    public string Titulo { get; set; } = string.Empty;

    public string? Descricao { get; set; }

    public EstadoIniciativa Estado { get; set; } = EstadoIniciativa.Pendente;

    /// <summary>Ponto de situação atual (resumo). O histórico fica em <see cref="NotasPontoSituacao"/>.</summary>
    public string? PontoSituacao { get; set; }

    /// <summary>Responsável / gestor da iniciativa.</summary>
    public int? ResponsavelId { get; set; }

    public Utilizador? Responsavel { get; set; }

    /// <summary>Nome do responsável quando não é utilizador da plataforma (ex.: importado do Mod.246).</summary>
    public string? ResponsavelNome { get; set; }

    /// <summary>De onde veio na importação (ex.: "Mod.246/Projetos/12"), para não importar duas vezes.</summary>
    public string? ReferenciaImportacao { get; set; }

    public DateOnly? DataInicio { get; set; }

    public DateOnly? DataFim { get; set; }

    /// <summary>Origem ou financiamento (ex.: Inov. Interna, IDT, PRR, reunião interna, Caixa de inovação).</summary>
    public string? Origem { get; set; }

    public PilarEstrategico? PilarEstrategico { get; set; }

    public Horizonte? Horizonte { get; set; }

    /// <summary>Ideia que deu origem a esta iniciativa (0..1).</summary>
    public int? IdeiaOrigemId { get; set; }

    public Ideia? IdeiaOrigem { get; set; }

    public ICollection<LinkDocumento> Links { get; set; } = new List<LinkDocumento>();

    public ICollection<NotaPontoSituacao> NotasPontoSituacao { get; set; } = new List<NotaPontoSituacao>();

    public ICollection<MembroEquipa> MembrosEquipa { get; set; } = new List<MembroEquipa>();

    public ICollection<Parceiro> Parceiros { get; set; } = new List<Parceiro>();

    /// <summary>Prefixo do código (ex.: PRJ-012) de cada tipo.</summary>
    public static string PrefixoCodigo(TipoIniciativa tipo) => tipo switch
    {
        TipoIniciativa.Projeto => "PRJ",
        TipoIniciativa.Desafio => "DES",
        TipoIniciativa.MelhoriaContinua => "MC",
        TipoIniciativa.Oportunidade => "OPO",
        TipoIniciativa.Vigilancia => "VIG",
        TipoIniciativa.Mestrado => "MES",
        TipoIniciativa.AcaoInovacao => "ACA",
        _ => "INI",
    };

    /// <summary>Cria uma iniciativa vazia do tipo pedido.</summary>
    public static Iniciativa Nova(TipoIniciativa tipo) => tipo switch
    {
        TipoIniciativa.Projeto => new Projeto(),
        TipoIniciativa.Desafio => new Desafio(),
        TipoIniciativa.MelhoriaContinua => new MelhoriaContinua(),
        TipoIniciativa.Oportunidade => new Oportunidade(),
        TipoIniciativa.Vigilancia => new Vigilancia(),
        TipoIniciativa.Mestrado => new Mestrado(),
        TipoIniciativa.AcaoInovacao => new AcaoInovacao(),
        _ => throw new ArgumentOutOfRangeException(nameof(tipo)),
    };

    /// <summary>Acrescenta uma nota ao histórico (RF03) e atualiza o ponto de situação atual.</summary>
    public void RegistarPontoSituacao(string texto, int? autorId, DateTimeOffset data)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            throw new RegraDeNegocioException("O ponto de situação não pode estar vazio.");
        }

        NotasPontoSituacao.Add(new NotaPontoSituacao { Texto = texto.Trim(), AutorId = autorId, Data = data });
        PontoSituacao = texto.Trim();
    }
}
