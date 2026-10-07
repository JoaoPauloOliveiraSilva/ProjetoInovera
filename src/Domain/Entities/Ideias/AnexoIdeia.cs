namespace Innovera.Domain.Entities;

/// <summary>
/// Ficheiro anexado a uma ideia (ex.: RECRUTAMENTO.jpg). O ficheiro fica no armazenamento de objetos
/// (AWS); aqui guarda-se só a referência.
/// </summary>
public class AnexoIdeia : BaseAuditableEntity
{
    /// <summary>Limite do formulário atual: 25 MB.</summary>
    public const long TamanhoMaximoBytes = 25L * 1024 * 1024;

    public int IdeiaId { get; set; }

    public Ideia Ideia { get; set; } = null!;

    public string NomeFicheiro { get; set; } = string.Empty;

    public string TipoConteudo { get; set; } = string.Empty;

    public long TamanhoBytes { get; set; }

    /// <summary>Chave do objeto no armazenamento (ex.: "ideias/3036/recrutamento.jpg").</summary>
    public string ChaveArmazenamento { get; set; } = string.Empty;
}
