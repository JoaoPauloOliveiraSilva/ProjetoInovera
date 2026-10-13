using Innovera.Domain.Constants;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Domain.ValueObjects;
using Innovera.Infrastructure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Innovera.Infrastructure.Data;

public static class InitialiserExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();

        await initialiser.InitialiseAsync();
        await initialiser.SeedAsync();
    }
}

/// <summary>
/// Aplica as migrações e cria os perfis, a conta de administrador e (em desenvolvimento) dados de exemplo.
/// </summary>
public class ApplicationDbContextInitialiser
{
    /// <summary>Password das contas de exemplo criadas em desenvolvimento.</summary>
    public const string PasswordExemplo = "Innovera2026!";

    private readonly ILogger<ApplicationDbContextInitialiser> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly TimeProvider _relogio;

    public ApplicationDbContextInitialiser(ILogger<ApplicationDbContextInitialiser> logger, ApplicationDbContext context,
        UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration, TimeProvider relogio)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _relogio = relogio;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao aplicar as migrações da base de dados.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao preencher a base de dados.");
            throw;
        }
    }

    public async Task TrySeedAsync()
    {
        await GarantirPerfisAsync();

        // Conta inicial da equipa de Inovação. Fora do desenvolvimento, a password tem de vir da configuração.
        var emailAdmin = _configuration["ContaAdministrador:Email"] ?? "admin@innovera.local";
        var passwordAdmin = _configuration["ContaAdministrador:Password"]
            ?? (_configuration.GetValue("DadosExemplo", false) ? PasswordExemplo : null);
        if (passwordAdmin is null)
        {
            _logger.LogWarning("Sem ContaAdministrador:Password configurada: a conta de administrador não foi criada.");
            return;
        }

        await GarantirContaAsync(emailAdmin, "Administrador", "dstelecom, s.a.", Roles.Administrador, passwordAdmin);

        if (_configuration.GetValue("DadosExemplo", false) && !await _context.Ideias.AnyAsync())
        {
            await CriarDadosExemploAsync();
        }
    }

    /// <summary>
    /// Cria os perfis do sistema com as permissões por omissão. As permissões só se põem quando o perfil é criado:
    /// depois quem manda é o backoffice.
    /// </summary>
    private async Task GarantirPerfisAsync()
    {
        // Bases de dados da primeira versão: o perfil "GestorProjeto" era a equipa de Inovação.
        var antigo = await _roleManager.FindByNameAsync("GestorProjeto");
        if (antigo is not null && !await _roleManager.RoleExistsAsync(Roles.EquipaInovacao))
        {
            await _roleManager.SetRoleNameAsync(antigo, Roles.EquipaInovacao);
            await _roleManager.UpdateAsync(antigo);
            await DarPermissoesPorOmissaoAsync(antigo);
        }

        foreach (var nome in Roles.DoSistema)
        {
            if (await _roleManager.RoleExistsAsync(nome))
            {
                continue;
            }

            var perfil = new IdentityRole(nome);
            await _roleManager.CreateAsync(perfil);
            await DarPermissoesPorOmissaoAsync(perfil);
        }

        await DarPermissoesNovasAsync();
    }

    /// <summary>
    /// Permissões que apareceram numa versão nova da aplicação: os perfis do sistema recebem as que lhes cabem
    /// por omissão, uma única vez (fica registado no perfil Administrador).
    /// </summary>
    private async Task DarPermissoesNovasAsync()
    {
        var administrador = await _roleManager.FindByNameAsync(Roles.Administrador);
        if (administrador is null)
        {
            return;
        }

        var conhecidas = (await _roleManager.GetClaimsAsync(administrador))
            .Where(c => c.Type == Permissoes.TipoClaimConhecida)
            .Select(c => c.Value)
            .ToHashSet();

        foreach (var permissao in Permissoes.Todas.Select(p => p.Codigo).Where(p => !conhecidas.Contains(p)))
        {
            foreach (var (nomePerfil, porOmissao) in Permissoes.PorOmissao)
            {
                var perfil = await _roleManager.FindByNameAsync(nomePerfil);
                if (perfil is null || !porOmissao.Contains(permissao))
                {
                    continue;
                }

                var atuais = await _roleManager.GetClaimsAsync(perfil);
                if (!atuais.Any(c => c.Type == Permissoes.TipoClaim && c.Value == permissao))
                {
                    await _roleManager.AddClaimAsync(perfil, new System.Security.Claims.Claim(Permissoes.TipoClaim, permissao));
                }
            }

            await _roleManager.AddClaimAsync(administrador, new System.Security.Claims.Claim(Permissoes.TipoClaimConhecida, permissao));
        }
    }

    private async Task DarPermissoesPorOmissaoAsync(IdentityRole perfil)
    {
        if (!Permissoes.PorOmissao.TryGetValue(perfil.Name!, out var permissoes))
        {
            return;
        }

        var atuais = (await _roleManager.GetClaimsAsync(perfil)).Select(c => c.Value).ToHashSet();
        foreach (var permissao in permissoes.Where(p => !atuais.Contains(p)))
        {
            await _roleManager.AddClaimAsync(perfil, new System.Security.Claims.Claim(Permissoes.TipoClaim, permissao));
        }
    }

    private async Task<Utilizador> GarantirContaAsync(string email, string nome, string empresa, string role, string password = PasswordExemplo)
    {
        var conta = await _userManager.FindByEmailAsync(email);
        if (conta is null)
        {
            conta = new ApplicationUser { UserName = email, Email = email, EmailConfirmed = true };
            var resultado = await _userManager.CreateAsync(conta, password);
            if (!resultado.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", resultado.Errors.Select(e => e.Description)));
            }
        }

        if (!await _userManager.IsInRoleAsync(conta, role))
        {
            await _userManager.AddToRoleAsync(conta, role);
        }

        var utilizador = await _context.Utilizadores.FirstOrDefaultAsync(u => u.IdentityId == conta.Id);
        if (utilizador is null)
        {
            utilizador = new Utilizador
            {
                IdentityId = conta.Id,
                Email = email,
                Nome = nome,
                Empresa = empresa,
            };
            _context.Utilizadores.Add(utilizador);
            await _context.SaveChangesAsync();
        }

        return utilizador;
    }

    /// <summary>Ideias fictícias para a equipa ver a plataforma a funcionar (só em desenvolvimento).</summary>
    private async Task CriarDadosExemploAsync()
    {
        var agora = _relogio.GetUtcNow();
        var ricardo = await GarantirContaAsync("ricardo.lopes@exemplo.pt", "Ricardo Lopes", "dstelecom, s.a.", Roles.Trabalhador);
        var marta = await GarantirContaAsync("marta.sousa@exemplo.pt", "Marta Sousa", "fiber t, s.a.", Roles.Trabalhador);
        var carla = await GarantirContaAsync("carla.ferreira@exemplo.pt", "Carla Ferreira", "dstelecom, s.a.", Roles.Trabalhador);
        var ines = await GarantirContaAsync("ines.correia@exemplo.pt", "Inês Correia", "dstgroup", Roles.Trabalhador);
        var joana = await GarantirContaAsync("joana.ribeiro@exemplo.pt", "Joana Ribeiro", "dstelecom, s.a.", Roles.Trabalhador);
        var tiago = await GarantirContaAsync("tiago.araujo@exemplo.pt", "Tiago Araújo", "dstelecom, s.a.", Roles.Trabalhador);
        var beatriz = await GarantirContaAsync("beatriz.pinto@exemplo.pt", "Beatriz Pinto", "dstgroup", Roles.Trabalhador);
        var andre = await GarantirContaAsync("andre.costa@exemplo.pt", "André Costa", "dstelecom, s.a.", Roles.Trabalhador);
        await GarantirContaAsync("sofia.martins@exemplo.pt", "Sofia Martins", "dstelecom, s.a.", Roles.EquipaInovacao);
        var todos = new[] { ricardo, marta, carla, ines, joana, tiago, beatriz, andre };

        Ideia Nova(Utilizador? autor, TipoIdeia tipo, string titulo, string descricao, int diasAtras)
        {
            var ideia = new Ideia
            {
                Tipo = tipo,
                Titulo = titulo,
                Descricao = descricao,
                AutorId = autor?.Id,
                Anonima = autor is null,
                Origem = "Plataforma",
            };
            ideia.AceitarTermos(agora.AddDays(-diasAtras));
            ideia.Submeter(agora.AddDays(-diasAtras));
            return ideia;
        }

        async Task GuardarAsync(Ideia ideia)
        {
            var numero = await _context.Database
                .SqlQueryRaw<int>($"SELECT nextval('{ApplicationDbContext.SequenciaIdeias}')::int AS \"Value\"")
                .SingleAsync();
            ideia.AtribuirNumero(numero);
            _context.Ideias.Add(ideia);
            await _context.SaveChangesAsync();
        }

        void Discussao(Ideia ideia, int diasAtras, int likes)
        {
            ideia.IniciarDiscussao(agora.AddDays(-diasAtras));
            foreach (var u in todos.Take(likes))
            {
                ideia.AdicionarLike(u.Id, agora.AddDays(-diasAtras + 1));
            }
        }

        // Ideias já decididas (avaliadas com a CE)
        var olt = Nova(andre, TipoIdeia.NovoProdutoServicoDetalhado, "Manutenção preditiva das OLT com IA",
            "Usar os registos dos equipamentos para prever falhas antes de afetarem os clientes.", 75);
        olt.ModeloNegocio = "Reduz deslocações e penalizações por falhas.";
        Discussao(olt, 74, 6);
        olt.FecharDiscussao();
        olt.RegistarAvaliacao(new AvaliacaoIdeia
        {
            Custo = EscalaDeAvaliacao.Nivel2, Enquadramento = EscalaDeAvaliacao.Nivel3, Beneficio = EscalaDeAvaliacao.Nivel3,
            AdequacaoTecnica = EscalaDeAvaliacao.Nivel3, Incerteza = EscalaDeAvaliacao.Nivel2,
            DataReuniaoCE = DateOnly.FromDateTime(agora.AddDays(-35).UtcDateTime),
        }, PesosAvaliacao.Mod246);
        olt.DefinirClasse(ClasseIdeia.Projeto);
        olt.Status = "Será desenvolvida como projeto de inovação interna.";
        await GuardarAsync(olt);
        var hoje = DateOnly.FromDateTime(agora.UtcDateTime);
        var projetoOlt = new Projeto
        {
            Codigo = olt.Codigo, Titulo = olt.Titulo, Descricao = olt.Descricao, IdeiaOrigemId = olt.Id,
            ResponsavelId = andre.Id, Estado = EstadoIniciativa.EmCurso, TipoProjeto = TipoProjeto.Inovacao,
            Horizonte = Horizonte.H2, Origem = "Inov. Interna", DataInicio = hoje.AddDays(-40), DataFim = hoje.AddMonths(9),
            PontoSituacao = "Dados históricos recolhidos; a começar o treino do modelo.",
        };
        projetoOlt.Links.Add(new LinkDocumento
        {
            Titulo = "Proposta aprovada pela CE", Url = "https://dstelecom.sharepoint.com/sites/inovacao/OLT-proposta.pdf",
        });
        _context.Iniciativas.Add(projetoOlt);
        await _context.SaveChangesAsync();
        await CriarCharterExemploAsync(projetoOlt, hoje);

        var bebedouros = Nova(tiago, TipoIdeia.Melhoria, "Bebedouros nas obras", "Instalar bebedouros portáteis nas frentes de obra no verão.", 80);
        Discussao(bebedouros, 79, 2);
        bebedouros.FecharDiscussao();
        bebedouros.RegistarAvaliacao(new AvaliacaoIdeia
        {
            Custo = EscalaDeAvaliacao.Nivel0, Enquadramento = EscalaDeAvaliacao.Nivel0, Beneficio = EscalaDeAvaliacao.Nivel0,
            AdequacaoTecnica = EscalaDeAvaliacao.Nivel0, Incerteza = EscalaDeAvaliacao.Nivel0,
        }, PesosAvaliacao.Mod246);
        bebedouros.DefinirClasse(ClasseIdeia.IdeiaDuplicada);
        await GuardarAsync(bebedouros);

        // Em avaliação (discussão terminada)
        var sensores = Nova(tiago, TipoIdeia.NovoProdutoServicoDetalhado, "Sensores de ocupação nas salas de reunião",
            "Sensores simples que mostram no calendário se a sala está mesmo ocupada.", 40);
        Discussao(sensores, 39, 5);
        sensores.FecharDiscussao();
        await GuardarAsync(sensores);

        var kit = Nova(beatriz, TipoIdeia.Melhoria, "Kit de boas-vindas digital",
            "Substituir o manual impresso de acolhimento por um kit digital com vídeos curtos.", 42);
        Discussao(kit, 41, 3);
        kit.FecharDiscussao();
        kit.Responsabilidade = Responsabilidade.Dst;
        await GuardarAsync(kit);

        // Em discussão
        var discussao = new (Utilizador? Autor, TipoIdeia Tipo, string Titulo, string Descricao, int Dias, int Likes)[]
        {
            (joana, TipoIdeia.Melhoria, "Oferta de bilhetes de museu no aniversário do colaborador",
                "Oferecer a cada colaborador um bilhete para um museu da cidade no dia do aniversário.", 15, 7),
            (null, TipoIdeia.Melhoria, "Sugestão de melhoria de espaço - Copa",
                "Reorganizar a copa do piso 1 com mais lugares sentados e uma zona para refeições rápidas.", 15, 4),
            (ines, TipoIdeia.Melhoria, "Gestão partilhada dos carregamentos de veículos elétricos",
                "Uma aplicação simples para reservar os postos de carregamento do parque.", 8, 3),
            (carla, TipoIdeia.NovoProdutoServicoSimples, "Academia SAP",
                "Criar um percurso de formação interna em SAP para novos colaboradores das áreas administrativas.", 8, 3),
            (ricardo, TipoIdeia.Melhoria, "Informação de Recrutamento",
                "Uma empresa da concorrência usa uma forma de publicidade que pode ser útil para divulgarmos as vagas de emprego do grupo.", 3, 2),
        };
        foreach (var (autor, tipo, titulo, descricao, dias, likes) in discussao)
        {
            var ideia = Nova(autor, tipo, titulo, descricao, dias);
            Discussao(ideia, dias, likes);
            if (titulo == "Informação de Recrutamento")
            {
                ideia.AdicionarComentario(marta.Id, "Excelente ideia, sugeria até incluir um QR code para facilitar o acesso ao site de recrutamento do grupo.", agora.AddHours(-2));
            }

            if (titulo == "Academia SAP")
            {
                ideia.AdicionarComentario(joana.Id, "Seria muito útil para quem entra no grupo.", agora.AddDays(-5));
            }

            await GuardarAsync(ideia);
        }

        // À espera da validação da equipa de Inovação
        var pendente = Nova(marta, TipoIdeia.Melhoria, "Lugares de estacionamento para boleias",
            "Reservar lugares perto da entrada para quem vem de boleia com colegas.", 1);
        await GuardarAsync(pendente);

        await _context.SaveChangesAsync();
        await CriarRegistosExemploAsync(DateOnly.FromDateTime(agora.UtcDateTime));
        _logger.LogInformation("Dados de exemplo criados (contas com a password {Password}).", PasswordExemplo);
    }

    /// <summary>
    /// Project Charter de exemplo, já preenchido, para a demonstração: tarefas (uma atrasada), milestones (o M1 com
    /// relatório, que entra no conhecimento codificado), riscos (um sem plano de ação), orçamento, conformidade e uma lição.
    /// </summary>
    private async Task CriarCharterExemploAsync(Projeto projeto, DateOnly hoje)
    {
        var charter = new ProjectCharter
        {
            Iniciativa = projeto,
            SupervisorNome = "Rui Dias",
            Fase = "Fase de desenvolvimento",
            Classe = "Inovação interna / Projeto",
            ContextoJustificacao = "As falhas das OLT só são detetadas quando os clientes reclamam, o que obriga a deslocações de emergência.",
            BusinessCase = "Menos deslocações urgentes, menos clientes afetados e menos penalizações nos contratos.",
            DecisaoAprovacao = $"Aprovado pela CE a {hoje.AddDays(-45):dd/MM/yyyy}",
            Objetivos = "Prever 70% das falhas das OLT com 48 horas de antecedência\nIntegrar os alertas no NOC",
            ResultadosEsperados = "Modelo de previsão em produção em 2 regiões\nPainel de alertas usado pelo NOC",
        };

        var m1 = new Entrega
        {
            Charter = charter, Tipo = TipoEntrega.Milestone, Codigo = "M1", Descricao = "Dados históricos das OLT limpos e documentados",
            Prazo = hoje.AddDays(-5), Estado = EstadoTrabalho.EmCurso, MeioVerificacao = "Relatório", Controlo = "120 h de RH, sem desvios",
        };
        var m2 = new Entrega
        {
            Charter = charter, Tipo = TipoEntrega.Milestone, Codigo = "M2", Descricao = "Modelo validado numa região",
            Prazo = hoje.AddDays(60), Estado = EstadoTrabalho.Planeado, MeioVerificacao = "Relatório",
        };
        var e1 = new Entrega
        {
            Charter = charter, Tipo = TipoEntrega.Entregavel, Codigo = "E1", Descricao = "Painel de alertas para o NOC",
            Prazo = hoje.AddDays(150), Estado = EstadoTrabalho.Planeado, MeioVerificacao = "Demonstração",
        };
        charter.Entregas.Add(m1);
        charter.Entregas.Add(m2);
        charter.Entregas.Add(e1);

        charter.Tarefas.Add(new TarefaWbs
        {
            Charter = charter, CodigoWbs = "1", Nome = "Gestão do projeto", Tipo = TipoTarefa.Gestao, Estado = EstadoTrabalho.EmCurso,
            InicioPlaneado = projeto.DataInicio, FimPlaneado = projeto.DataFim, InicioReal = projeto.DataInicio, ResponsavelNome = "André Costa",
        });
        charter.Tarefas.Add(new TarefaWbs
        {
            Charter = charter, CodigoWbs = "2.1", Nome = "Recolha dos registos das OLT", Tipo = TipoTarefa.Dados, Estado = EstadoTrabalho.EmCurso,
            InicioPlaneado = hoje.AddDays(-35), FimPlaneado = hoje.AddDays(-3), InicioReal = hoje.AddDays(-33), ResponsavelNome = "Inês Correia",
            Milestone = m1,
        });
        charter.Tarefas.Add(new TarefaWbs
        {
            Charter = charter, CodigoWbs = "2.2", Nome = "Treino do modelo de previsão", Tipo = TipoTarefa.Tecnica, Estado = EstadoTrabalho.Planeado,
            InicioPlaneado = hoje.AddDays(5), FimPlaneado = hoje.AddDays(55), ResponsavelNome = "Tiago Araújo", Milestone = m2,
        });

        charter.Riscos.Add(new Risco
        {
            Charter = charter, Numero = 1, Data = hoje.AddDays(-38), Descricao = "Registos incompletos em equipamentos antigos",
            Impacto = NivelImpacto.Alto, Probabilidade = NivelProbabilidade.PoucoProvavel, ResponsavelNome = "Inês Correia", Prazo = hoje.AddDays(20),
        });
        charter.Riscos.Add(new Risco
        {
            Charter = charter, Numero = 2, Data = hoje.AddDays(-38), Descricao = "Equipa do NOC sem tempo para testar os alertas",
            Impacto = NivelImpacto.Medio, Probabilidade = NivelProbabilidade.Improvavel, Resposta = RespostaRisco.Aceitar,
            ResponsavelNome = "André Costa",
        });

        charter.LinhasOrcamento.Add(new LinhaOrcamento
        {
            Charter = charter, Categoria = "RH", Descricao = "Equipa interna (840 h)", Ano = hoje.Year, ValorPrevisto = 18000m, ValorReal = 6200m,
        });
        charter.LinhasOrcamento.Add(new LinhaOrcamento
        {
            Charter = charter, Categoria = "Serviços", Descricao = "Cloud para o treino do modelo", Ano = hoje.Year, ValorPrevisto = 3250m,
            ValorReal = 3400m,
        });

        charter.RequisitosConformidade.Add(new RequisitoConformidade
        {
            Charter = charter, Tipo = TipoRequisitoConformidade.Legal, Requisito = "RGPD: dados de clientes nos registos das OLT", Entidade = "CNPD",
            ResponsavelNome = "Jurídico", DataPrevista = hoje.AddDays(10), Estado = EstadoRequisito.EmCurso,
        });
        charter.RequisitosConformidade.Add(new RequisitoConformidade
        {
            Charter = charter, Tipo = TipoRequisitoConformidade.Seguranca, Requisito = "Acesso só de leitura às OLT",
            Resposta = "Conta técnica dedicada, sem permissões de escrita", ResponsavelNome = "TI", DataPrevista = hoje.AddDays(-30),
            DataConfirmacao = hoje.AddDays(-31), Estado = EstadoRequisito.Cumprido,
        });

        _context.ProjectCharters.Add(charter);

        // Pelos métodos do domínio, como na aplicação: o relatório do M1 entra no conhecimento codificado (Mod.237)
        // e a lição no repositório de lições aprendidas (Mod.252).
        m1.RegistarRelatorio("https://dstelecom.sharepoint.com/sites/inovacao/OLT-relatorio-M1.pdf", hoje.AddDays(-3));
        charter.RegistarLicao(new LicaoAprendida
        {
            Charter = charter, Categoria = CategoriaLicao.Gestao, Licao = "Envolver o NOC desde o arranque",
            Descricao = "Os requisitos dos alertas mudaram a meio porque o NOC só foi ouvido no fim da recolha de dados.",
            Impacto = "Duas semanas de retrabalho",
        });

        await _context.SaveChangesAsync();
    }

    /// <summary>Plano anual, parcerias, ativos, conhecimento tácito e ferramentas de exemplo.</summary>
    private async Task CriarRegistosExemploAsync(DateOnly hoje)
    {
        var sofia = await _context.Utilizadores.FirstAsync(u => u.Email == "sofia.martins@exemplo.pt");
        var ano = hoje.Year;

        _context.AtividadesPlanoAnual.AddRange(
            new AtividadePlanoAnual
            {
                Titulo = "Avaliação de ideias com a CE", Descricao = "Reunião mensal com a Comissão Executiva.", Cor = "#c8102e",
                DataInicio = new DateOnly(ano, 1, 13), Recorrencia = Periodicidade.Mensal, DiasAntecedenciaAviso = 7, Responsaveis = { sofia },
            },
            new AtividadePlanoAnual
            {
                Titulo = "GetTogether de inovação", Cor = "#3a6f9a", DataInicio = new DateOnly(ano, 6, 18), DataFim = new DateOnly(ano, 6, 19),
                Recorrencia = Periodicidade.Anual, DiasAntecedenciaAviso = 30,
            },
            new AtividadePlanoAnual
            {
                Titulo = "Candidatura SIFIDE", Cor = "#7a9a3a", DataInicio = new DateOnly(ano, 5, 4), DataFim = new DateOnly(ano, 5, 29),
                Recorrencia = Periodicidade.Anual, DiasAntecedenciaAviso = 21,
            },
            new AtividadePlanoAnual
            {
                Titulo = "Revisão do portefólio de iniciativas", Cor = "#7a5aa0", DataInicio = new DateOnly(ano, 3, 31),
                Recorrencia = Periodicidade.Trimestral, DiasAntecedenciaAviso = 10,
            });

        var uminho = new Parceiro { Nome = "Universidade do Minho", Tipo = TipoParceiro.Academico };
        uminho.Acordos.Add(new AcordoParceria
        {
            Descricao = "Protocolo de estágios e dissertações de mestrado", ProtocoloEstabelecido = true, TipoProtocolo = TipoProtocolo.Investigacao,
            DataInicio = new DateOnly(ano - 1, 9, 1), IncluiGestaoPropriedadeIntelectual = true,
        });
        var fornecedor = new Parceiro { Nome = "Fibras do Norte, Lda.", Tipo = TipoParceiro.Empresarial, Nif = "509000000" };
        fornecedor.Acordos.Add(new AcordoParceria
        {
            Descricao = "Acordo de confidencialidade para o piloto de sensores", ProtocoloEstabelecido = true,
            TipoProtocolo = TipoProtocolo.Confidencialidade, DataInicio = new DateOnly(ano, 2, 1), DataFim = new DateOnly(ano + 1, 1, 31),
        });
        _context.Parceiros.AddRange(uminho, fornecedor);

        _context.AtivosIntangiveis.AddRange(
            new AtivoIntangivel
            {
                Data = hoje.AddYears(-2), Tipo = TipoAtivoIntangivel.Marca, EntidadeRegisto = "INPI", Designacao = "Innovera",
                NumeroPedido = "712345", DataRegisto = hoje.AddYears(-2), DataValidade = hoje.AddDays(60), Ambito = "Nacional",
                Situacao = "Concedida", Custo = 250m,
            },
            new AtivoIntangivel
            {
                Data = hoje.AddMonths(-6), Tipo = TipoAtivoIntangivel.Patente, EntidadeRegisto = "EPO", Designacao = "Sensor de esforço em postes",
                Autores = "André Costa, Tiago Araújo", Situacao = "Pedido submetido", Custo = 4800m,
            });

        _context.ConhecimentosTacitos.AddRange(
            new ConhecimentoTacito
            {
                Data = hoje.AddMonths(-3), Empresa = "dstelecom", Detentor = "Inês Correia",
                Descricao = "Configuração das OLT antigas (firmware anterior à versão 5).",
            },
            new ConhecimentoTacito
            {
                Data = hoje.AddMonths(-1), Empresa = "dstelecom", Detentor = "André Costa",
                Descricao = "Negociação dos contratos de manutenção com os operadores.", Confidencialidade = NivelConfidencialidade.Elevado,
            });

        _context.FerramentasMetodos.Add(new FerramentaMetodo
        {
            Data = hoje.AddMonths(-8), Nome = "Design Thinking", SumarioExecutivo = "Método de ideação centrado no utilizador.",
            Potencial = "Workshops de ideação com as equipas de campo.", CustoAquisicao = 0m, Divulgacao = "Formação interna, GetTogether",
        });

        await _context.SaveChangesAsync();
    }
}
