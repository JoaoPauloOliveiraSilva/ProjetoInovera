using Innovera.Application.Common.Exceptions;
using Innovera.Application.Iniciativas;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Innovera.Infrastructure.Data;

namespace Innovera.Application.FunctionalTests.Iniciativas;

public class IniciativasTests : TestBase
{
    /// <summary>Um Mod.246 pequeno com o mesmo formato do verdadeiro (cabeçalho na linha 6, linhas de ano a separar).</summary>
    private static MemoryStream Mod246()
    {
        object?[] Vazia() => [];
        var ideias = new object?[][]
        {
            Vazia(),
            [null, "EMPRESA: dstelecom"],
            Vazia(), Vazia(),
            [null, "IDEIAS dstelecom"],
            [null, "ID", "Título", "Autor/Gestor", "Data de submissão", "Origem", "Responsabilidade", "Custo", "Enquadramento", "Benefício",
                "Adeq Técnica", "Incerteza", "TOTAL", "Classificação ", "Classe", "Status "],
            [null, 2025],
            [null, 2764, "Criar mecanismo seguro de abertura de CVP's", "Pedro Monteiro", new DateTime(2025, 12, 9), "Caixa de Inovação",
                "dstelecom ", 3, 4, 3, 3, 1, 3.05, "Aprovada", "Melhoria Contínua"],
            [null, 2780, "Rádio DST", "Leonel Martins", "12/26/2025", "Caixa de Inovação", "dst", "N.A.", "N.A.", "N.A.", "N.A.", "N.A.", "N.A.",
                "Não Aprovada", "Ideia Duplicada", "Já existe"],
            [null, 2026],
            [null, 3004, "Memorial sobre o tempo", "Autor Anónimo", new DateTime(2026, 8, 27), "Caixa de inovação", "dst",
                "N.A.", "N.A.", "N.A.", "N.A.", "N.A.", "N.A."],
        };
        var melhorias = new object?[][]
        {
            ["Data da atualização: 07/08/2026"], Vazia(), Vazia(), ["Oportunidades de Inovação"],
            ["ID", "Título", "Autor / Gestor", "Data de submissão / início", "Origem", "Ponto de situação", "Resultado"],
            [1, "Criar mecanismo seguro de abertura de CVP's", "Pedro Monteiro", new DateTime(2025, 12, 9), "Caixa de inovação", "Em implementação", "Em curso"],
        };
        var projetos = new object?[][]
        {
            ["Data da atualização: 07/08/2026"], Vazia(), Vazia(), ["Projetos de Inovação"],
            ["ID", "Título", "Tipo", "Horizonte", "Gestor", "Data de início", "Data de fim", "Origem", "Ponto de situação"],
            [1, "FWA - Fixed Wireless Access", "Investigação", "H2", "Sérgio Lopes", new DateTime(2019, 3, 1), new DateTime(2022, 12, 1), "Inov. Interna", "Concluído"],
        };

        return XlsxDeTeste.Criar(("Folha1", new[] { Vazia() }), ("Ideias", ideias), ("Projetos", projetos), ("Melhorias Contínuas", melhorias));
    }

