namespace Innovera.Domain.Entities;

/// <summary>Projeto de inovação interno ou externo (Mod.246, folha Projetos) com Project Charter.</summary>
public class Projeto : IniciativaComCharter
{
    public override TipoIniciativa Tipo => TipoIniciativa.Projeto;

    public TipoProjeto? TipoProjeto { get; set; }

    public AmbitoProjeto Ambito { get; set; } = AmbitoProjeto.Interno;
}
