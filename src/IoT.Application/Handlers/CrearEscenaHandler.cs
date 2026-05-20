using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Application.Mappings;
using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta la creación de una nueva escena con sus acciones (SRP).
/// </summary>
public class CrearEscenaHandler
{
    private readonly IEscenaRepository _escenaRepo;
    private readonly ISaveChanges _uow;
    private readonly IEventPublisher _eventPublisher;

    public CrearEscenaHandler(IEscenaRepository escenaRepo, ISaveChanges uow, IEventPublisher eventPublisher)
    {
        _escenaRepo = escenaRepo;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<EscenaDto> HandleAsync(CrearEscenaCommand command)
    {
        var nombre = new NombreEscena(command.Nombre);
        var escena = new Escena(0, nombre, command.HogarId);

        foreach (var a in command.Acciones)
        {
            var parametro = a.ParametroNombre != null
                ? new ParametroComando(a.ParametroNombre, a.ParametroValor ?? string.Empty, "string")
                : null;
            escena.AgregarAccion(0, a.Orden, a.DispositivoId, a.Comando, parametro);
        }

        await _escenaRepo.SaveAsync(escena);
        await _uow.SaveChangesAsync();
        escena.ConfirmarCreacion();
        await _eventPublisher.PublishAllAsync(escena.DomainEvents);
        escena.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(escena);
    }
}
