namespace Innovera.Domain.Entities;

public class Licao_Aprendida : BaseAuditableEntity
{
    
public string? Descricao { get; set; }

    public Categoria Categoria { get; set; }

    public int IniciativaId { get; set; }
    public Iniciativa? Iniciativa { get; set; }
   
}
