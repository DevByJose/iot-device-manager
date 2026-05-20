using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Application.Mappings;
using IoT.Domain.Entities;
using IoT.Domain.Interfaces;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta el registro de un hogar. Persiste y genera ID antes de mapear el DTO (SRP).
/// </summary>
public class RegistrarHogarHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly ISaveChanges _uow;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarHogarHandler(IHogarRepository hogarRepo, ISaveChanges uow, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<HogarDto> HandleAsync(RegistrarHogarCommand command)
    {
        var direccion = new DireccionFisica(command.Calle, command.Numero, command.Ciudad, command.Pais, command.CodigoPostal);
        var hogar = new Hogar(0, command.Nombre, direccion, command.ClienteId);

        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
        hogar.ConfirmarRegistro();
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(hogar);
    }
}
