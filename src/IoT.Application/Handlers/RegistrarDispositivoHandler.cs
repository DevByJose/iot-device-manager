using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta el registro de un dispositivo. Delega al servicio de dominio (SRP).
/// </summary>
public class RegistrarDispositivoHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly SvcRegistroDispositivo _svcRegistro;
    private readonly ISaveChanges _uow;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarDispositivoHandler(IHogarRepository hogarRepo, SvcRegistroDispositivo svcRegistro,
        ISaveChanges uow, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _svcRegistro = svcRegistro;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterDeviceResponse> HandleAsync(RegistrarDispositivoCommand command)
    {
        var hogar = await _hogarRepo.GetByIdAsync(command.HogarId)
            ?? throw new DomainException($"Hogar {command.HogarId} no encontrado.");

        var identificador = new IdentificadorFisico(command.IdentificadorFisico, command.TipoIdentificador);
        var firmware = new VersionFirmware(command.FirmwareMajor, command.FirmwareMinor, command.FirmwarePatch);

        var dispositivo = await _svcRegistro.RegistrarAsync(hogar, 0, command.Nombre,
            command.TipoDispositivo, identificador, firmware, command.HabitacionId);

        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return new RegisterDeviceResponse(dispositivo.Id, dispositivo.Nombre, dispositivo.TipoDispositivo, true, "Dispositivo registrado exitosamente.");
    }
}