    [Test]
    public async Task ImportaOMod246ELigaAMelhoriaAIdeia()
    {
        await TestApp.RunAsAdministratorAsync();

        var resultado = await TestApp.SendAsync(new ImportarPortefolioCommand(Mod246()));

        resultado.Folhas.Single(f => f.Folha == "Ideias").Importadas.ShouldBe(3);
        resultado.Folhas.Single(f => f.Folha == "Projetos").Importadas.ShouldBe(1);
        resultado.Folhas.Single(f => f.Folha == "Melhorias Contínuas").Importadas.ShouldBe(1);

        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var aprovada = await context.Ideias.Include(i => i.Avaliacao).Include(i => i.IniciativaResultante)
            .SingleAsync(i => i.IdCaixaInovacao == 2764);
        aprovada.Estado.ShouldBe(EstadoIdeia.Aprovada);
        aprovada.Responsabilidade.ShouldBe(Responsabilidade.Dstelecom);
        aprovada.Avaliacao!.Nota.ShouldBe(3.05m);
        aprovada.Codigo.ShouldMatch("^[A-Z]{2}[0-9]{2}$");
        aprovada.IniciativaResultante.ShouldBeOfType<MelhoriaContinua>().Estado.ShouldBe(EstadoIniciativa.EmImplementacao);

        var duplicada = await context.Ideias.SingleAsync(i => i.IdCaixaInovacao == 2780);
        duplicada.Estado.ShouldBe(EstadoIdeia.NaoAprovada);
        duplicada.Classe.ShouldBe(ClasseIdeia.IdeiaDuplicada);
        duplicada.DataSubmissao.ShouldBe(new DateTimeOffset(2025, 12, 26, 0, 0, 0, TimeSpan.Zero));

        (await context.Ideias.SingleAsync(i => i.IdCaixaInovacao == 3004)).Estado.ShouldBe(EstadoIdeia.EmAvaliacao);

        var projeto = await context.Iniciativas.OfType<Projeto>().SingleAsync();
        projeto.Codigo.ShouldBe("PRJ-001");
        projeto.Estado.ShouldBe(EstadoIniciativa.Concluida);
        projeto.Horizonte.ShouldBe(Horizonte.H2);
        projeto.TipoProjeto.ShouldBe(TipoProjeto.Investigacao);
        projeto.ResponsavelNome.ShouldBe("Sérgio Lopes");
        projeto.DataInicio.ShouldBe(new DateOnly(2019, 3, 1));

        // Importar outra vez não duplica nada.
        var outraVez = await TestApp.SendAsync(new ImportarPortefolioCommand(Mod246()));
        outraVez.Folhas.Sum(f => f.Importadas).ShouldBe(0);
        outraVez.Folhas.Single(f => f.Folha == "Ideias").JaExistiam.ShouldBe(3);
    }

    [Test]
    public async Task TrabalhadorVeOPortefolioMasNaoImportaNemEdita()
    {
        await TestApp.RunAsAdministratorAsync();
        await TestApp.SendAsync(new ImportarPortefolioCommand(Mod246()));

        await TestApp.RunAsDefaultUserAsync();
        var projetos = await TestApp.SendAsync(new ListarIniciativasQuery(TipoIniciativa.Projeto, null, null));
        projetos.ShouldHaveSingleItem().Responsavel.ShouldBe("Sérgio Lopes");

        var detalhe = await TestApp.SendAsync(new ObterIniciativaQuery(projetos[0].Id));
        detalhe.PodeEditar.ShouldBeFalse();
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new ImportarPortefolioCommand(Mod246())));
        await Should.ThrowAsync<ForbiddenAccessException>(() => TestApp.SendAsync(new RegistarPontoSituacaoCommand(projetos[0].Id, "x")));
    }

    [Test]
    public async Task EquipaCriaIniciativaERegistaPontoDeSituacao()
    {
        await TestApp.RunAsEquipaInovacaoAsync();

        var id = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Desafio, new DadosIniciativa
        {
            Titulo = "Passadeiras inteligentes",
            Responsavel = "Sofia Martins",
            DataInicio = new DateOnly(2026, 10, 12),
        }));
        await TestApp.SendAsync(new RegistarPontoSituacaoCommand(id, "Reunião de arranque feita."));

        var detalhe = await TestApp.SendAsync(new ObterIniciativaQuery(id));
        detalhe.Resumo.Codigo.ShouldBe("DES-001");
        detalhe.Resumo.Tipo.ShouldBe(TipoIniciativa.Desafio);
        detalhe.Resumo.PontoSituacao.ShouldBe("Reunião de arranque feita.");
        detalhe.Historico.ShouldHaveSingleItem();
        detalhe.PodeEditar.ShouldBeTrue();

        await Should.ThrowAsync<ValidationException>(() => TestApp.SendAsync(new AtualizarIniciativaCommand(id, new DadosIniciativa
        {
            Titulo = "Passadeiras inteligentes",
            DataInicio = new DateOnly(2026, 10, 12),
            DataFim = new DateOnly(2026, 1, 1),
        })));
    }

    [Test]
    public async Task FicheiroQueNaoEExcelDaUmErroClaro()
    {
        await TestApp.RunAsAdministratorAsync();

        await Should.ThrowAsync<Domain.Exceptions.RegraDeNegocioException>(() =>
            TestApp.SendAsync(new ImportarPortefolioCommand(new MemoryStream("isto não é um Excel"u8.ToArray()))));
    }
}
