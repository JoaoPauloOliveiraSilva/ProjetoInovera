using System.Globalization;
using System.Text;

namespace Innovera.Application.Iniciativas;

public record ResumoFolhaDto(string Folha, int Importadas, int JaExistiam, int Ignoradas);

public record ResultadoImportacaoDto(IReadOnlyList<ResumoFolhaDto> Folhas, IReadOnlyList<string> Avisos);

/// <summary>
/// Importa o Excel Mod.246 (Portefólio de Iniciativas de Inovação) da dstelecom: a folha Ideias e as folhas de
/// iniciativas (Projetos, Desafios, Melhorias Contínuas, Oportunidades, Vigilâncias, Mestrados, Ações de Inovação).
/// Pode correr-se várias vezes: o que já foi importado (mesmo ID na mesma folha) não se repete.
/// </summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record ImportarPortefolioCommand(Stream Conteudo) : IRequest<ResultadoImportacaoDto>;

public class ImportarPortefolioCommandHandler(IApplicationDbContext context, ILeitorFolhas leitor, IGeradorNumeroIdeia gerador)
    : IRequestHandler<ImportarPortefolioCommand, ResultadoImportacaoDto>
{
    private static readonly (string Folha, TipoIniciativa Tipo)[] FolhasIniciativas =
    [
        ("Projetos", TipoIniciativa.Projeto),
        ("Desafios", TipoIniciativa.Desafio),
        ("Melhorias Contínuas", TipoIniciativa.MelhoriaContinua),
        ("Oportunidades", TipoIniciativa.Oportunidade),
        ("Vigilâncias", TipoIniciativa.Vigilancia),
        ("Mestrados", TipoIniciativa.Mestrado),
        ("Ações de Inovação", TipoIniciativa.AcaoInovacao),
    ];

    public async Task<ResultadoImportacaoDto> Handle(ImportarPortefolioCommand request, CancellationToken cancellationToken)
    {
        IReadOnlyList<FolhaLida> folhas;
        try
        {
            folhas = leitor.Ler(request.Conteudo);
        }
        catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException)
        {
            throw new RegraDeNegocioException("Não foi possível ler o ficheiro. Tem de ser um Excel (.xlsx).");
        }

        var resumos = new List<ResumoFolhaDto>();
        var avisos = new List<string>();

        var folhaIdeias = Procurar(folhas, "Ideias");
        var ideiasNovas = new List<Ideia>();
        if (folhaIdeias is not null)
        {
            resumos.Add(await ImportarIdeiasAsync(folhaIdeias, ideiasNovas, avisos, cancellationToken));
        }

        // Ideias aprovadas ainda sem iniciativa: as melhorias/projetos/desafios com o mesmo título ficam ligados a elas.
        var classesComIniciativa = new ClasseIdeia?[] { ClasseIdeia.MelhoriaContinua, ClasseIdeia.Projeto, ClasseIdeia.Desafio };
        var porTitulo = (await context.Ideias
                .Where(i => classesComIniciativa.Contains(i.Classe) && i.IniciativaResultante == null)
                .ToListAsync(cancellationToken))
            .Concat(ideiasNovas.Where(i => classesComIniciativa.Contains(i.Classe)))
            .GroupBy(i => Normalizar(i.Titulo))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var (nomeFolha, tipo) in FolhasIniciativas)
        {
            var folha = Procurar(folhas, nomeFolha);
            if (folha is not null)
            {
                resumos.Add(await ImportarIniciativasAsync(folha, nomeFolha, tipo, porTitulo, avisos, cancellationToken));
            }
        }

        if (resumos.Count == 0)
        {
            throw new RegraDeNegocioException("O ficheiro não tem as folhas do Mod.246 (Ideias, Projetos, Melhorias Contínuas…).");
        }

        await context.SaveChangesAsync(cancellationToken);
        return new ResultadoImportacaoDto(resumos, avisos);
    }

    // ---------- Ideias ----------

    private async Task<ResumoFolhaDto> ImportarIdeiasAsync(FolhaLida folha, List<Ideia> novas, List<string> avisos, CancellationToken cancellationToken)
    {
        var tabela = new Tabela(folha);
        var existentes = (await context.Ideias.Where(i => i.IdCaixaInovacao != null).Select(i => i.IdCaixaInovacao!.Value).ToListAsync(cancellationToken))
            .ToHashSet();
        int jaExistiam = 0, ignoradas = 0;
        int? anoDaSeccao = null;

        foreach (var (linha, numeroLinha) in tabela.Dados())
        {
            var id = Inteiro(tabela.Valor(linha, "id"));
            var titulo = Texto(tabela.Valor(linha, "titulo"));
            if (titulo is null)
            {
                // Linhas só com o ano (2023, 2024…) separam as secções da folha.
                if (id is >= 2000 and <= 2100)
                {
                    anoDaSeccao = id;
                }

                continue;
            }

            if (id is null)
            {
                avisos.Add($"Ideias, linha {numeroLinha}: sem ID — ignorada.");
                ignoradas++;
                continue;
            }

            if (!existentes.Add(id.Value))
            {
                jaExistiam++;
                continue;
            }

            var data = Data(tabela.Valor(linha, "data")) ?? (anoDaSeccao is null ? null : new DateOnly(anoDaSeccao.Value, 1, 1));
            if (data is null)
            {
                avisos.Add($"Ideias, ID {id}: sem data de submissão — ficou com a data de hoje.");
            }

            var notas = new[] { "custo", "enquadramento", "beneficio", "adeq", "incerteza" }
                .Select(c => Nota(tabela.Valor(linha, c)))
                .ToArray();
            var avaliacao = notas.All(n => n is not null)
                ? new AvaliacaoIdeia { Custo = notas[0], Enquadramento = notas[1], Beneficio = notas[2], AdequacaoTecnica = notas[3], Incerteza = notas[4] }
                : null;

            var ideia = Ideia.Importar(
                id.Value,
                Cortar(titulo, 200)!,
                Cortar(Texto(tabela.Valor(linha, "autor")), 200),
                Instante(data ?? DateOnly.FromDateTime(DateTime.UtcNow)),
                Cortar(Texto(tabela.Valor(linha, "origem")), 100),
                ResponsabilidadeDe(Texto(tabela.Valor(linha, "responsabilidade"))),
                avaliacao,
                Decisao(Texto(tabela.Valor(linha, "classificacao"))),
                ClasseDe(Texto(tabela.Valor(linha, "classe"))),
                Cortar(Texto(tabela.Valor(linha, "status")), 500));
            novas.Add(ideia);
        }

        // Os códigos AA01… seguem a ordem das datas.
        foreach (var ideia in novas.OrderBy(i => i.DataSubmissao).ThenBy(i => i.IdCaixaInovacao))
        {
            ideia.AtribuirNumero(await gerador.ProximoAsync(cancellationToken));
            context.Ideias.Add(ideia);
        }

        return new ResumoFolhaDto("Ideias", novas.Count, jaExistiam, ignoradas);
    }

    // ---------- Iniciativas ----------

    private async Task<ResumoFolhaDto> ImportarIniciativasAsync(FolhaLida folha, string nomeFolha, TipoIniciativa tipo,
        Dictionary<string, Ideia> ideiasPorTitulo, List<string> avisos, CancellationToken cancellationToken)
    {
        var tabela = new Tabela(folha);
        var prefixoReferencia = $"Mod.246/{nomeFolha}/";
        var existentes = (await context.Iniciativas
                .Where(i => i.ReferenciaImportacao != null && i.ReferenciaImportacao.StartsWith(prefixoReferencia))
                .Select(i => i.ReferenciaImportacao!)
                .ToListAsync(cancellationToken))
            .ToHashSet();
        int importadas = 0, jaExistiam = 0, ignoradas = 0;

        foreach (var (linha, numeroLinha) in tabela.Dados())
        {
            var titulo = Texto(tabela.Valor(linha, "titulo"));
            if (titulo is null)
            {
                continue;
            }

            var id = Inteiro(tabela.Valor(linha, "id"));
            if (id is null)
            {
                avisos.Add($"{nomeFolha}, linha {numeroLinha}: sem ID — ignorada.");
                ignoradas++;
                continue;
            }

            var referencia = prefixoReferencia + id;
            if (!existentes.Add(referencia))
            {
                jaExistiam++;
                continue;
            }

            var ponto = Texto(tabela.Valor(linha, "ponto de situacao"));
            var resultado = Texto(tabela.Valor(linha, "resultado"));
            var origem = Texto(tabela.Valor(linha, "origem"));

            var iniciativa = Iniciativa.Nova(tipo);
            iniciativa.Codigo = $"{Iniciativa.PrefixoCodigo(tipo)}-{id:000}";
            iniciativa.ReferenciaImportacao = referencia;
            iniciativa.Titulo = Cortar(titulo, 300)!;
            iniciativa.ResponsavelNome = Cortar(Texto(tabela.Valor(linha, "gestor", "autor", "orientador")), 200);
            iniciativa.DataInicio = Data(tabela.Valor(linha, "data de inicio", "data de submissao"));
            iniciativa.DataFim = Data(tabela.Valor(linha, "data de fim"));
            iniciativa.Origem = Cortar(origem, 200);
            iniciativa.PontoSituacao = ponto;
            iniciativa.Horizonte = Texto(tabela.Valor(linha, "horizonte")) switch
            {
                "H1" => Horizonte.H1,
                "H2" => Horizonte.H2,
                "H3" => Horizonte.H3,
                _ => null,
            };
            iniciativa.Estado = EstadoDe(ponto, tipo == TipoIniciativa.Oportunidade && resultado is not null);

            switch (iniciativa)
            {
                case Projeto p:
                    p.TipoProjeto = TipoProjetoDe(Texto(tabela.Valor(linha, "tipo")));
                    p.Ambito = origem is not null && Normalizar(origem).Contains("interna") ? AmbitoProjeto.Interno : AmbitoProjeto.Externo;
                    break;
                case MelhoriaContinua m:
                    m.Resultado = resultado;
                    break;
                case Oportunidade o:
                    o.Resultado = resultado;
                    break;
                case Vigilancia v:
                    v.Descritivo = Texto(tabela.Valor(linha, "descritivo"));
                    iniciativa.Descricao = v.Descritivo;
                    break;
                case Mestrado m:
                    m.NomeAluno = Cortar(Texto(tabela.Valor(linha, "nome aluno")), 200) ?? string.Empty;
                    m.Curso = Cortar(Texto(tabela.Valor(linha, "curso")), 200);
                    break;
                case AcaoInovacao a:
                    a.TipoAcao = origem is not null && Normalizar(origem).Contains("externo") ? TipoAcaoInovacao.Externa : TipoAcaoInovacao.Criada;
                    break;
            }

            // Liga à ideia de origem quando o título é igual (ex.: as melhorias contínuas vêm das ideias aprovadas).
            if (tipo is TipoIniciativa.MelhoriaContinua or TipoIniciativa.Projeto or TipoIniciativa.Desafio
                && ideiasPorTitulo.Remove(Normalizar(titulo), out var ideia))
            {
                iniciativa.IdeiaOrigem = ideia;
            }

            context.Iniciativas.Add(iniciativa);
            importadas++;
        }

        return new ResumoFolhaDto(nomeFolha, importadas, jaExistiam, ignoradas);
    }

    // ---------- Leitura das células ----------

    /// <summary>Uma folha com a linha de cabeçalho (a que começa por "ID") e as colunas pelo nome.</summary>
    private sealed class Tabela
    {
        private readonly FolhaLida _folha;
        private readonly int _linhaCabecalho = -1;
        private readonly List<(string Nome, int Coluna)> _colunas = [];

        public Tabela(FolhaLida folha)
        {
            _folha = folha;
            for (var l = 0; l < folha.Linhas.Count && _linhaCabecalho < 0; l++)
            {
                var linha = folha.Linhas[l];
                for (var c = 0; c < linha.Length; c++)
                {
                    if (linha[c] is string s && Normalizar(s) == "id")
                    {
                        _linhaCabecalho = l;
                        for (var k = c; k < linha.Length; k++)
                        {
                            if (linha[k] is string nome && !string.IsNullOrWhiteSpace(nome))
                            {
                                _colunas.Add((Normalizar(nome), k));
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>Linhas de dados com o número da linha (para os avisos).</summary>
        public IEnumerable<(object?[] Linha, int Numero)> Dados()
        {
            if (_linhaCabecalho < 0)
            {
                yield break;
            }

            for (var l = _linhaCabecalho + 1; l < _folha.Linhas.Count; l++)
            {
                yield return (_folha.Linhas[l], l + 1);
            }
        }

        /// <summary>Valor da primeira coluna cujo cabeçalho começa por um dos nomes.</summary>
        public object? Valor(object?[] linha, params string[] nomes)
        {
            foreach (var nome in nomes)
            {
                var coluna = _colunas.FirstOrDefault(c => c.Nome.StartsWith(nome, StringComparison.Ordinal));
                if (coluna.Nome is not null)
                {
                    return coluna.Coluna < linha.Length ? linha[coluna.Coluna] : null;
                }
            }

            return null;
        }
    }

    private static FolhaLida? Procurar(IReadOnlyList<FolhaLida> folhas, string nome)
        => folhas.FirstOrDefault(f => Normalizar(f.Nome) == Normalizar(nome));

    /// <summary>Minúsculas, sem acentos e com os espaços normalizados (para comparar cabeçalhos e títulos).</summary>
    internal static string Normalizar(string texto)
    {
        var decomposto = texto.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        var espaco = false;
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                if (!espaco)
                {
                    sb.Append(' ');
                }

                espaco = true;
                continue;
            }

            espaco = false;
            sb.Append(c);
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    private static string? Texto(object? valor) => valor switch
    {
        null => null,
        string s => string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
        double d => d.ToString(CultureInfo.InvariantCulture),
        _ => valor.ToString(),
    };

    private static string? Cortar(string? texto, int maximo) => texto is null || texto.Length <= maximo ? texto : texto[..maximo];

    private static int? Inteiro(object? valor) => valor switch
    {
        double d when d == Math.Floor(d) && d is >= int.MinValue and <= int.MaxValue => (int)d,
        string s when int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var n) => n,
        _ => null,
    };

    private static readonly string[] FormatosData = ["M/d/yyyy", "d/M/yyyy", "yyyy-MM-dd", "d-M-yyyy", "d.M.yyyy", "M/d/yy"];

    private static DateOnly? Data(object? valor)
    {
        switch (valor)
        {
            case double d when d is > 1 and < 2958466:
                return DateOnly.FromDateTime(DateTime.FromOADate(d));
            case string s:
                var t = s.Trim();
                if (DateTime.TryParseExact(t, FormatosData, CultureInfo.InvariantCulture, DateTimeStyles.None, out var data))
                {
                    return DateOnly.FromDateTime(data);
                }

                return DateTime.TryParse(t, CultureInfo.GetCultureInfo("pt-PT"), DateTimeStyles.None, out data) ? DateOnly.FromDateTime(data) : null;
            default:
                return null;
        }
    }

    private static DateTimeOffset Instante(DateOnly data) => new(data.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    /// <summary>"N.A." ou vazio = sem nota; 0 a 4 = nota.</summary>
    private static EscalaDeAvaliacao? Nota(object? valor)
        => Inteiro(valor) is int n && n is >= 0 and <= 4 ? (EscalaDeAvaliacao)n : null;

    private static Responsabilidade ResponsabilidadeDe(string? texto)
        => texto is not null && Normalizar(texto) == "dst" ? Responsabilidade.Dst : Responsabilidade.Dstelecom;

    /// <summary>Classificação: "Aprovada", "Não Aprovada" ou vazio/"Em avaliação" (nulo).</summary>
    private static bool? Decisao(string? texto) => texto is null ? null : Normalizar(texto) switch
    {
        "aprovada" => true,
        "nao aprovada" => false,
        _ => null,
    };

    private static ClasseIdeia? ClasseDe(string? texto) => texto is null ? null : Normalizar(texto) switch
    {
        "melhoria continua" => ClasseIdeia.MelhoriaContinua,
        "projeto" => ClasseIdeia.Projeto,
        "desafio" => ClasseIdeia.Desafio,
        "ideia duplicada" => ClasseIdeia.IdeiaDuplicada,
        "arquivada" => ClasseIdeia.Arquivada,
        "n.a." or "na" or "n.a" => ClasseIdeia.NaoAplicavel,
        _ => null,
    };

    private static TipoProjeto? TipoProjetoDe(string? texto) => texto is null ? null : Normalizar(texto) switch
    {
        "investigacao" => TipoProjeto.Investigacao,
        "desenvolvimento & inovacao" => TipoProjeto.DesenvolvimentoInovacao,
        "inovacao" => TipoProjeto.Inovacao,
        "investigacao & desenvolvimento" => TipoProjeto.InvestigacaoDesenvolvimento,
        "id+i" or "i&d+i" => TipoProjeto.InvestigacaoDesenvolvimentoInovacao,
        _ => null,
    };

    /// <summary>Estado a partir do texto do "Ponto de situação" (o Mod.246 não tem uma coluna de estado).</summary>
    private static EstadoIniciativa EstadoDe(string? ponto, bool oportunidadeComResultado)
    {
        var t = ponto is null ? string.Empty : Normalizar(ponto);
        if (t.Contains("nao conclu"))
        {
            return EstadoIniciativa.Cancelada;
        }

        if (t.Contains("conclu") || t.Contains("terminad"))
        {
            return EstadoIniciativa.Concluida;
        }

        if (t.Contains("rejeitad"))
        {
            return EstadoIniciativa.Rejeitada;
        }

        if (t.Contains("em implementa"))
        {
            return EstadoIniciativa.EmImplementacao;
        }

        if (t.Contains("em analise"))
        {
            return EstadoIniciativa.EmAnalise;
        }

        if (t.StartsWith("pendente", StringComparison.Ordinal))
        {
            return EstadoIniciativa.Pendente;
        }

        return oportunidadeComResultado ? EstadoIniciativa.Concluida : EstadoIniciativa.EmCurso;
    }
}
