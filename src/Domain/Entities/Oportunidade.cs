namespace Innovera.Domain.Entities;

public class Oportunidade : Iniciativa
{
    public string? Resultado { get; set; }
    public string? LinkConsulta { get; set; }

    public int? IniciativaResultanteId { get; set; }
    public Iniciativa? IniciativaResultante { get; set; }
}
