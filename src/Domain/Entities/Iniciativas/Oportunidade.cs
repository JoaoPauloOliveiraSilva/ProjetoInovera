namespace Innovera.Domain.Entities;

/// <summary>Oportunidade (Mod.246). O resultado indica se deu origem a outra iniciativa.</summary>
public class Oportunidade : Iniciativa
{
    public override TipoIniciativa Tipo => TipoIniciativa.Oportunidade;

    /// <summary>Ex.: "Deu origem ao projeto X", "Sem iniciativa implementada".</summary>
    public string? Resultado { get; set; }
}
