using IoT.Application.Interfaces;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Marca un dispositivo como desconectado pasando por el Aggregate Root Hogar (DDD).
/// </summary>
public class DesconectarDispositivoHandler
{
    private readonly IDispositivoRepository _dispositivoRepo;
    private readonly IHogarRepository _hogarRepo;
    private readonly ISaveChanges _uow;

    public DesconectarDispositivoHandler(IDispositivoRepository dispositivoRepo,
        IHogarRepository hogarRepo, ISaveChanges uow)
    {
        _dispositivoRepo = dispositivoRepo;
        _hogarRepo = hogarRepo;
        _uow = uow;
    }

    public async Task HandleAsync(int dispositivoId)
    {
        var dispositivo = await _dispositivoRepo.GetByIdAsync(dispositivoId)
            ?? throw new DomainException($"Dispositivo {dispositivoId} no encontrado.");

        var hogar = await _hogarRepo.GetByIdAsync(dispositivo.HogarId)
            ?? throw new DomainException($"Hogar del dispositivo no encontrado.");

        hogar.DesconectarDispositivo(dispositivoId);
        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
    }
}
