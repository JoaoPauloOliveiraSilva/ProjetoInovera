using Innovera.Application.Charters;
using Innovera.Application.Common.Exceptions;
using Innovera.Application.Importacoes;
using Innovera.Application.Iniciativas;
using Innovera.Application.Kpis;
using Innovera.Application.Registos;
using Innovera.Domain.Enums;
using Innovera.Domain.Exceptions;
using Innovera.Infrastructure.Services;

namespace Innovera.Application.FunctionalTests.Importacoes;

/// <summary>Importação dos modelos Excel da dstelecom com ficheiros montados no teste (iguais em forma aos reais, sem os dados).</summary>
public class ImportarModelosTests : TestBase
{
    private static Task<ResultadoImportacaoModeloDto> Importar(XlsxDeTeste xlsx) => TestApp.SendAsync(new ImportarModeloCommand(xlsx.Gerar()));

    [Test]
    public async Task ParceriasReconhecidasPeloConteudoESemRepetirAoImportarOutraVez()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        XlsxDeTeste Ficheiro() => new XlsxDeTeste().Folha("Gestão de acordos de parcer")
            .Linha(2, "Data da atualização:", new DateOnly(2025, 9, 11))
            .Linha(4, "Entidade", "Descrição da parceria", "Protocolo estabelecido", "Tipo de protocolo", "Duração do protocolo / parceria",
                "Inclui gestão de PI?", "Observações")
            .Linha(5, "Universidade do Minho", "Cátedra", "Sim", "Investigação partilhada", "2020 - 2024", "Sim", "Terminado")
            .Linha(6, "K2D", "Contrato de consórcio ", "«", "Co-Promoção", "01.07.2020 - 30.06.2023", "Sim", "Terminado")
            .Linha(7, "INESC", "NDA DAS", "Sim", "Acordo de confidencialidade", "28.10.2025 - Sem fim", "Não", "Em curso");

        var resultado = await Importar(Ficheiro());
        resultado.Modelo.ShouldStartWith("Mod.235");
        resultado.Folhas.ShouldHaveSingleItem().Importadas.ShouldBe(3);

        var acordos = await TestApp.SendAsync(new ListarAcordosQuery(null));
        var k2d = acordos.Single(a => a.Parceiro == "K2D");
        k2d.ProtocoloEstabelecido.ShouldBeTrue();
        k2d.TipoProtocolo.ShouldBe(TipoProtocolo.CoPromocao);
        k2d.DataInicio.ShouldBe(new DateOnly(2020, 7, 1));
        k2d.DataFim.ShouldBe(new DateOnly(2023, 6, 30));
        k2d.Estado.ShouldBe(EstadoAcordo.Terminado);
        var um = acordos.Single(a => a.Parceiro == "Universidade do Minho");
        um.TipoParceiro.ShouldBe(TipoParceiro.Academico);
        um.DataInicio.ShouldBe(new DateOnly(2020, 1, 1));
        um.DataFim.ShouldBe(new DateOnly(2024, 12, 31));
        var inesc = acordos.Single(a => a.Parceiro == "INESC");
        inesc.TipoProtocolo.ShouldBe(TipoProtocolo.Confidencialidade);
        inesc.DataFim.ShouldBeNull();
        inesc.Estado.ShouldBe(EstadoAcordo.EmCurso);
        inesc.IncluiGestaoPropriedadeIntelectual.ShouldBeFalse();

