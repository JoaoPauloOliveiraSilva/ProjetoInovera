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
