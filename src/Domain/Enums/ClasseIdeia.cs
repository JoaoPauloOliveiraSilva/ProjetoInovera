namespace Innovera.Domain.Enums;

/// <summary>
/// Classe da ideia depois da decisão (Mod.246).
/// </summary>
public enum ClasseIdeia
{
    /// <summary>Segue para implementação no dia a dia ("vai para o office").</summary>
    MelhoriaContinua,
    /// <summary>Precisa de mais investigação: passa a projeto.</summary>
    Projeto,
    /// <summary>Precisa de mais investigação: passa a desafio.</summary>
    Desafio,
    /// <summary>Já houve uma ideia igual.</summary>
    IdeiaDuplicada,
    /// <summary>Tem mérito mas não avança agora; fica em arquivo para usar mais tarde.</summary>
    Arquivada,
    /// <summary>Não tem mérito.</summary>
    NaoAplicavel
}
