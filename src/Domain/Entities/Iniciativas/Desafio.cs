namespace Innovera.Domain.Entities;

/// <summary>Desafio (ex.: AR&amp;VR, IA): acompanhado como um projeto, com Project Charter.</summary>
public class Desafio : IniciativaComCharter
{
    public override TipoIniciativa Tipo => TipoIniciativa.Desafio;
}
