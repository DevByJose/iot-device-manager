using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Application.Mappings;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta agregar una habitación a un hogar existente (DDD).
/// </summary>
public class AgregarHabitacionHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly ISaveChanges _uow;
    private readonly IEventPublisher _eventPublisher;

    public AgregarHabitacionHandler(IHogarRepository hogarRepo, ISaveChanges uow, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<HabitacionDto> HandleAsync(AgregarHabitacionCommand command)
    {
        var hogar = await _hogarRepo.GetByIdAsync(command.HogarId)
            ?? throw new DomainException($"Hogar {command.HogarId} no encontrado.");

        var habitacion = hogar.AgregarHabitacion(0, command.Nombre);

        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(habitacion);
    }
}
