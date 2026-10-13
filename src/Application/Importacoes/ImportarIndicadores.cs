using System.Globalization;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;

namespace Innovera.Application.Importacoes;

// Importação do Mod.239 (indicadores de desempenho): valores dos anos e semestres já fechados, objetivo anual e
// plano de ação / acompanhamento de cada KPI.
public partial class ImportarModeloCommandHandler
{
    private sealed record GrupoPeriodo(string Rotulo, int Ano, PeriodoKpi Periodo, int? Abs, int? Rel, int? Objetivo);

    private async Task<List<ResumoFolhaDto>> IndicadoresAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        FolhaLida? folha = null;
        var cabecalho = -1;
        foreach (var f in folhas)
        {
            cabecalho = Enumerable.Range(0, Math.Min(f.Linhas.Count, 20))
                .FirstOrDefault(l => f.Linhas[l].Any(v => Celulas.Texto(v) is { } t && Celulas.Normalizar(t) == "indicador"), -1);
            if (cabecalho >= 0)
            {
                folha = f;
                break;
            }
        }

        if (folha is null || cabecalho + 1 >= folha.Linhas.Count)
        {
            throw new RegraDeNegocioException("Não encontrei a tabela dos indicadores (colunas ID e Indicador) no Mod.239.");
        }

        var topo = folha.Linhas[cabecalho];
        var sub = folha.Linhas[cabecalho + 1];
        var colunaId = Array.FindIndex(topo, v => Celulas.Texto(v) is { } t && Celulas.Normalizar(t) == "id");
        var colunaIndicador = Array.FindIndex(topo, v => Celulas.Texto(v) is { } t && Celulas.Normalizar(t) == "indicador");
        if (colunaId < 0)
        {
            throw new RegraDeNegocioException("Falta a coluna ID no Mod.239.");
        }

        // Grupos de colunas: cada período (2023, "2025 - 1ºS", "2025 - TOTAL"…) tem Abs · Rel · Objetivo anual;
        // depois vêm "Plano Ação Inicial" e "Acompanhamento".
        var inicios = Enumerable.Range(colunaIndicador + 1, Math.Max(0, topo.Length - colunaIndicador - 1))
            .Where(c => Celulas.Texto(topo[c]) is not null)
            .ToList();
        var grupos = new List<GrupoPeriodo>();
        int? planoInicio = null, acompInicio = null;
        for (var i = 0; i < inicios.Count; i++)
        {
            var de = inicios[i];
            var ate = i + 1 < inicios.Count ? inicios[i + 1] : Math.Max(topo.Length, sub.Length);
            var rotulo = Celulas.Texto(topo[de])!;
            var n = Celulas.Normalizar(rotulo);
            if (n.StartsWith("plano", StringComparison.Ordinal))
            {
                planoInicio = de;
                continue;
            }

            if (n.StartsWith("acompanhamento", StringComparison.Ordinal))
            {
                acompInicio = de;
                continue;
            }

            int? Sub(string nome) => Enumerable.Range(de, ate - de)
                .Where(c => c < sub.Length && Celulas.Texto(sub[c]) is { } t && Celulas.Normalizar(t).StartsWith(nome, StringComparison.Ordinal))
                .Select(c => (int?)c)
                .FirstOrDefault();

            var ano = Celulas.Ano(rotulo);
            PeriodoKpi? periodo = n.Contains("1º") || n.Contains("1.º") || n.Contains("1 sem") ? PeriodoKpi.PrimeiroSemestre
                : n.Contains("2º") || n.Contains("2.º") || n.Contains("2 sem") ? PeriodoKpi.SegundoSemestre
                : n.Contains("total") || n == ano?.ToString(CultureInfo.InvariantCulture) ? PeriodoKpi.Anual
                : null;
            if (ano is null || periodo is null)
            {
                Avisar($"Coluna \"{rotulo}\" não importada: só se guardam semestres e anos completos.");
                continue;
            }

            grupos.Add(new GrupoPeriodo(rotulo, ano.Value, periodo.Value, Sub("abs") ?? de, Sub("rel"), Sub("objetivo")));
        }

        var utilizadores = (await context.Utilizadores.Select(u => new { u.Id, u.Nome }).ToListAsync(cancellationToken))
            .GroupBy(u => Celulas.Chave(u.Nome))
            .ToDictionary(g => g.Key, g => g.First().Id);
        int valoresNovos = 0, valoresExistiam = 0, objetivosNovos = 0, objetivosExistiam = 0, acoesNovas = 0, acoesExistiam = 0;

