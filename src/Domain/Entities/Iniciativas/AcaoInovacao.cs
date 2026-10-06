namespace Innovera.Domain.Entities;

/// <summary>
/// Ação de inovação: evento criado pela equipa de Inovação ou evento externo em que participa
/// (Mod.246 e KPIs 32–34).
/// </summary>
public class AcaoInovacao : Iniciativa
{
    public override TipoIniciativa Tipo => TipoIniciativa.AcaoInovacao;

    public TipoAcaoInovacao TipoAcao { get; set; } = TipoAcaoInovacao.Criada;

    public int? NumeroParticipantes { get; set; }
}
