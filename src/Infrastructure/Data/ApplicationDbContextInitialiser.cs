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
        foreach (var role in new[] { Roles.Administrador, Roles.GestorProjeto, Roles.Trabalhador })
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Conta inicial da equipa de Inovação. Fora do desenvolvimento, a password tem de vir da configuração.
        var emailAdmin = _configuration["ContaAdministrador:Email"] ?? "admin@innovera.local";
        var passwordAdmin = _configuration["ContaAdministrador:Password"]
            ?? (_configuration.GetValue("DadosExemplo", false) ? PasswordExemplo : null);
        if (passwordAdmin is null)
        {
            _logger.LogWarning("Sem ContaAdministrador:Password configurada: a conta de administrador não foi criada.");
            return;
        }

        await GarantirContaAsync(emailAdmin, "Equipa de Inovação", "dstelecom, s.a.", Roles.Administrador, passwordAdmin);

        if (_configuration.GetValue("DadosExemplo", false) && !await _context.Ideias.AnyAsync())
        {
            await CriarDadosExemploAsync();
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
                Perfil = role == Roles.Administrador ? PerfilUtilizador.Administrador : PerfilUtilizador.Trabalhador,
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
        _context.Iniciativas.Add(new Projeto
        {
            Codigo = olt.Codigo, Titulo = olt.Titulo, Descricao = olt.Descricao, IdeiaOrigemId = olt.Id,
            ResponsavelId = andre.Id, Estado = EstadoIniciativa.EmCurso, TipoProjeto = TipoProjeto.Inovacao,
            Horizonte = Horizonte.H2, Origem = "Inov. Interna",
        });

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
        _logger.LogInformation("Dados de exemplo criados (contas com a password {Password}).", PasswordExemplo);
    }
}