        for (var l = cabecalho + 2; l < folha.Linhas.Count; l++)
        {
            var linha = folha.Linhas[l];
            if (Celulas.Inteiro(TabelaModelo.Valor(linha, colunaId)) is not int numero || numero is < 1 or > 41)
            {
                continue;
            }

            var def = CatalogoKpi.Todos.First(k => k.Numero == numero);
            foreach (var g in grupos)
            {
                var valor = ValorNaUnidade(def.Unidade, Celulas.Numero(TabelaModelo.Valor(linha, g.Abs)), Celulas.Numero(TabelaModelo.Valor(linha, g.Rel)));
                if (valor is not null)
                {
                    var registo = await PlanosKpi.ObterOuCriarAsync(context, numero, g.Ano, g.Periodo, cancellationToken);
                    if (registo.ValorAbsoluto is null)
                    {
                        registo.ValorAbsoluto = valor;
                        registo.ValorRelativo = Celulas.Numero(TabelaModelo.Valor(linha, g.Rel));
                        valoresNovos++;
                    }
                    else
                    {
                        valoresExistiam++;
                    }
                }

                // O objetivo é anual: o que está num semestre também é o objetivo do ano.
                var objetivo = TextoObjetivo(def.Unidade, TabelaModelo.Valor(linha, g.Objetivo));
                if (objetivo is not null)
                {
                    var anual = await PlanosKpi.ObterOuCriarAsync(context, numero, g.Ano, cancellationToken);
                    if (anual.Objetivo is null)
                    {
                        anual.Objetivo = objetivo;
                        objetivosNovos++;
                    }
                    else
                    {
                        objetivosExistiam++;
                    }
                }
            }

            var (novas, existiam) = await AcoesAsync(numero, linha, sub, planoInicio, acompInicio, utilizadores, cancellationToken);
            acoesNovas += novas;
            acoesExistiam += existiam;
        }

        if (folhas.Count > 1)
        {
            Avisar($"Só foi importada a folha \"{folha.Nome}\" (as outras folhas têm indicadores antigos com outra numeração).");
        }

