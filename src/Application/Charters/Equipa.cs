namespace Innovera.Application.Charters;

// Equipa de trabalho de um projeto/desafio e alocação temporal por mês (Project Charter › planeamento).
// A alocação é em % por mês; as horas são 8 h × 21 dias × % (como no modelo Excel).

public record AlocacaoDto(int Ano, int Mes, decimal Percentagem, decimal Horas);

public record MembroDto(int Id, int? UtilizadorId, string Nome, string Funcao, IReadOnlyList<AlocacaoDto> Alocacoes);

public record MembroDados(int? UtilizadorId, string? Nome, string? Funcao);

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarMembroCommand(int IniciativaId, int? Id, MembroDados Dados) : IRequest<int>;

public class GuardarMembroCommandValidator : AbstractValidator<GuardarMembroCommand>
{
    public GuardarMembroCommandValidator()
    {
        RuleFor(c => c.Dados).Must(d => d.UtilizadorId is not null || !string.IsNullOrWhiteSpace(d.Nome))
            .WithMessage("Escolha um utilizador ou escreva o nome da pessoa.");
        RuleFor(c => c.Dados.Nome).MaximumLength(200);
        RuleFor(c => c.Dados.Funcao).MaximumLength(150);
    }
}

public class GuardarMembroCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarMembroCommand, int>
{
    public async Task<int> Handle(GuardarMembroCommand request, CancellationToken cancellationToken)
    {
        if (!await context.Iniciativas.AnyAsync(i => i.Id == request.IniciativaId, cancellationToken))
        {
            throw new NotFoundException(request.IniciativaId.ToString(System.Globalization.CultureInfo.InvariantCulture), nameof(Iniciativa));
        }

        var d = request.Dados;
        if (d.UtilizadorId is { } utilizadorId)
        {
            if (!await context.Utilizadores.AnyAsync(u => u.Id == utilizadorId, cancellationToken))
            {
                throw new RegraDeNegocioException("O utilizador escolhido já não existe.");
            }

            if (await context.MembrosEquipa.AnyAsync(m => m.IniciativaId == request.IniciativaId && m.UtilizadorId == utilizadorId
                    && (request.Id == null || m.Id != request.Id), cancellationToken))
            {
                throw new RegraDeNegocioException("Essa pessoa já faz parte da equipa.");
            }
        }

        MembroEquipa m;
        if (request.Id is null)
        {
            m = new MembroEquipa { IniciativaId = request.IniciativaId };
            context.MembrosEquipa.Add(m);
        }
        else
        {
            var existente = await context.MembrosEquipa
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.IniciativaId == request.IniciativaId, cancellationToken);
            Guard.Against.NotFound(request.Id.Value, existente);
            m = existente;
        }

        m.UtilizadorId = d.UtilizadorId;
        m.Nome = d.UtilizadorId is null ? d.Nome?.Trim() : null;
        m.Funcao = d.Funcao?.Trim() ?? string.Empty;
        await context.SaveChangesAsync(cancellationToken);
        return m.Id;
    }
}

[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record ApagarMembroCommand(int IniciativaId, int MembroId) : IRequest;

public class ApagarMembroCommandHandler(IApplicationDbContext context) : IRequestHandler<ApagarMembroCommand>
{
    public async Task Handle(ApagarMembroCommand request, CancellationToken cancellationToken)
    {
        var m = await context.MembrosEquipa.Include(x => x.Alocacoes)
            .FirstOrDefaultAsync(x => x.Id == request.MembroId && x.IniciativaId == request.IniciativaId, cancellationToken);
        Guard.Against.NotFound(request.MembroId, m);
        context.MembrosEquipa.Remove(m);
        await context.SaveChangesAsync(cancellationToken);
    }
}

/// <summary>Alocação de um membro nos 12 meses de um ano (em %, vazio = 0). Substitui o que havia nesse ano.</summary>
[Authorize(Permissao = Permissoes.Iniciativas.Gerir)]
public record GuardarAlocacoesCommand(int IniciativaId, int MembroId, int Ano, IReadOnlyList<decimal?> Meses) : IRequest;

public class GuardarAlocacoesCommandValidator : AbstractValidator<GuardarAlocacoesCommand>
{
    public GuardarAlocacoesCommandValidator()
    {
        RuleFor(c => c.Ano).InclusiveBetween(2000, 2100);
        RuleFor(c => c.Meses).Must(m => m.Count == 12).WithMessage("São precisos os 12 meses.");
        RuleForEach(c => c.Meses).Must(p => p is null || (p >= 0 && p <= 100)).WithMessage("A alocação é uma percentagem entre 0 e 100.");
    }
}

public class GuardarAlocacoesCommandHandler(IApplicationDbContext context) : IRequestHandler<GuardarAlocacoesCommand>
{
    public async Task Handle(GuardarAlocacoesCommand request, CancellationToken cancellationToken)
    {
        var m = await context.MembrosEquipa.Include(x => x.Alocacoes)
            .FirstOrDefaultAsync(x => x.Id == request.MembroId && x.IniciativaId == request.IniciativaId, cancellationToken);
        Guard.Against.NotFound(request.MembroId, m);

        for (var mes = 1; mes <= 12; mes++)
        {
            var percentagem = request.Meses[mes - 1] ?? 0m;
            var atual = m.Alocacoes.FirstOrDefault(a => a.Ano == request.Ano && a.Mes == mes);
            if (percentagem == 0m)
            {
                if (atual is not null)
                {
                    context.AlocacoesMensais.Remove(atual);
                }
            }
            else if (atual is null)
            {
                m.Alocacoes.Add(new AlocacaoMensal { Ano = request.Ano, Mes = mes, Percentagem = percentagem });
            }
            else
            {
                atual.Percentagem = percentagem;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
