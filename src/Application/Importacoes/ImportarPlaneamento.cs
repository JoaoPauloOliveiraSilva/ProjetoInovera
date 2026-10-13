using System.Globalization;
using Innovera.Application.Iniciativas;

namespace Innovera.Application.Importacoes;

// Importação do Mod.52 (estratégia de inovação), Mod.69 (plano anual de atividades) e Mod.252 (lições aprendidas).
public partial class ImportarModeloCommandHandler
{
    // ---------- Mod.52 · Estratégia (uma folha por ano) ----------

    private async Task<List<ResumoFolhaDto>> EstrategiaAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var existentes = (await context.LinhasEstrategia.Select(l => new { l.Ano, l.AreaAtuacao, l.PlanoAcao, l.Abordagem }).ToListAsync(cancellationToken))
            .Select(l => ChaveEstrategia(l.Ano, l.AreaAtuacao, l.PlanoAcao ?? l.Abordagem))
            .ToHashSet();
        var iniciativas = await context.Iniciativas.Select(i => new { i.Id, i.Titulo }).ToListAsync(cancellationToken);
        var titulos = iniciativas
            .Select(i => (i.Id, Chave: Celulas.Chave(i.Titulo)))
            .Where(i => i.Chave.Length >= 5)
            .OrderByDescending(i => i.Chave.Length)
            .ToList();
        var resumos = new List<ResumoFolhaDto>();

