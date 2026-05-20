using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta la ejecución de una escena. El controller gestiona la transacción explícita (SRP).
/// </summary>
public class EjecutarEscenaHandler
{
    private readonly IEscenaRepository _escenaRepo;
    private readonly IComandoRepository _comandoRepo;
    private readonly SvcEjecucionEscena _svcEjecucion;
    private readonly IEventPublisher _eventPublisher;

    public EjecutarEscenaHandler(IEscenaRepository escenaRepo, IComandoRepository comandoRepo,
        SvcEjecucionEscena svcEjecucion, IEventPublisher eventPublisher)
    {
        _escenaRepo = escenaRepo;
        _comandoRepo = comandoRepo;
        _svcEjecucion = svcEjecucion;
        _eventPublisher = eventPublisher;
    }

    public async Task<EjecutarEscenaResponse> HandleAsync(EjecutarEscenaCommand command)
    {
        var escena = await _escenaRepo.GetByIdAsync(command.EscenaId)
            ?? throw new DomainException($"Escena {command.EscenaId} no encontrada.");

        var comandos = await _svcEjecucion.EjecutarAsync(escena, command.Origen);

        await _escenaRepo.SaveAsync(escena);
        await _comandoRepo.SaveAllAsync(comandos);

        await _eventPublisher.PublishAllAsync(escena.DomainEvents);
        escena.ClearDomainEvents();

        foreach (var cmd in comandos)
        {
            await _eventPublisher.PublishAllAsync(cmd.DomainEvents);
            cmd.ClearDomainEvents();
        }

        var enviados = comandos.Count(c => c.Estado == "Enviado");
        var fallidos = comandos.Count(c => c.Estado == "Fallido");
        var detalles = comandos.Select(c => $"[{c.Estado}] Dispositivo {c.DispositivoId}: {c.Comando}").ToList();

        return new EjecutarEscenaResponse(escena.Id, enviados, fallidos, detalles);
    }
}
