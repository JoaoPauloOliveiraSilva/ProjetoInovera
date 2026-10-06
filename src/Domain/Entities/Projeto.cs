namespace Innovera.Domain.Entities;

public class Projeto : Iniciativa
{
    public string? Tipo { get; set; } 
    public string? Horizonte { get; set; } 
    public string? Origem { get; set; } 
    public string? PontoDeSituacao { get; set; } 

    public DateTime? DataInicio { get; set; } 
    public DateTime? DataFim { get; set; } 

    public string? ContextoJustificacao { get; set; }
    public string? BusinessCase { get; set; }
    public string? ProjectCharterUrl { get; set; }

    public string? MatrizRiscos { get; set; }
    public string? NotasConformidade { get; set; }

    public ICollection<Orçamento> Orcamentos { get; set; }
    public ICollection<Licao_Aprendida> LicoesAprendidas { get; set; }

    public Projeto()
    {
        Orcamentos = new List<Orçamento>();
        LicoesAprendidas = new List<Licao_Aprendida>();
    }
}