        return
        [
            new ResumoFolhaDto("Valores dos indicadores", valoresNovos, valoresExistiam, 0),
            new ResumoFolhaDto("Objetivos anuais", objetivosNovos, objetivosExistiam, 0),
            new ResumoFolhaDto("Ações (plano e acompanhamento)", acoesNovas, acoesExistiam, 0),
        ];
    }

    /// <summary>O valor na unidade do KPI: as percentagens ficam de 0 a 100 (no Excel estão como 0,74).</summary>
    private static decimal? ValorNaUnidade(UnidadeKpi unidade, decimal? abs, decimal? rel)
    {
        if (unidade != UnidadeKpi.Percentagem)
        {
            return abs is null ? null : Math.Round(abs.Value, 2);
        }

        if (rel is not null)
        {
            return Math.Round(rel.Value * 100, 1);
        }

        return abs is null ? null : Math.Round(abs.Value <= 1 ? abs.Value * 100 : abs.Value, 1);
    }

    /// <summary>"≥ 30" fica como está; um número sozinho (18, 0,2) passa a texto ("18", "20%").</summary>
    private static string? TextoObjetivo(UnidadeKpi unidade, object? valor)
    {
        if (valor is double d)
        {
            var n = (decimal)d;
            // 0,2 é sempre uma percentagem (o Mod.239 tem objetivos relativos também em KPIs de contagem).
            return (unidade == UnidadeKpi.Percentagem && n <= 1) || (n > 0 && n < 1)
                ? (n * 100).ToString("0.##", CultureInfo.InvariantCulture) + "%"
                : n.ToString("0.##", CultureInfo.InvariantCulture);
        }

        var texto = Celulas.Texto(valor);
        return Celulas.SemValor(texto) ? null : Celulas.Cortar(texto, 50);
    }

    /// <summary>Plano de ação inicial e acompanhamento do KPI (no ano da data de criação).</summary>
    private async Task<(int Novas, int Existiam)> AcoesAsync(int numero, object?[] linha, object?[] sub, int? planoInicio, int? acompInicio,
        Dictionary<string, int> utilizadores, CancellationToken cancellationToken)
    {
        int? Coluna(int? de, int? ate, string nome)
        {
            if (de is null)
            {
                return null;
            }

            for (var c = de.Value; c < (ate ?? sub.Length) && c < sub.Length; c++)
            {
                if (Celulas.Texto(sub[c]) is { } t && Celulas.Normalizar(t).StartsWith(nome, StringComparison.Ordinal))
                {
                    return c;
                }
            }

            return null;
        }

        var fimPlano = acompInicio is not null && acompInicio > planoInicio ? acompInicio : null;
        int novas = 0, existiam = 0;

        // Plano de ação inicial
        var descricao = Celulas.Texto(TabelaModelo.Valor(linha, Coluna(planoInicio, fimPlano, "descricao")));
        var criacao = Celulas.Data(TabelaModelo.Valor(linha, Coluna(planoInicio, fimPlano, "data")));
        var ano = criacao?.Year ?? Hoje.Year;
        if (!Celulas.SemValor(descricao))
        {
            var (responsavelId, recursos) = Responsavel(Celulas.Texto(TabelaModelo.Valor(linha, Coluna(planoInicio, fimPlano, "responsavel"))),
                Celulas.Texto(TabelaModelo.Valor(linha, Coluna(planoInicio, fimPlano, "recursos"))), utilizadores);
            var valor = await PlanosKpi.ObterOuCriarAsync(context, numero, ano, cancellationToken);
            if (valor.Acoes.Any(a => a.Tipo == TipoAcaoKpi.PlanoInicial && Celulas.Chave(a.Descricao) == Celulas.Chave(descricao)))
            {
                existiam++;
            }
            else
            {
                valor.Acoes.Add(new AcaoKpi
                {
                    ValorKpi = valor,
                    Tipo = TipoAcaoKpi.PlanoInicial,
                    DataCriacao = criacao ?? Hoje,
                    Descricao = Celulas.Cortar(descricao, 2000)!,
                    Recursos = Celulas.Cortar(recursos, 1000),
                    ResponsavelId = responsavelId,
                    Prazo = Celulas.Data(TabelaModelo.Valor(linha, Coluna(planoInicio, fimPlano, "prazo"))),
                });
                novas++;
            }
        }

        // Acompanhamento
        if (acompInicio is not null)
        {
            var analise = Celulas.Texto(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "analise")));
            var acao = Celulas.Texto(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "plano")));
            if (!Celulas.SemValor(acao) || !Celulas.SemValor(analise))
            {
                var texto = Celulas.SemValor(acao) ? "(análise dos resultados)" : acao!;
                var (responsavelId, recursos) = Responsavel(Celulas.Texto(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "responsavel"))), null,
                    utilizadores);
                var concluida = Celulas.Data(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "acao conclu")));
                var resultado = Celulas.Texto(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "resultado")));
                var valor = await PlanosKpi.ObterOuCriarAsync(context, numero, ano, cancellationToken);
                if (valor.Acoes.Any(a => a.Tipo == TipoAcaoKpi.Acompanhamento && Celulas.Chave(a.Descricao) == Celulas.Chave(texto)
                    && Celulas.Chave(a.AnaliseResultados) == Celulas.Chave(analise)))
                {
                    existiam++;
                }
                else
                {
                    valor.Acoes.Add(new AcaoKpi
                    {
                        ValorKpi = valor,
                        Tipo = TipoAcaoKpi.Acompanhamento,
                        DataCriacao = criacao ?? Hoje,
                        AnaliseResultados = Celulas.SemValor(analise) ? null : Celulas.Cortar(analise, 4000),
                        Descricao = Celulas.Cortar(texto, 2000)!,
                        Recursos = Celulas.Cortar(recursos, 1000),
                        ResponsavelId = responsavelId,
                        Prazo = Celulas.Data(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "prazo"))),
                        ConcluidaEm = concluida,
                        Eficaz = Eficaz(resultado),
                        AvaliacaoAnual = Celulas.Cortar(Celulas.Texto(TabelaModelo.Valor(linha, Coluna(acompInicio, null, "avaliacao"))), 4000),
                    });
                    novas++;
                }
            }
        }

        return (novas, existiam);
    }

    /// <summary>"eficaz" → sim, "não eficaz" → não, o resto → por avaliar.</summary>
    private static bool? Eficaz(string? resultado)
    {
        var n = resultado is null ? string.Empty : Celulas.Normalizar(resultado);
        if (n.StartsWith("nao", StringComparison.Ordinal))
        {
            return false;
        }

        return n.StartsWith("eficaz", StringComparison.Ordinal) ? true : null;
    }

    /// <summary>O responsável é um utilizador da plataforma com o mesmo nome; se não houver, o nome fica escrito nos recursos.</summary>
    private static (int? Id, string? Recursos) Responsavel(string? nome, string? recursos, Dictionary<string, int> utilizadores)
    {
        if (Celulas.SemValor(nome))
        {
            return (null, recursos);
        }

        if (utilizadores.TryGetValue(Celulas.Chave(nome), out var id))
        {
            return (id, recursos);
        }

        return (null, Celulas.SemValor(recursos) ? $"Responsável: {nome}" : $"{recursos} · Responsável: {nome}");
    }
}
