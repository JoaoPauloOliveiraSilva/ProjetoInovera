namespace Innovera.Domain.Entities;

public class Ideia : Iniciativa
{
   
   public DateTime? InicioDiscussao { get; set; }
    public DateTime? FimDiscussao { get; set; }

    public int NumeroVotos { get; set; } 
    public int NumeroComentarios { get; set; }


    public  EstadoIdeia? Estado { get; set; } 
    public int? PontuacaoFinal { get; set; }

    public int? IniciativaResultanteId { get; set; }
    public Iniciativa? IniciativaResultante { get; set; }
}
