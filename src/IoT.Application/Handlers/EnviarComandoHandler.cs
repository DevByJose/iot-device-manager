using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Envía un comando directo a un dispositivo validando tipo y conectividad (SRP).
/// </summary>
public class EnviarComandoHandler
{
    private readonly IDispositivoRepository _dispositivoRepo;
    private readonly IComandoRepository _comandoRepo;
    private readonly SvcValidacionComando _svcValidacion;
    private readonly ISaveChanges _uow;
    private readonly IEventPublisher _eventPublisher;

    public EnviarComandoHandler(IDispositivoRepository dispositivoRepo, IComandoRepository comandoRepo,
        SvcValidacionComando svcValidacion, ISaveChanges uow, IEventPublisher eventPublisher)
    {
        _dispositivoRepo = dispositivoRepo;
        _comandoRepo = comandoRepo;
        _svcValidacion = svcValidacion;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<ComandoDispositivoDto> HandleAsync(EnviarComandoCommand command)
    {
        var dispositivo = await _dispositivoRepo.GetByIdAsync(command.DispositivoId)
            ?? throw new DomainException($"Dispositivo {command.DispositivoId} no encontrado.");

        _svcValidacion.Validar(dispositivo, command.Comando);

        var parametros = command.Parametros?
            .Select(p => new ParametroComando(p.Key, p.Value, "string"))
            .ToList();

        var cmd = new ComandoDispositivo(0, command.DispositivoId, command.Comando, parametros);
        cmd.MarcarEnviado();

        await _comandoRepo.SaveAllAsync(new[] { cmd });
        await _uow.SaveChangesAsync();
        await _eventPublisher.PublishAllAsync(cmd.DomainEvents);
        cmd.ClearDomainEvents();

        return new ComandoDispositivoDto(cmd.Id, cmd.DispositivoId, cmd.Comando, cmd.Estado, cmd.CreadoEn);
    }
}
