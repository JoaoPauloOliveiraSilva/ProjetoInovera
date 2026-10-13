namespace Innovera.Domain.Constants;

/// <summary>Uma permissão como aparece no backoffice.</summary>
public record PermissaoInfo(string Codigo, string Nome, string Grupo);

/// <summary>
/// Permissões da plataforma. Os casos de uso pedem uma permissão (<c>[Authorize(Permissao = ...)]</c>), nunca um perfil.
/// Cada perfil tem a sua lista de permissões guardada na base de dados (AspNetRoleClaims, tipo "permissao"),
/// editável pelo Administrador. Uma funcionalidade nova só precisa de uma constante aqui.
/// </summary>
public static class Permissoes
{
    /// <summary>Tipo do claim do perfil que guarda cada permissão.</summary>
    public const string TipoClaim = "permissao";

    /// <summary>
    /// Guardado no perfil Administrador: permissões que a aplicação já conhece. Quando aparece uma permissão nova,
    /// os perfis do sistema recebem-na por omissão uma vez; depois quem manda é o backoffice.
    /// </summary>
    public const string TipoClaimConhecida = "permissao-conhecida";

    public static class Ideias
    {
        public const string VerTodas = "ideias.ver-todas";
        public const string Validar = "ideias.validar";
        public const string Avaliar = "ideias.avaliar";
    }

    public static class Iniciativas
    {
        public const string Gerir = "iniciativas.gerir";
    }

    /// <summary>Registos manuais (Mod.52, 235, 236, 237, 238, 241) e plano anual de atividades (Mod.69).</summary>
    public static class Registos
    {
        public const string Gerir = "registos.gerir";
        public const string GerirPlano = "plano.gerir";
    }

    /// <summary>Valores introduzidos à mão para os KPIs (n.º de trabalhadores, receitas…). Por omissão, só o Administrador.</summary>
    public static class Kpis
    {
        public const string ValoresExternos = "kpis.valores-externos";
        public const string Objetivos = "kpis.objetivos";
    }

    /// <summary>Orçamento dos projetos: por omissão só o Administrador (pode dar a outros perfis no backoffice).</summary>
    public static class Orcamento
    {
        public const string Ver = "orcamento.ver";
        public const string Gerir = "orcamento.gerir";
    }

    public static class Administracao
    {
        public const string GerirUtilizadores = "utilizadores.gerir";
        public const string GerirPerfis = "perfis.gerir";
        public const string VerAuditoria = "auditoria.ver";
    }

    public static readonly IReadOnlyList<PermissaoInfo> Todas =
    [
        new(Ideias.VerTodas, "Ver todas as ideias (privadas e por validar) e anexar ficheiros", "Ideias"),
        new(Ideias.Validar, "Validar ideias e fechar discussões", "Ideias"),
        new(Ideias.Avaliar, "Registar a avaliação da CE, a decisão dst e a classe", "Ideias"),
        new(Iniciativas.Gerir, "Gerir projetos, desafios e outras iniciativas e importar o portefólio (Mod.246)", "Iniciativas"),
        new(Registos.Gerir, "Gerir parcerias, ativos intangíveis, conhecimento codificado e tácito, ferramentas e métodos e a estratégia (Mod.52)", "Registos"),
        new(Registos.GerirPlano, "Gerir o plano anual de atividades (Mod.69)", "Registos"),
        new(Kpis.ValoresExternos, "Introduzir os valores externos dos KPIs (n.º de trabalhadores, receitas…)", "Indicadores"),
        new(Kpis.Objetivos, "Definir os objetivos anuais e os planos de ação dos KPIs", "Indicadores"),
        new(Orcamento.Ver, "Ver o orçamento dos projetos e desafios", "Orçamento"),
        new(Orcamento.Gerir, "Introduzir e alterar o orçamento dos projetos e desafios", "Orçamento"),
        new(Administracao.GerirUtilizadores, "Adicionar e gerir utilizadores", "Administração"),
        new(Administracao.GerirPerfis, "Gerir perfis e permissões", "Administração"),
        new(Administracao.VerAuditoria, "Ver o registo de auditoria (quem alterou o quê e quando)", "Administração"),
    ];

    public static bool Existe(string codigo) => Todas.Any(p => p.Codigo == codigo);

    /// <summary>Permissões dadas a cada perfil quando é criado pela primeira vez (depois o Administrador pode mudar).</summary>
    public static readonly IReadOnlyDictionary<string, IReadOnlyList<string>> PorOmissao = new Dictionary<string, IReadOnlyList<string>>
    {
        [Roles.EquipaInovacao] = [Ideias.VerTodas, Ideias.Validar, Ideias.Avaliar, Iniciativas.Gerir, Registos.Gerir, Registos.GerirPlano, Kpis.Objetivos],
        [Roles.Trabalhador] = [],
    };
}
