using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;

namespace Innovera.Application.Importacoes;

public record ResultadoImportacaoModeloDto(string Modelo, IReadOnlyList<ResumoFolhaDto> Folhas, IReadOnlyList<string> Avisos);

/// <summary>
/// Importa qualquer um dos modelos Excel da dstelecom: o modelo é reconhecido pelo conteúdo (cabeçalhos das colunas), por isso basta
/// escolher o ficheiro. Mod.246 (portefólio), Mod.235 (parcerias), Mod.236 (ativos intangíveis), Mod.237 (conhecimento codificado),
/// Mod.238 (conhecimento tácito), Mod.239 (indicadores: valores, objetivos e plano de ação), Mod.241 (ferramentas e métodos),
/// Mod.252 (lições aprendidas), Mod.52 (estratégia) e Mod.69 (plano anual de atividades).
/// Pode correr-se várias vezes: o que já existe não se repete.
/// </summary>
[Authorize]
public record ImportarModeloCommand(Stream Conteudo) : IRequest<ResultadoImportacaoModeloDto>;

public partial class ImportarModeloCommandHandler(IApplicationDbContext context, ILeitorFolhas leitor, IUtilizadores utilizadores,
    ISender sender, TimeProvider relogio) : IRequestHandler<ImportarModeloCommand, ResultadoImportacaoModeloDto>
{
    private enum Modelo
    {
        Mod246,
        Mod235,
        Mod236,
        Mod237,
        Mod238,
        Mod239,
        Mod241,
        Mod252,
        Mod52,
        Mod69,
    }

    private readonly List<string> _avisos = [];

    private DateOnly Hoje => DateOnly.FromDateTime(relogio.GetUtcNow().UtcDateTime);

    public async Task<ResultadoImportacaoModeloDto> Handle(ImportarModeloCommand request, CancellationToken cancellationToken)
    {
        // Copia para memória: o Mod.246 é lido outra vez pela importação do portefólio.
        var copia = new MemoryStream();
        await request.Conteudo.CopyToAsync(copia, cancellationToken);
        copia.Position = 0;

        IReadOnlyList<FolhaLida> folhas;
        try
        {
            folhas = leitor.Ler(copia);
        }
        catch (Exception ex) when (ex is InvalidDataException or System.Xml.XmlException)
        {
            throw new RegraDeNegocioException("Não foi possível ler o ficheiro. Tem de ser um Excel (.xlsx).");
        }

        var modelo = Detetar(folhas)
            ?? throw new RegraDeNegocioException("Não reconheci o modelo deste ficheiro. Escolha um dos modelos da inovação "
                + "(Mod.246, 235, 236, 237, 238, 239, 241, 252, 52 ou 69) sem mudar os cabeçalhos das colunas.");

        if (modelo == Modelo.Mod246)
        {
            copia.Position = 0;
            var portefolio = await sender.Send(new ImportarPortefolioCommand(copia), cancellationToken);
            return new ResultadoImportacaoModeloDto(Nome(modelo), portefolio.Folhas, portefolio.Avisos);
        }

        await ExigirAsync(Permissao(modelo), cancellationToken);

        List<ResumoFolhaDto> resumos = modelo switch
        {
            Modelo.Mod235 => [await ParceriasAsync(folhas, cancellationToken)],
            Modelo.Mod236 => [await AtivosAsync(folhas, cancellationToken)],
            Modelo.Mod237 => [await ConhecimentoCodificadoAsync(folhas, cancellationToken)],
            Modelo.Mod238 => [await ConhecimentoTacitoAsync(folhas, cancellationToken)],
            Modelo.Mod239 => await IndicadoresAsync(folhas, cancellationToken),
            Modelo.Mod241 => [await FerramentasAsync(folhas, cancellationToken)],
            Modelo.Mod252 => [await LicoesAsync(folhas, cancellationToken)],
            Modelo.Mod52 => await EstrategiaAsync(folhas, cancellationToken),
            _ => [await PlanoAnualAsync(folhas, cancellationToken)],
        };

        await context.SaveChangesAsync(cancellationToken);
        return new ResultadoImportacaoModeloDto(Nome(modelo), resumos, _avisos);
    }

    private static string Nome(Modelo modelo) => modelo switch
    {
        Modelo.Mod246 => "Mod.246 · Portefólio de iniciativas",
        Modelo.Mod235 => "Mod.235 · Acordos de parceria",
        Modelo.Mod236 => "Mod.236 · Ativos intangíveis",
        Modelo.Mod237 => "Mod.237 · Conhecimento codificado",
        Modelo.Mod238 => "Mod.238 · Conhecimento tácito",
        Modelo.Mod239 => "Mod.239 · Indicadores de desempenho",
        Modelo.Mod241 => "Mod.241 · Ferramentas e métodos",
        Modelo.Mod252 => "Mod.252 · Lições aprendidas",
        Modelo.Mod52 => "Mod.52 · Estratégia de inovação",
        _ => "Mod.69 · Plano anual de atividades",
    };

    private static string Permissao(Modelo modelo) => modelo switch
    {
        Modelo.Mod239 => Permissoes.Kpis.Objetivos,
        Modelo.Mod252 => Permissoes.Iniciativas.Gerir,
        Modelo.Mod69 => Permissoes.Registos.GerirPlano,
        _ => Permissoes.Registos.Gerir,
    };

    private async Task ExigirAsync(string permissao, CancellationToken cancellationToken)
    {
        if (!await utilizadores.TemPermissaoAsync(permissao, cancellationToken))
        {
            throw new ForbiddenAccessException();
        }
    }

    /// <summary>Reconhece o modelo pelos nomes das folhas e pelos textos das primeiras linhas.</summary>
    private static Modelo? Detetar(IReadOnlyList<FolhaLida> folhas)
    {
        var nomes = folhas.Select(f => Celulas.Normalizar(f.Nome)).ToHashSet();
        if (nomes.Contains("ideias") && nomes.Contains("projetos"))
        {
            return Modelo.Mod246;
        }

        var textos = folhas
            .SelectMany(f => f.Linhas.Take(15))
            .SelectMany(l => l)
            .OfType<string>()
            .Select(Celulas.Normalizar)
            .ToList();
        bool Tem(params string[] inicios) => textos.Any(t => inicios.Any(i => t.StartsWith(i, StringComparison.Ordinal)));

        if (Tem("planeamento anual de atividades"))
        {
            return Modelo.Mod69;
        }

        if (Tem("plano acao inicial", "plano de acao inicial") || (Tem("indicador") && Tem("objetivo anual")))
        {
            return Modelo.Mod239;
        }

        if (Tem("areas de atuacao"))
        {
            return Modelo.Mod52;
        }

        if (Tem("licao"))
        {
            return Modelo.Mod252;
        }

        if (Tem("identicacao da ferramenta", "identificacao da ferramenta"))
        {
            return Modelo.Mod241;
        }

        if (Tem("detentor"))
        {
            return Modelo.Mod238;
        }

        if (Tem("arquivado em"))
        {
            return Modelo.Mod237;
        }

        if (Tem("data validade", "ativo intelectual"))
        {
            return Modelo.Mod236;
        }

        return Tem("descricao da parceria", "tipo de protocolo") ? Modelo.Mod235 : null;
    }

    /// <summary>A folha com a tabela (a primeira que tem o cabeçalho com o texto-chave).</summary>
    private static (FolhaLida Folha, TabelaModelo Tabela) Tabela(IReadOnlyList<FolhaLida> folhas, string chave)
    {
        foreach (var folha in folhas)
        {
            var tabela = new TabelaModelo(folha, chave);
            if (tabela.Encontrada)
            {
                return (folha, tabela);
            }
        }

        throw new RegraDeNegocioException($"Não encontrei a tabela do modelo (falta a coluna \"{chave}\").");
    }

    private void Avisar(string aviso)
    {
        if (_avisos.Count < 200)
        {
            _avisos.Add(aviso);
        }
    }

    /// <summary>Atividades "Atividades a realizar · Quem · Quando" (uma por linha do Excel); concluída se a data já passou.</summary>
    private AtividadeGestao? Atividade(object?[] linha, TabelaModelo t)
    {
        var descricao = Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("atividades")));
        if (Celulas.SemValor(descricao))
        {
            return null;
        }

        var data = Celulas.Data(TabelaModelo.Valor(linha, t.Coluna("quando")));
        return new AtividadeGestao
        {
            Descricao = Celulas.Cortar(descricao, 1000)!,
            Responsavel = Celulas.Cortar(Celulas.Texto(TabelaModelo.Valor(linha, t.Coluna("quem"))), 200),
            Data = data,
            Concluida = data is { } d && d < Hoje,
        };
    }
}
