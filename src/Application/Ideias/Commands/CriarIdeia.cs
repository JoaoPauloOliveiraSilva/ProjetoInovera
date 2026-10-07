namespace Innovera.Application.Ideias.Commands;

/// <summary>"Registar ideia": os 4 passos do formulário.</summary>
[Authorize]
public record CriarIdeiaCommand : IRequest<IdeiaCriadaDto>
{
    // Passo 1
    public TipoIdeia Tipo { get; init; }

    // Passo 2
    public string Titulo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public bool Privada { get; init; }
    public bool Anonima { get; init; }
    public IReadOnlyList<int> CoautoresIds { get; init; } = [];

    // Passo 3
    public string? Vantagens { get; init; }
    public string? Requisitos { get; init; }
    public string? ComoEFeitoAtualmente { get; init; }
    public string? ModeloNegocio { get; init; }
    public string? Competidores { get; init; }
    public string? Custos { get; init; }

    // Passo 4
    public bool TermosAceites { get; init; }
}

public class CriarIdeiaCommandValidator : AbstractValidator<CriarIdeiaCommand>
{
    public CriarIdeiaCommandValidator()
    {
        RuleFor(c => c.Titulo).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Descricao).NotEmpty().MaximumLength(4000);
        RuleFor(c => c.TermosAceites).Equal(true).WithMessage("É preciso aceitar os termos e condições.");
        RuleFor(c => c.CoautoresIds).Empty().When(c => c.Anonima).WithMessage("Uma ideia anónima não pode ter outros autores.");
        RuleFor(c => c.Tipo).IsInEnum();
    }
}

public class CriarIdeiaCommandHandler : IRequestHandler<CriarIdeiaCommand, IdeiaCriadaDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUtilizadores _utilizadores;
    private readonly IGeradorNumeroIdeia _gerador;
    private readonly TimeProvider _relogio;

    public CriarIdeiaCommandHandler(IApplicationDbContext context, IUtilizadores utilizadores, IGeradorNumeroIdeia gerador, TimeProvider relogio)
    {
        _context = context;
        _utilizadores = utilizadores;
        _gerador = gerador;
        _relogio = relogio;
    }

    public async Task<IdeiaCriadaDto> Handle(CriarIdeiaCommand request, CancellationToken cancellationToken)
    {
        var eu = await _utilizadores.AtualAsync(cancellationToken);
        var agora = _relogio.GetUtcNow();
        var detalhado = request.Tipo == TipoIdeia.NovoProdutoServicoDetalhado;

        var ideia = new Ideia
        {
            Tipo = request.Tipo,
            Titulo = request.Titulo.Trim(),
            Descricao = request.Descricao.Trim(),
            Privada = request.Privada,
            Anonima = request.Anonima,
            AutorId = request.Anonima ? null : eu.Id,
            Vantagens = request.Vantagens,
            Requisitos = request.Requisitos,
            ComoEFeitoAtualmente = request.ComoEFeitoAtualmente,
            ModeloNegocio = detalhado ? request.ModeloNegocio : null,
            Competidores = detalhado ? request.Competidores : null,
            Custos = detalhado ? request.Custos : null,
            Origem = "Plataforma",
        };

        var outros = request.CoautoresIds.Where(id => id != eu.Id).Distinct().ToList();
        var existentes = await _context.Utilizadores.Where(u => outros.Contains(u.Id)).Select(u => u.Id).ToListAsync(cancellationToken);
        foreach (var id in existentes)
        {
            ideia.Coautores.Add(new AutorIdeia { UtilizadorId = id });
        }

        ideia.AceitarTermos(agora);
        ideia.Submeter(agora);
        ideia.AtribuirNumero(await _gerador.ProximoAsync(cancellationToken));

        _context.Ideias.Add(ideia);
        await _context.SaveChangesAsync(cancellationToken);

        return new IdeiaCriadaDto(ideia.Id, ideia.Codigo, ideia.Estado);
    }
}