        var outraVez = (await Importar(Ficheiro())).Folhas.ShouldHaveSingleItem();
        (outraVez.Importadas, outraVez.JaExistiam).ShouldBe((0, 3));
    }

    [Test]
    public async Task ConhecimentoCodificadoComCabecalhoEmDuasLinhasEAtividades()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var xlsx = new XlsxDeTeste().Folha("Folha1")
            .Linha(1, "Data da atualização: 16/09/2026")
            .Linha(3, "Data", "Descrição do conhecimento a gerir", null, null, null, "Modelo de gestão", null, null, "Resultado obtido")
            .Linha(4, null, "Documento", "Arquivado em", "Modo de consulta", "Descrição do conhecimento", "Atividades a realizar?", "Quem", "Quando")
            .Linha(5, new DateOnly(2023, 6, 1), "Relatório de fecho K2D", new DateOnly(2023, 6, 1), "OneDrive dstelecom inov", "Relatório final",
                "Apresentação aos trabalhadores", "João Faria", new DateOnly(2023, 6, 1), "Síntese e partilha");

        (await Importar(xlsx)).Modelo.ShouldStartWith("Mod.237");

        var entrada = (await TestApp.SendAsync(new ListarConhecimentoCodificadoQuery(null, null))).ShouldHaveSingleItem();
        entrada.Documento.ShouldBe("Relatório de fecho K2D");
        entrada.ModoConsulta.ShouldBe("OneDrive dstelecom inov");
        entrada.Descricao.ShouldBe("Relatório final");
        entrada.Resultado.ShouldBe("Síntese e partilha");
        var atividade = entrada.Atividades.ShouldNotBeNull().ShouldHaveSingleItem();
        atividade.Descricao.ShouldBe("Apresentação aos trabalhadores");
        atividade.Responsavel.ShouldBe("João Faria");
        atividade.Concluida.ShouldBeTrue();
    }

    [Test]
    public async Task PlanoAnualPelasCoresDoCalendario()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        // Janeiro de 2026 começa à quinta: o dia 1 está na coluna H, como no Mod.69.
        var xlsx = new XlsxDeTeste().Folha("Folha1")
            .Celula("B1", "Planeamento Anual de Atividades\nAno: 2026")
            .Celula("B7", "janeiro").Celula("H7", 1, "FFE699").Celula("I7", 2).Celula("J7", 3, "D9D9D9")
            .Celula("L7", 5, "00B0F0").Celula("M7", 6, "00B0F0", "tema:9:0").Celula("N7", 7, "123456")
            .Celula("B8", "fevereiro").Celula("D8", 1).Celula("E8", 2, "00B0F0")
            .Celula("B22", "Responsáveis").Celula("E22", "Cor").Celula("F22", "Descrição das Atividades")
            .Celula("B23", "João Faria e DI").Celula("E23", null, "00B0F0").Celula("F23", "GetTogether - Inovação")
            .Celula("U22", "Responsáveis").Celula("X22", "Cor").Celula("Y22", "Descrição das Atividades")
            .Celula("U23", "Matilde Rocha").Celula("X23", null, "tema:9:0").Celula("Y23", "i9meetings dstgroup")
            .Celula("X24", null, "FFE699").Celula("Y24", "Feriado");

        var resultado = await Importar(xlsx);
        resultado.Modelo.ShouldStartWith("Mod.69");
        resultado.Folhas.ShouldHaveSingleItem().Importadas.ShouldBe(4);
        resultado.Avisos.ShouldContain(a => a.Contains("#123456"));

        var plano = await TestApp.SendAsync(new ListarPlanoAnualQuery(2026));
        plano.Where(a => a.Titulo == "GetTogether - Inovação").Select(a => a.DataInicio).Order()
            .ShouldBe(new[] { new DateOnly(2026, 1, 5), new DateOnly(2026, 1, 6), new DateOnly(2026, 2, 2) });
        var reuniao = plano.Single(a => a.Titulo == "i9meetings dstgroup");
        reuniao.DataInicio.ShouldBe(new DateOnly(2026, 1, 6));
        reuniao.Cor.ShouldBe("#70AD47");
        reuniao.Descricao.ShouldBe("Responsáveis: Matilde Rocha");
        plano.ShouldNotContain(a => a.Titulo == "Feriado");

        (await Importar(xlsx)).Folhas.ShouldHaveSingleItem().JaExistiam.ShouldBe(4);
    }

    [Test]
    public async Task IndicadoresValoresObjetivosEPlanoDeAcao()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var xlsx = new XlsxDeTeste().Folha("Mod.239.dstl_IDInovacao")
            .Linha(3, "Data de atualização: 21/01/2026")
            .Linha(5, "Categoria", "ID", "Indicador", 2024, null, null, "2025 - 1ºS", null, null, "2025 - Jan a Out", null, null,
                "Plano Ação Inicial", null, null, null, null, "Acompanhamento")
            .Linha(6, null, null, null, "Abs", "Rel", "Objetivo\nAnual", "Abs", "Rel", "Objetivo\nAnual", "Abs", "Rel", "Objetivo\nAnual",
                "Data de criação", "Descrição", "Recursos", "Responsável", "Prazo", "Análise dos Resultados", "Plano de ação ")
            .Linha(7, "Criatividade", 1, "Ideias submetidas", 33, null, 13, 22, null, "≥ 30", 47, null, "≥ 30",
                "31.01.2025", "Criar uma cultura de inovação", "Eventos", "João Faria", "31.12.2025", "Abaixo do objetivo", "Mais ID Talks")
            .Linha(8, null, 2, "Ideias aprovadas", null, null, null, 18, 0.818, "≥ 80%")
            .Linha(9, "Parcerias", 26, "% empresarial", null, null, null, 0.8889, null, null);

        var resultado = await Importar(xlsx);
        resultado.Modelo.ShouldStartWith("Mod.239");
        resultado.Avisos.ShouldContain(a => a.Contains("Jan a Out"));

        // 2024 e o 1.º semestre de 2025 já fecharam: valem os valores do Mod.239.
        var de2024 = (await TestApp.SendAsync(new ObterIndicadoresQuery(2024))).Kpis.Single(k => k.Numero == 1);
        de2024.Valor.ShouldBe(33m);
        de2024.Objetivo.ShouldBe("13");
        var semestre = (await TestApp.SendAsync(new ObterIndicadoresQuery(2025, PeriodoKpi.PrimeiroSemestre))).Kpis;
        semestre.Single(k => k.Numero == 1).Valor.ShouldBe(22m);
        semestre.Single(k => k.Numero == 2).Valor.ShouldBe(18m);
        semestre.Single(k => k.Numero == 26).Valor.ShouldBe(88.9m);

        var plano = await TestApp.SendAsync(new ObterPlanoKpiQuery(1, 2025));
        plano.Objetivo.ShouldBe("≥ 30");
        plano.Acoes.Count.ShouldBe(2);
        var inicial = plano.Acoes.Single(a => a.Tipo == TipoAcaoKpi.PlanoInicial);
        inicial.Descricao.ShouldBe("Criar uma cultura de inovação");
        inicial.Recursos.ShouldBe("Eventos · Responsável: João Faria");
        inicial.Prazo.ShouldBe(new DateOnly(2025, 12, 31));
        var acompanhamento = plano.Acoes.Single(a => a.Tipo == TipoAcaoKpi.Acompanhamento);
        acompanhamento.AnaliseResultados.ShouldBe("Abaixo do objetivo");
        acompanhamento.Descricao.ShouldBe("Mais ID Talks");

        (await Importar(xlsx)).Folhas.Sum(f => f.Importadas).ShouldBe(0);
    }

    [Test]
    public async Task LicoesFicamNoCharterDoProjetoComONomeParecido()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "AI Poles" }));
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Desafio, new DadosIniciativa { Titulo = "Inteligência Artificial" }));
        await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "Postes Compósitos" }));
        var xlsx = new XlsxDeTeste().Folha("Folha1")
            .Linha(3, "Data da atualização:", null, " 16/09/2026")
            .Linha(6, "ID", "Categroia ", "Iniciativa ", "Lição/Descrição", "Impacto na Iniciativa")
            .Linha(8, 1, "Estratégica", "IA", "IA como ferramenta de apoio", "Melhora a cultura")
            .Linha(9, null, "Técnica", "AIPoles", "Qualidade das imagens", "Dataset validado")
            .Linha(10, null, "Gestão", "Postes Compoósito", "Know-how", null)
            .Linha(11, null, "Operacional ", "Projeto que não existe", "Lição perdida", null);

        var resultado = await Importar(xlsx);
        resultado.Modelo.ShouldStartWith("Mod.252");
        var resumo = resultado.Folhas.ShouldHaveSingleItem();
        resumo.Importadas.ShouldBe(3);
        resumo.Ignoradas.ShouldBe(1);
        resultado.Avisos.ShouldContain(a => a.Contains("Projeto que não existe"));

        var licoes = await TestApp.SendAsync(new ListarLicoesQuery(null, null));
        licoes.Count.ShouldBe(3);
        var aiPoles = licoes.Single(l => l.IniciativaId == projeto);
        aiPoles.Categoria.ShouldBe(CategoriaLicao.Tecnica);
        aiPoles.Licao.ShouldBe("Qualidade das imagens");
        aiPoles.Impacto.ShouldBe("Dataset validado");
        licoes.Single(l => l.Licao == "IA como ferramenta de apoio").IniciativaTitulo.ShouldBe("Inteligência Artificial");
    }

    [Test]
    public async Task EstrategiaComAreaRepetidaParaBaixoEKpisEmTexto()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        var projeto = await TestApp.SendAsync(new CriarIniciativaCommand(TipoIniciativa.Projeto, new DadosIniciativa { Titulo = "DASensing" }));
        var xlsx = new XlsxDeTeste().Folha("2026")
            .Linha(1, "Ano:", 2026)
            .Linha(4, "Áreas de atuação", "Abordagens e/ou Aplicações Específicas", "Plano de Ação", "KPI associado", "Duração (meses)",
                "Nº RHs Associados", "Horas/mês dos RHs", "Dedicação horária total", "Recursos extra", "Horizonte")
            .Linha(5, "Tecnologia de Fibra Ótica", "Monitorização da rede", "Desenvolver o Projeto DASensing", 35, 12, 8, 8, 768, "Consultoria INESC", "H2")
            .Linha(6, null, "Mobile 5G", "Desenvolver o projeto 5G.RURAL", "14 e 36", 12, "2,4M€ proj budget", null, null, null, "H3");

        (await Importar(xlsx)).Folhas.ShouldHaveSingleItem().Importadas.ShouldBe(2);

        var linhas = await TestApp.SendAsync(new ListarEstrategiaQuery(2026));
        var das = linhas.Single(l => l.PlanoAcao == "Desenvolver o Projeto DASensing");
        das.IniciativaId.ShouldBe(projeto);
        das.DedicacaoTotalHoras.ShouldBe(768m);
        das.Horizonte.ShouldBe(Horizonte.H2);
        var rural = linhas.Single(l => l.PlanoAcao == "Desenvolver o projeto 5G.RURAL");
        rural.AreaAtuacao.ShouldBe("Tecnologia de Fibra Ótica");
        rural.Kpis.Select(k => k.Numero).ShouldBe(new[] { 14, 36 });
        rural.NumeroRecursosHumanos.ShouldBeNull();
        rural.RecursosExtra.ShouldBe("2,4M€ proj budget");
    }

    [Test]
    public async Task FicheiroQueNaoEUmModeloEPermissoes()
    {
        await TestApp.RunAsEquipaInovacaoAsync();
        await Should.ThrowAsync<RegraDeNegocioException>(() => Importar(new XlsxDeTeste().Folha("Folha1").Linha(1, "Nome", "Morada")));

        await TestApp.RunAsDefaultUserAsync();
        await Should.ThrowAsync<ForbiddenAccessException>(() => Importar(new XlsxDeTeste().Folha("Folha1")
            .Linha(6, "Data", "Empresa ", "Descrição do conhecimento a gerir")
            .Linha(7, null, null, "Detentor", "Descrição do conhecimento tácito")
            .Linha(8, new DateOnly(2025, 7, 14), "dstelecom", "João Faria", "Conhecimento dos projetos")));
    }

    [Test]
    public void LeitorDeExcelLeAsCoresDoFundo()
    {
        var xlsx = new XlsxDeTeste().Folha("Folha1")
            .Celula("A1", "rgb", "C00000")
            .Celula("A2", "tema", "tema:4:0")
            .Celula("A3", "tema mais claro", "tema:4:0.4")
            .Celula("A4", "gradiente", "00B0F0", "tema:9:0")
            .Celula("A5", "sem fundo");

        var folha = new LeitorXlsx().Ler(xlsx.Gerar()).ShouldHaveSingleItem();

        folha.CoresDe(0, 0).ShouldBe(new[] { "C00000" });
        folha.CoresDe(1, 0).ShouldBe(new[] { "4472C4" });
        folha.CoresDe(2, 0).ShouldBe(new[] { "8FAADC" });
        folha.CoresDe(3, 0).ShouldBe(new[] { "00B0F0", "70AD47" });
        folha.CoresDe(4, 0).ShouldBeEmpty();
    }
}