        foreach (var folha in folhas)
        {
            var t = new TabelaModelo(folha, "areas de atuacao");
            if (!t.Encontrada)
            {
                continue;
            }

            var ano = AnoDaFolha(folha);
            if (ano is null)
            {
                Avisar($"Folha {folha.Nome}: não encontrei o ano (\"Ano: 2026\") — ignorada.");
                continue;
            }

            int novos = 0, existiam = 0;
            string? area = null;
            foreach (var (linha, indice) in t.Dados())
            {
                area = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("areas de atuacao"))) ?? area;
                var abordagem = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("abordagens")));
                var plano = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("plano de acao")));
                if (abordagem is null && plano is null)
                {
                    continue;
                }

                if (area is null)
                {
                    Avisar($"Folha {folha.Nome}, linha {indice + 1}: sem área de atuação — ficou \"Outras\".");
                }

                var areaFinal = area ?? "Outras";
                if (!existentes.Add(ChaveEstrategia(ano.Value, areaFinal, plano ?? abordagem)))
                {
                    existiam++;
                    continue;
                }

                // "Nº RHs" às vezes tem texto ("2,4M€ proj budget"): vai para os recursos extra.
                var rhs = TabelaModelo.Valor(linha, t.Coluna("n rhs", "nº rhs", "n.º rhs"));
                var extra = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("recursos extra")));
                if (Celulas.Inteiro(rhs) is null && Celulas.Texto(rhs) is { } textoRhs)
                {
                    extra = Celulas.SemValor(extra) ? textoRhs : $"{textoRhs}; {extra}";
                }

                var kpis = Celulas.Numeros(TabelaModelo.Valor(linha, t.Coluna("kpi"))).Where(n => n is >= 1 and <= 41).ToList();
                var chavePlano = Celulas.Chave(plano ?? abordagem);
                var iniciativa = titulos.FirstOrDefault(i => chavePlano.Contains(i.Chave, StringComparison.Ordinal));
                var horizonte = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("horizonte")));

                context.LinhasEstrategia.Add(new LinhaEstrategia
                {
                    Ano = ano.Value,
                    AreaAtuacao = Celulas.Cortar(areaFinal, 200)!,
                    Abordagem = Celulas.Cortar(abordagem, 500),
                    PlanoAcao = plano,
                    Kpis = kpis,
                    DuracaoMeses = Celulas.Inteiro(TabelaModelo.Valor(linha, t.Coluna("duracao"))),
                    NumeroRecursosHumanos = Celulas.Inteiro(rhs),
                    HorasMesPorRecurso = Celulas.Numero(TabelaModelo.Valor(linha, t.Coluna("horas"))),
                    RecursosExtra = Celulas.SemValor(extra) ? null : Celulas.Cortar(extra, 500),
                    Horizonte = horizonte is null ? null : Celulas.Normalizar(horizonte) switch
                    {
                        "h1" => Horizonte.H1,
                        "h2" => Horizonte.H2,
                        "h3" => Horizonte.H3,
                        _ => null,
                    },
                    IniciativaId = iniciativa.Chave is null ? null : iniciativa.Id,
                });
                novos++;
            }

            resumos.Add(new ResumoFolhaDto($"Estratégia {ano}", novos, existiam, 0));
        }

        if (resumos.Count == 0)
        {
            throw new RegraDeNegocioException("Não encontrei nenhuma folha com a coluna \"Áreas de atuação\".");
        }

        return resumos;
    }

    private static string ChaveEstrategia(int ano, string area, string? plano) => $"{ano}|{Celulas.Chave(area)}|{Celulas.Chave(plano)}";

    /// <summary>O ano escrito na folha ("Ano: 2026") ou, se não houver, o nome da folha ("2026").</summary>
    private static int? AnoDaFolha(FolhaLida folha)
    {
        foreach (var linha in folha.Linhas.Take(5))
        {
            for (var c = 0; c < linha.Length; c++)
            {
                if (Celulas.Texto(linha[c]) is { } texto && Celulas.Normalizar(texto).StartsWith("ano", StringComparison.Ordinal))
                {
                    var ano = Celulas.Ano(texto) ?? (c + 1 < linha.Length ? Celulas.Inteiro(linha[c + 1]) : null);
                    if (ano is >= 2000 and <= 2100)
                    {
                        return ano;
                    }
                }
            }
        }

        return Celulas.Ano(folha.Nome);
    }

    // ---------- Mod.69 · Plano anual (calendário pintado com a cor de cada atividade) ----------

    private static readonly string[] Meses =
        ["janeiro", "fevereiro", "marco", "abril", "maio", "junho", "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"];

    private async Task<ResumoFolhaDto> PlanoAnualAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var folha = folhas.First(f => f.Linhas.Take(15).SelectMany(l => l).OfType<string>()
            .Any(s => Celulas.Normalizar(s).StartsWith("planeamento anual de atividades", StringComparison.Ordinal)));
        var ano = folha.Linhas.Take(6).SelectMany(l => l).Select(Celulas.Texto).Select(Celulas.Ano).FirstOrDefault(a => a is not null)
            ?? throw new RegraDeNegocioException("Não encontrei o ano do plano (\"Ano: 2026\") no título do Mod.69.");

        var legenda = Legenda(folha);
        if (legenda.Count == 0)
        {
            throw new RegraDeNegocioException("Não encontrei a legenda das atividades (colunas \"Cor\" e \"Descrição das Atividades\") no Mod.69.");
        }

        var existentes = (await context.AtividadesPlanoAnual.Select(a => new { a.Titulo, a.DataInicio }).ToListAsync(cancellationToken))
            .Select(a => $"{Celulas.Chave(a.Titulo)}|{a.DataInicio:yyyy-MM-dd}")
            .ToHashSet();
        var semLegenda = new Dictionary<string, List<DateOnly>>();
        int novos = 0, existiam = 0;

        for (var l = 0; l < folha.Linhas.Count; l++)
        {
            var linha = folha.Linhas[l];
            var colunaMes = Enumerable.Range(0, Math.Min(linha.Length, 4))
                .FirstOrDefault(c => Celulas.Texto(linha[c]) is { } t && Meses.Contains(Celulas.Normalizar(t)), -1);
            if (colunaMes < 0)
            {
                continue;
            }

            var mes = Array.IndexOf(Meses, Celulas.Normalizar(Celulas.Texto(linha[colunaMes])!)) + 1;
            // O dia é a posição a partir da célula com o dia 1 (há números trocados no Excel, a posição não engana).
            var colunaDia1 = Enumerable.Range(colunaMes + 1, Math.Max(0, linha.Length - colunaMes - 1))
                .FirstOrDefault(c => Celulas.Inteiro(linha[c]) == 1, -1);
            if (colunaDia1 < 0)
            {
                continue;
            }

            var diasNoMes = DateTime.DaysInMonth(ano, mes);
            for (var c = colunaDia1; c < linha.Length && c - colunaDia1 < diasNoMes; c++)
            {
                if (Celulas.Inteiro(linha[c]) is null)
                {
                    continue;
                }

                var data = new DateOnly(ano, mes, c - colunaDia1 + 1);
                foreach (var cor in folha.CoresDe(l, c))
                {
                    if (!legenda.TryGetValue(cor, out var atividade))
                    {
                        if (!CinzentoOuBranco(cor))
                        {
                            if (!semLegenda.TryGetValue(cor, out var dias))
                            {
                                dias = [];
                                semLegenda[cor] = dias;
                            }

                            dias.Add(data);
                        }

                        continue;
                    }

                    if (Celulas.Normalizar(atividade.Titulo).StartsWith("feriado", StringComparison.Ordinal))
                    {
                        continue;
                    }

                    if (!existentes.Add($"{Celulas.Chave(atividade.Titulo)}|{data:yyyy-MM-dd}"))
                    {
                        existiam++;
                        continue;
                    }

                    context.AtividadesPlanoAnual.Add(new AtividadePlanoAnual
                    {
                        Titulo = Celulas.Cortar(atividade.Titulo, 200)!,
                        Descricao = atividade.Responsaveis is null ? null : $"Responsáveis: {atividade.Responsaveis}",
                        Cor = "#" + cor,
                        DataInicio = data,
                        Recorrencia = Periodicidade.Nenhuma,
                    });
                    novos++;
                }
            }
        }

        foreach (var (cor, dias) in semLegenda)
        {
            Avisar($"Cor #{cor} sem atividade na legenda em {string.Join(", ", dias.Take(5).Select(d => d.ToString("dd/MM", CultureInfo.InvariantCulture)))}"
                + (dias.Count > 5 ? $" e mais {dias.Count - 5} dias" : string.Empty) + " — não importado.");
        }

        return new ResumoFolhaDto($"Plano anual {ano}", novos, existiam, 0);
    }

    private sealed record AtividadeLegenda(string Titulo, string? Responsaveis);

    /// <summary>Legenda do Mod.69: cor → atividade (há dois blocos lado a lado com Responsáveis · Cor · Descrição das Atividades).</summary>
    private Dictionary<string, AtividadeLegenda> Legenda(FolhaLida folha)
    {
        var legenda = new Dictionary<string, AtividadeLegenda>();
        for (var l = 0; l < folha.Linhas.Count; l++)
        {
            var linha = folha.Linhas[l];
            for (var c = 0; c < linha.Length; c++)
            {
                if (Celulas.Texto(linha[c]) is not { } texto
                    || !Celulas.Normalizar(texto).StartsWith("descricao das atividades", StringComparison.Ordinal))
                {
                    continue;
                }

                var colunaCor = UltimaAntes(linha, c, t => t == "cor");
                var colunaResp = UltimaAntes(linha, c, t => t.StartsWith("responsave", StringComparison.Ordinal));
                if (colunaCor < 0)
                {
                    continue;
                }

                var vazias = 0;
                for (var k = l + 1; k < folha.Linhas.Count && vazias < 3; k++)
                {
                    var abaixo = folha.Linhas[k];
                    var titulo = Celulas.Texto(TabelaModelo.Valor(abaixo, c));
                    if (titulo is null)
                    {
                        vazias++;
                        continue;
                    }

                    vazias = 0;
                    var cores = folha.CoresDe(k, colunaCor);
                    if (cores.Count == 0)
                    {
                        Avisar($"Legenda: \"{titulo}\" não tem cor — não importada.");
                        continue;
                    }

                    var responsaveis = colunaResp < 0 ? null : Celulas.Texto(TabelaModelo.Valor(abaixo, colunaResp));
                    legenda.TryAdd(cores[0], new AtividadeLegenda(titulo, responsaveis));
                }
            }
        }

        return legenda;
    }

    private static int UltimaAntes(object?[] linha, int coluna, Func<string, bool> condicao)
    {
        for (var c = coluna - 1; c >= 0; c--)
        {
            if (Celulas.Texto(linha[c]) is { } t && condicao(Celulas.Normalizar(t)))
            {
                return c;
            }
        }

        return -1;
    }

    /// <summary>O fundo dos dias sem atividade (branco) e dos fins de semana (cinzento).</summary>
    private static bool CinzentoOuBranco(string cor) => cor.Length == 6 && cor[..2] == cor[2..4] && cor[2..4] == cor[4..6];

    // ---------- Mod.252 · Lições aprendidas (ficam no Project Charter da iniciativa) ----------

    private async Task<ResumoFolhaDto> LicoesAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (folha, t) = Tabela(folhas, "licao");
        var iniciativas = await context.Iniciativas.OfType<IniciativaComCharter>()
            .Include(i => i.Charter).ThenInclude(c => c!.LicoesAprendidas)
            .ToListAsync(cancellationToken);
        var naoEncontradas = new Dictionary<string, int>();
        int novos = 0, existiam = 0, ignorados = 0;

        foreach (var (linha, _) in t.Dados())
        {
            var licao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("licao")));
            var nome = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("iniciativa")));
            if (licao is null)
            {
                continue;
            }

            var iniciativa = nome is null ? null : ProcurarIniciativa(iniciativas, nome);
            if (iniciativa is null)
            {
                var chave = nome ?? "(sem iniciativa)";
                naoEncontradas[chave] = naoEncontradas.GetValueOrDefault(chave) + 1;
                ignorados++;
                continue;
            }

            if (iniciativa.Charter is null)
            {
                iniciativa.Charter = new ProjectCharter { Iniciativa = iniciativa };
                context.ProjectCharters.Add(iniciativa.Charter);
            }

            var charter = iniciativa.Charter;
            if (charter.LicoesAprendidas.Any(x => Celulas.Chave(x.Licao) == Celulas.Chave(licao)))
            {
                existiam++;
                continue;
            }

            // Sem o evento LicaoRegistada: importar o histórico não deve encher a equipa de avisos.
            charter.LicoesAprendidas.Add(new LicaoAprendida
            {
                Charter = charter,
                Categoria = CategoriaDe(Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("categ")))),
                Licao = licao,
                Impacto = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("impacto"))),
            });
            novos++;
        }

        foreach (var (nome, quantas) in naoEncontradas)
        {
            Avisar($"{folha.Nome}: não encontrei o projeto ou desafio \"{nome}\" ({quantas} lições não importadas). "
                + "Importe primeiro o Mod.246 ou crie a iniciativa, e importe outra vez.");
        }

        return new ResumoFolhaDto("Lições aprendidas", novos, existiam, ignorados);
    }

    private static CategoriaLicao CategoriaDe(string? texto)
    {
        var n = texto is null ? string.Empty : Celulas.Normalizar(texto);
        return n switch
        {
            _ when n.StartsWith("estrat", StringComparison.Ordinal) => CategoriaLicao.Estrategica,
            _ when n.StartsWith("oper", StringComparison.Ordinal) => CategoriaLicao.Operacional,
            _ when n.StartsWith("tecn", StringComparison.Ordinal) => CategoriaLicao.Tecnica,
            _ => CategoriaLicao.Gestao,
        };
    }

    /// <summary>
    /// A iniciativa com o nome escrito no Excel: igual sem espaços e acentos ("AIPoles" = "AI Poles"), pelo código, pelas iniciais
    /// ("IA" = "Inteligência Artificial") ou com 1–2 letras trocadas ("Postes Compoósito" = "Postes Compósitos").
    /// </summary>
    private static IniciativaComCharter? ProcurarIniciativa(IReadOnlyList<IniciativaComCharter> iniciativas, string nome)
    {
        var chave = Celulas.Chave(nome);
        if (chave.Length == 0)
        {
            return null;
        }

        return iniciativas.FirstOrDefault(i => Celulas.Chave(i.Titulo) == chave || Celulas.Chave(i.Codigo) == chave)
            ?? iniciativas.FirstOrDefault(i => Iniciais(i.Titulo) == chave)
            ?? iniciativas
                .Select(i => (Iniciativa: i, Distancia: Distancia(Celulas.Chave(i.Titulo), chave)))
                .Where(x => chave.Length >= 6 && x.Distancia <= 2)
                .OrderBy(x => x.Distancia)
                .Select(x => x.Iniciativa)
                .FirstOrDefault();
    }

    private static string Iniciais(string titulo)
    {
        var palavras = Celulas.Normalizar(titulo).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return palavras.Length < 2 ? string.Empty : new string(palavras.Select(p => p[0]).Where(char.IsLetterOrDigit).ToArray());
    }

    /// <summary>Distância de edição (Levenshtein).</summary>
    private static int Distancia(string a, string b)
    {
        var anterior = Enumerable.Range(0, b.Length + 1).ToArray();
        for (var i = 1; i <= a.Length; i++)
        {
            var atual = new int[b.Length + 1];
            atual[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var custo = a[i - 1] == b[j - 1] ? 0 : 1;
                atual[j] = Math.Min(Math.Min(atual[j - 1] + 1, anterior[j] + 1), anterior[j - 1] + custo);
            }

            anterior = atual;
        }

        return anterior[b.Length];
    }
}
