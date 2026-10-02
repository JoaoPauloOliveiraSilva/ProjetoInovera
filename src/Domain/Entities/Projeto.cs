namespace Innovera.Domain.Entities;

public class Projeto : Iniciativa
{
    public string? ProjectCharterResumo { get; set; }
    public string? ProjectCharterUrl { get; set; }

    public ICollection<Orçamento> Orcamentos { get; set; }

    public ICollection<Atividade> MarcosPlaneamento { get; set; }

    public string? MatrizRiscos { get; set; }
    public string? NotasConformidade { get; set; }

    public ICollection<Licao_Aprendida> LicoesAprendidas { get; set; }

    public Projeto()
    {
        Orcamentos = new List<Orçamento>();
        MarcosPlaneamento = new List<Atividade>();
        LicoesAprendidas = new List<Licao_Aprendida>();
    }
}