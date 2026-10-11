using System.Text.Json;
using System.Text.Json.Serialization;
using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Common;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Innovera.Infrastructure.Data.Interceptors;

/// <summary>
/// Registo de auditoria (ISO 56001): cada criação, alteração ou eliminação de uma entidade do domínio fica em
/// RegistosAuditoria com quem fez, quando e os valores antes/depois (só os campos que mudaram).
/// </summary>
public class AuditoriaInterceptor : SaveChangesInterceptor
{
    // Entidades que não interessam ao histórico (avisos, likes) ou que são o próprio registo.
    private static readonly HashSet<Type> Ignoradas =
        [typeof(RegistoAuditoria), typeof(Notificacao), typeof(LikeIdeia), typeof(GostoComentario)];

    // Campos de controlo, iguais em todas as entidades.
    private static readonly HashSet<string> CamposIgnorados =
        [nameof(BaseAuditableEntity.Created), nameof(BaseAuditableEntity.CreatedBy),
         nameof(BaseAuditableEntity.LastModified), nameof(BaseAuditableEntity.LastModifiedBy)];

    private static readonly JsonSerializerOptions Json = new() { Converters = { new JsonStringEnumConverter() } };

    private readonly IUser _user;
    private readonly TimeProvider _relogio;
    private List<Pendente> _pendentes = [];
    private bool _aGravarAuditoria;

    private sealed record Pendente(EntityEntry Entrada, AcaoAuditoria Acao, string? Antes, string? Depois);

    public AuditoriaInterceptor(IUser user, TimeProvider relogio)
    {
        _user = user;
        _relogio = relogio;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Recolher(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Recolher(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result,
        CancellationToken cancellationToken = default)
    {
        if (Gravar(eventData.Context) && eventData.Context is not null)
        {
            await eventData.Context.SaveChangesAsync(cancellationToken);
            _aGravarAuditoria = false;
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (Gravar(eventData.Context) && eventData.Context is not null)
        {
            eventData.Context.SaveChanges();
            _aGravarAuditoria = false;
        }

        return base.SavedChanges(eventData, result);
    }

    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        _pendentes = [];
        base.SaveChangesFailed(eventData);
    }

    private void Recolher(DbContext? context)
    {
        if (context is null || _aGravarAuditoria)
        {
            return;
        }

        _pendentes = [];
        foreach (var entrada in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (Ignoradas.Contains(entrada.Entity.GetType()))
            {
                continue;
            }

            switch (entrada.State)
            {
                case EntityState.Added:
                    _pendentes.Add(new Pendente(entrada, AcaoAuditoria.Criar, null, null));
                    break;
                case EntityState.Deleted:
                    _pendentes.Add(new Pendente(entrada, AcaoAuditoria.Eliminar, Valores(entrada, original: true, soAlterados: false), null));
                    break;
                case EntityState.Modified:
                    var antes = Valores(entrada, original: true, soAlterados: true);
                    var depois = Valores(entrada, original: false, soAlterados: true);
                    if (depois is not null)
                    {
                        _pendentes.Add(new Pendente(entrada, AcaoAuditoria.Alterar, antes, depois));
                    }

                    break;
            }
        }
    }

    /// <summary>Depois de gravar (os ids novos já existem): acrescenta os registos e grava-os.</summary>
    private bool Gravar(DbContext? context)
    {
        if (context is null || _aGravarAuditoria || _pendentes.Count == 0)
        {
            return false;
        }

        var agora = _relogio.GetUtcNow();
        foreach (var p in _pendentes)
        {
            var entidade = (BaseEntity)p.Entrada.Entity;
            context.Add(new RegistoAuditoria
            {
                Entidade = entidade.GetType().Name,
                EntidadeId = entidade.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Acao = p.Acao,
                ValoresAntes = p.Antes,
                ValoresDepois = p.Acao == AcaoAuditoria.Criar ? Valores(p.Entrada, original: false, soAlterados: false) : p.Depois,
                UtilizadorId = _user.Id,
                Data = agora,
            });
        }

        _pendentes = [];
        _aGravarAuditoria = true;
        return true;
    }

    private static string? Valores(EntityEntry entrada, bool original, bool soAlterados)
    {
        var valores = new Dictionary<string, object?>();
        foreach (var propriedade in entrada.Properties)
        {
            var nome = propriedade.Metadata.Name;
            if (CamposIgnorados.Contains(nome) || (propriedade.Metadata.IsShadowProperty() && nome != "Discriminador"))
            {
                continue;
            }

            if (soAlterados && (!propriedade.IsModified || Equals(propriedade.OriginalValue, propriedade.CurrentValue)))
            {
                continue;
            }

            valores[nome] = original ? propriedade.OriginalValue : propriedade.CurrentValue;
        }

        return valores.Count == 0 ? null : JsonSerializer.Serialize(valores, Json);
    }
}
