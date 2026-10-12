using Innovera.Application.Common.Interfaces;
using Innovera.Domain.Entities;
using Innovera.Domain.Enums;
using Innovera.Domain.Events;
using Microsoft.EntityFrameworkCore;
using MediatR;

namespace Innovera.Application.Iniciativas.Vigilancias.EventHandlers;

public class ResultadoVigilanciaParaConhecimentoCodificado(
    IApplicationDbContext context
) : INotificationHandler<ResultadoVigilanciaRegistadoEvent>
{
    private const string ModoConsultaOneDrive = "OneDrive dstelecom inov";

    public async Task Handle(
        ResultadoVigilanciaRegistadoEvent notification,
        CancellationToken cancellationToken)
    {
        var execucao = notification.Execucao;
        var vigilancia = execucao.Vigilancia;

        var existente = execucao.Id == 0
            ? null
            : await context.ConhecimentosCodificados
                .FirstOrDefaultAsync(
                    c => c.ExecucaoVigilanciaId == execucao.Id,
                    cancellationToken);

        var registo = existente ?? new ConhecimentoCodificado
        {
            Origem = OrigemConhecimento.Vigilancia,
            ExecucaoVigilancia = execucao,
            Iniciativa = vigilancia
        };

        var data = execucao.DataRealizada ?? execucao.DataPrevista;

        var documento =
            $"Resultado da vigilância {vigilancia.Titulo} — {data:yyyy-MM-dd}";

        registo.Documento = documento.Length > 300
            ? documento[..300]
            : documento;

        registo.Data = data;
        registo.ArquivadoEm = data;
        registo.Descricao = vigilancia.Descritivo ?? vigilancia.Descricao;
        registo.Resultado = execucao.Resultado;
        registo.Url = execucao.LinkResultado;

        registo.ModoConsulta =
            string.IsNullOrWhiteSpace(execucao.LinkResultado)
                ? null
                : ModoConsultaOneDrive;

        if (existente is null)
        {
            context.ConhecimentosCodificados.Add(registo);
        }

      
    }
}