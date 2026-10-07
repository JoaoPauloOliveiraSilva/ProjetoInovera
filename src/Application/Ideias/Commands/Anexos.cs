namespace Innovera.Application.Ideias.Commands;

/// <summary>Anexa um ficheiro (até 25 MB) a uma ideia. Só os autores e a equipa de Inovação.</summary>
[Authorize]
public record AdicionarAnexoCommand(int IdeiaId, string NomeFicheiro, string TipoConteudo, long TamanhoBytes, Stream Conteudo) : IRequest<AnexoDto>;

public class AdicionarAnexoCommandValidator : AbstractValidator<AdicionarAnexoCommand>
{
    public AdicionarAnexoCommandValidator()
    {
        RuleFor(c => c.NomeFicheiro).NotEmpty().MaximumLength(260);
        RuleFor(c => c.TamanhoBytes).GreaterThan(0).LessThanOrEqualTo(AnexoIdeia.TamanhoMaximoBytes)
            .WithMessage("O anexo tem de ter no máximo 25 MB.");
    }
}

public class AdicionarAnexoCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, IArmazenamentoFicheiros armazenamento)
    : IRequestHandler<AdicionarAnexoCommand, AnexoDto>
{
    public async Task<AnexoDto> Handle(AdicionarAnexoCommand request, CancellationToken cancellationToken)
    {
        var eu = await utilizadores.AtualAsync(cancellationToken);
        var ideia = await CarregarIdeia.ComTudoAsync(context, request.IdeiaId, cancellationToken);

        var eAutor = ideia.AutorId == eu.Id || ideia.CreatedBy == eu.IdentityId || ideia.Coautores.Any(c => c.UtilizadorId == eu.Id);
        if (!eAutor && !utilizadores.EAdministrador)
        {
            throw new Common.Exceptions.ForbiddenAccessException();
        }

        var chave = await armazenamento.GuardarAsync(request.Conteudo, $"ideias/{ideia.Codigo}", request.NomeFicheiro, cancellationToken);
        var anexo = new AnexoIdeia
        {
            NomeFicheiro = Path.GetFileName(request.NomeFicheiro),
            TipoConteudo = string.IsNullOrWhiteSpace(request.TipoConteudo) ? "application/octet-stream" : request.TipoConteudo,
            TamanhoBytes = request.TamanhoBytes,
            ChaveArmazenamento = chave,
        };
        ideia.AdicionarAnexo(anexo);

        await context.SaveChangesAsync(cancellationToken);
        return new AnexoDto(anexo.Id, anexo.NomeFicheiro, anexo.TamanhoBytes);
    }
}
