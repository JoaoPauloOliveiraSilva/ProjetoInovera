namespace Innovera.Application.Ideias;

public record IdeiaResumoDto(
    int Id,
    string Codigo,
    string Titulo,
    TipoIdeia Tipo,
    string Autores,
    DateTimeOffset Data,
    EstadoIdeia Estado,
    bool Privada,
    int Likes,
    int Comentarios,
    Responsabilidade Responsabilidade,
    ClassificacaoIdeia? Classificacao,
    ClasseIdeia? Classe,
    decimal? Nota);

public record CoautorDto(int UtilizadorId, string Nome, bool Confirmado);

public record AnexoDto(int Id, string NomeFicheiro, long TamanhoBytes);

public record ComentarioDto(int Id, string Autor, string? Empresa, string Texto, DateTimeOffset Data, int Gostos, bool Gostei);

public record AvaliacaoDto(int? Custo, int? Enquadramento, int? Beneficio, int? AdequacaoTecnica, int? Incerteza, decimal? Nota, bool? Aprovada, string? Observacoes);

public record IdeiaDetalheDto(
    int Id,
    string Codigo,
    string Titulo,
    TipoIdeia Tipo,
    string? Descricao,
    string? Vantagens,
    string? Requisitos,
    string? ComoEFeitoAtualmente,
    string? ModeloNegocio,
    string? Competidores,
    string? Custos,
    bool Privada,
    bool Anonima,
    string? Autor,
    string Autores,
    IReadOnlyList<CoautorDto> Coautores,
    DateTimeOffset Data,
    EstadoIdeia Estado,
    DateTimeOffset? FimDiscussao,
    Responsabilidade Responsabilidade,
    ClassificacaoIdeia? Classificacao,
    ClasseIdeia? Classe,
    string? Status,
    int Likes,
    bool Gostei,
    IReadOnlyList<AnexoDto> Anexos,
    IReadOnlyList<ComentarioDto> Comentarios,
    AvaliacaoDto? Avaliacao,
    bool PodeConfirmarAutoria,
    bool PodeAnexar,
    int? IniciativaId);

public record IdeiaCriadaDto(int Id, string Codigo, EstadoIdeia Estado);
