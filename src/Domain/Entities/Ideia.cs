namespace Innovera.Domain.Entities;

public class Ideia : Iniciativa
{
    public string? Origem { get; set; } 
    public Responsabilidade? Responsabilidade { get; set; } 

    public DateTime? InicioDiscussao { get; set; }
    public DateTime? FimDiscussao { get; set; }
    public int NumeroVotos { get; set; } 
    public int NumeroComentarios { get; set; }

    public EscalaDeAvaliacao? AvaliacaoCusto { get; set; }
    public EscalaDeAvaliacao? AvaliacaoEnquadramento { get; set; } 
    public EscalaDeAvaliacao? AvaliacaoBeneficio { get; set; } 
    public EscalaDeAvaliacao? AvaliacaoAdequacaoTecnica { get; set; } 
    public EscalaDeAvaliacao? AvaliacaoIncerteza { get; set; } 
    public decimal? TotalAvaliacao { get; set; } 

    public Classificacao? Classificacao { get; set; } 
    public Classe? Classe { get; set; } 
    
    public string? StatusDetalhado { get; set; } 

    public int? IniciativaResultanteId { get; set; }
    public Iniciativa? IniciativaResultante { get; set; }
}