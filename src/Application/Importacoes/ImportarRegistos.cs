using Innovera.Application.Iniciativas;

namespace Innovera.Application.Importacoes;

// Importação dos registos manuais: Mod.235 (parcerias), Mod.236 (ativos intangíveis), Mod.237 (conhecimento codificado),
// Mod.238 (conhecimento tácito) e Mod.241 (ferramentas e métodos).
public partial class ImportarModeloCommandHandler
{
    // ---------- Mod.235 · Acordos de parceria ----------

    private async Task<ResumoFolhaDto> ParceriasAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (folha, t) = Tabela(folhas, "descricao da parceria");
        var parceiros = (await context.Parceiros.Include(p => p.Acordos).ToListAsync(cancellationToken))
            .GroupBy(p => Celulas.Chave(p.Nome))
            .ToDictionary(g => g.Key, g => g.First());
        int novos = 0, existiam = 0, ignorados = 0;

        foreach (var (linha, indice) in t.Dados())
        {
            var nome = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("entidade")));
            var descricao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("descricao da parceria")));
            if (nome is null)
            {
                if (descricao is not null)
                {
                    Avisar($"{folha.Nome}, linha {indice + 1}: sem entidade — ignorada.");
                    ignorados++;
                }

                continue;
            }

            descricao ??= "(sem descrição)";
            var (inicio, fim) = Celulas.Intervalo(TabelaModelo.Valor(linha, t.Coluna("duracao")));
            if (!parceiros.TryGetValue(Celulas.Chave(nome), out var parceiro))
            {
                parceiro = new Parceiro { Nome = Celulas.Cortar(nome, 200)!, Tipo = TipoParceiroDe(nome) };
                context.Parceiros.Add(parceiro);
                parceiros[Celulas.Chave(nome)] = parceiro;
            }

            if (parceiro.Acordos.Any(a => Celulas.Chave(a.Descricao) == Celulas.Chave(descricao) && a.DataInicio == inicio))
            {
                existiam++;
                continue;
            }

            var observacoes = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("observ")));
            var estado = EstadoAcordoDe(observacoes, fim);
            parceiro.Acordos.Add(new AcordoParceria
            {
                Parceiro = parceiro,
                Descricao = descricao,
                ProtocoloEstabelecido = Sim(TabelaModelo.Valor(linha, t.Coluna("protocolo estabelecido", "protocolo"))),
                TipoProtocolo = TipoProtocoloDe(Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("tipo de protocolo")))),
                DataInicio = inicio,
                DataFim = fim,
                IncluiGestaoPropriedadeIntelectual = Sim(TabelaModelo.Valor(linha, t.Coluna("inclui"))),
                Estado = estado,
                // "Terminado" / "Em curso" já ficam no estado; o resto do texto fica nas observações.
                Observacoes = observacoes is not null && Celulas.Normalizar(observacoes) is not ("terminado" or "em curso") ? observacoes : null,
            });
            novos++;
        }

        return new ResumoFolhaDto("Acordos de parceria", novos, existiam, ignorados);
    }

    /// <summary>"Sim", "«" e outros símbolos de visto contam como sim; vazio e "Não" como não.</summary>
    private static bool Sim(object? valor)
    {
        var texto = Celulas.Texto(valor);
        return texto is not null && Celulas.Normalizar(texto) is not ("nao" or "n" or "no" or "n.a." or "-");
    }

    private static TipoParceiro TipoParceiroDe(string nome)
    {
        var n = Celulas.Normalizar(nome);
        string[] academicos = ["universidade", "instituto", "inesc", "politecnico", "faculdade", "escola", "uminho", "piep", "centro de"];
        return academicos.Any(n.Contains) ? TipoParceiro.Academico : TipoParceiro.Empresarial;
    }

    private static TipoProtocolo? TipoProtocoloDe(string? texto)
    {
        if (texto is null)
        {
            return null;
        }

        var n = Celulas.Normalizar(texto);
        return n switch
        {
            _ when n.Contains("partilhad") => TipoProtocolo.InvestigacaoPartilhada,
            _ when n.Contains("promo") => TipoProtocolo.CoPromocao,
            _ when n.Contains("confidencial") || n.Contains("nda") => TipoProtocolo.Confidencialidade,
            _ when n.StartsWith("investiga", StringComparison.Ordinal) => TipoProtocolo.Investigacao,
            _ when n.StartsWith("inova", StringComparison.Ordinal) => TipoProtocolo.Inovacao,
            _ => null,
        };
    }

    private EstadoAcordo EstadoAcordoDe(string? observacoes, DateOnly? fim)
    {
        var n = observacoes is null ? string.Empty : Celulas.Normalizar(observacoes);
        if (n.StartsWith("terminad", StringComparison.Ordinal) || n.StartsWith("conclu", StringComparison.Ordinal))
        {
            return EstadoAcordo.Terminado;
        }

        if (n.StartsWith("em curso", StringComparison.Ordinal))
        {
            return EstadoAcordo.EmCurso;
        }

        return fim is { } f && f < Hoje ? EstadoAcordo.Terminado : EstadoAcordo.EmCurso;
    }

    // ---------- Mod.236 · Ativos intangíveis ----------

    private async Task<ResumoFolhaDto> AtivosAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (folha, t) = Tabela(folhas, "designacao");
        var existentes = (await context.AtivosIntangiveis.ToListAsync(cancellationToken))
            .Select(a => Celulas.Chave(a.Designacao) + "|" + Celulas.Chave(a.Descricao))
            .ToHashSet();
        int novos = 0, existiam = 0, ignorados = 0;

        foreach (var (linha, indice) in t.Dados())
        {
            var designacao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("designacao")));
            var descricao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("descricao")));
            if (designacao is null && descricao is null)
            {
                continue;
            }

            designacao ??= descricao!;
            if (!existentes.Add(Celulas.Chave(designacao) + "|" + Celulas.Chave(descricao)))
            {
                existiam++;
                continue;
            }

            var entidade = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("tipo")));
            var tipo = TipoAtivoDe($"{entidade} {designacao} {descricao}");
            if (tipo is null)
            {
                Avisar($"{folha.Nome}, linha {indice + 1}: não percebi se \"{designacao}\" é patente, design ou marca — ficou como patente.");
            }

            var data = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data")));
            var dataRegisto = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data registo")));
            var ativo = new AtivoIntangivel
            {
                Data = data ?? dataRegisto ?? Hoje,
                Tipo = tipo ?? TipoAtivoIntangivel.Patente,
                EntidadeRegisto = Celulas.Cortar(entidade, 100) ?? string.Empty,
                Designacao = Celulas.Cortar(designacao, 300)!,
                Descricao = descricao,
                NumeroPedido = NumeroDoPedido(descricao),
                Autores = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("inventores", "autores"))),
                DataRegisto = dataRegisto,
                DataValidade = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data validade"))),
                Ambito = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("ambito"))),
                Situacao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("situacao"))),
                Custo = Celulas.Numero(TabelaModelo.Valor(linha, t.Coluna("custo"))),
                PerspetivaValor = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("prespetiva", "perspetiva"))),
                Observacoes = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("observ"))),
            };
            if (Atividade(linha, t) is { } atividade)
            {
                ativo.Atividades.Add(atividade);
            }

            context.AtivosIntangiveis.Add(ativo);
            novos++;
        }

        return new ResumoFolhaDto("Ativos intangíveis", novos, existiam, ignorados);
    }

    private static TipoAtivoIntangivel? TipoAtivoDe(string texto)
    {
        var n = Celulas.Normalizar(texto);
        if (n.Contains("patente"))
        {
            return TipoAtivoIntangivel.Patente;
        }

        if (n.Contains("marca"))
        {
            return TipoAtivoIntangivel.Marca;
        }

        return n.Contains("desenho") || n.Contains("design") || n.Contains("modelo") ? TipoAtivoIntangivel.Design : null;
    }

    /// <summary>"Pedido provisório de patente Nº 20262008307586" → "20262008307586".</summary>
    private static string? NumeroDoPedido(string? descricao)
    {
        if (descricao is null)
        {
            return null;
        }

        var i = descricao.IndexOf("º", StringComparison.Ordinal);
        if (i < 0)
        {
            return null;
        }

        var numero = descricao[(i + 1)..].Trim();
        return numero.Length is > 0 and <= 100 ? numero : null;
    }

    // ---------- Mod.237 · Conhecimento codificado ----------

    private async Task<ResumoFolhaDto> ConhecimentoCodificadoAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (folha, t) = Tabela(folhas, "arquivado em");
        var existentes = (await context.ConhecimentosCodificados.Select(c => new { c.Data, c.Documento }).ToListAsync(cancellationToken))
            .Select(c => $"{c.Data:yyyy-MM-dd}|{Celulas.Chave(c.Documento)}")
            .ToHashSet();
        int novos = 0, existiam = 0, ignorados = 0;

        foreach (var (linha, indice) in t.Dados())
        {
            var documento = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("documento")));
            if (documento is null)
            {
                continue;
            }

            var data = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data")));
            var arquivado = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("arquivado em")));
            if (data is null && arquivado is null)
            {
                Avisar($"{folha.Nome}, linha {indice + 1}: \"{documento}\" sem data — ignorada.");
                ignorados++;
                continue;
            }

            var dataFinal = data ?? arquivado!.Value;
            if (!existentes.Add($"{dataFinal:yyyy-MM-dd}|{Celulas.Chave(documento)}"))
            {
                existiam++;
                continue;
            }

            var modo = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("modo de consulta")));
            var registo = new ConhecimentoCodificado
            {
                Data = dataFinal,
                Documento = Celulas.Cortar(documento, 300)!,
                ArquivadoEm = arquivado,
                ModoConsulta = modo is not null && !modo.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? modo : null,
                Url = modo is not null && modo.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? Celulas.Cortar(modo, 2000) : null,
                Descricao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("descricao do conhecimento", "descricao"))),
                Resultado = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("resultado"))),
                Origem = OrigemConhecimento.Manual,
            };
            if (Atividade(linha, t) is { } atividade)
            {
                registo.Atividades.Add(atividade);
            }

            context.ConhecimentosCodificados.Add(registo);
            novos++;
        }

        return new ResumoFolhaDto("Conhecimento codificado", novos, existiam, ignorados);
    }

    // ---------- Mod.238 · Conhecimento tácito (só informação, sem atividades) ----------

    private async Task<ResumoFolhaDto> ConhecimentoTacitoAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (_, t) = Tabela(folhas, "detentor");
        var existentes = (await context.ConhecimentosTacitos.Select(c => new { c.Detentor, c.Descricao }).ToListAsync(cancellationToken))
            .Select(c => $"{Celulas.Chave(c.Detentor)}|{Celulas.Chave(c.Descricao)}")
            .ToHashSet();
        int novos = 0, existiam = 0;

        foreach (var (linha, _) in t.Dados())
        {
            var detentor = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("detentor")));
            var descricao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("descricao do conhecimento tacito", "descricao")));
            if (detentor is null || descricao is null)
            {
                continue;
            }

            if (!existentes.Add($"{Celulas.Chave(detentor)}|{Celulas.Chave(descricao)}"))
            {
                existiam++;
                continue;
            }

            var nivel = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("nivel", "confidencialidade")));
            context.ConhecimentosTacitos.Add(new ConhecimentoTacito
            {
                Data = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data"))) ?? Hoje,
                Empresa = Celulas.Cortar(Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("empresa"))), 200),
                Detentor = Celulas.Cortar(detentor, 200)!,
                Descricao = descricao,
                Confidencialidade = nivel is not null && Celulas.Normalizar(nivel) is "elevado" or "elevada" or "alto" or "alta"
                    ? NivelConfidencialidade.Elevado
                    : NivelConfidencialidade.Baixo,
            });
            novos++;
        }

        return new ResumoFolhaDto("Conhecimento tácito", novos, existiam, 0);
    }

    // ---------- Mod.241 · Ferramentas e métodos ----------

    private async Task<ResumoFolhaDto> FerramentasAsync(IReadOnlyList<FolhaLida> folhas, CancellationToken cancellationToken)
    {
        var (folha, t) = Tabela(folhas, "identi");
        var existentes = (await context.FerramentasMetodos.ToListAsync(cancellationToken))
            .GroupBy(f => Celulas.Chave(f.Nome))
            .ToDictionary(g => g.Key, g => g.First());

        // Colunas "Análise 2026", "Análise 2027"… (uma por ano).
        var cabecalho = folha.Linhas[t.LinhaCabecalho];
        var analises = Enumerable.Range(0, cabecalho.Length)
            .Select(c => (Coluna: c, Texto: Celulas.Texto(cabecalho[c])))
            .Where(x => x.Texto is not null && Celulas.Normalizar(x.Texto).StartsWith("analise", StringComparison.Ordinal))
            .Select(x => (x.Coluna, Ano: Celulas.Ano(x.Texto)))
            .Where(x => x.Ano is not null)
            .ToList();
        int novos = 0, existiam = 0;

        foreach (var (linha, _) in t.Dados())
        {
            var nome = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("identi")));
            if (nome is null)
            {
                continue;
            }

            var chave = Celulas.Chave(nome);
            if (existentes.TryGetValue(chave, out var ferramenta))
            {
                existiam++;
            }
            else
            {
                var custo = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("custo")));
                ferramenta = new FerramentaMetodo
                {
                    Data = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("data"))) ?? Hoje,
                    Nome = Celulas.Cortar(nome, 200)!,
                    SumarioExecutivo = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("sumario"))),
                    Potencial = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("potencial"))),
                    CustoAquisicao = Celulas.SemValor(custo) ? null : Celulas.Numero(custo),
                    Divulgacao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("divulgacao"))),
                };
                context.FerramentasMetodos.Add(ferramenta);
                existentes[chave] = ferramenta;
                novos++;
            }

            // As análises anuais que faltarem juntam-se também às ferramentas que já existiam.
            foreach (var (coluna, ano) in analises)
            {
                var texto = Celulas.Texto(TabelaModelo.Valor(linha, coluna));
                if (texto is not null && ferramenta.Analises.All(a => a.Ano != ano))
                {
                    ferramenta.Analises.Add(new AnaliseAnual { Ano = ano!.Value, Analise = texto });
                }
            }
        }

        return new ResumoFolhaDto("Ferramentas e métodos", novos, existiam, 0);
    }
}
