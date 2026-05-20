using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Services;

/// <summary>
/// Coordina el registro de un Dispositivo verificando unicidad global del identificador (SRP).
/// </summary>
public class SvcRegistroDispositivo
{
    private readonly IDispositivoRepository _dispositivoRepo;

    public SvcRegistroDispositivo(IDispositivoRepository dispositivoRepo)
    {
        _dispositivoRepo = dispositivoRepo;
    }

    public async Task<Dispositivo> RegistrarAsync(Hogar hogar, int dispositivoId, string nombre,
        string tipoDispositivo, IdentificadorFisico identificador, VersionFirmware firmware, int habitacionId)
    {
        // Regla: Identificador físico único globalmente
        if (await _dispositivoRepo.ExisteIdentificadorAsync(identificador.Valor))
            throw new DomainException($"Ya existe un dispositivo con el identificador '{identificador.Valor}'.");

        // Delega al agregado raíz (Hogar) para mantener consistencia
        return hogar.RegistrarDispositivo(dispositivoId, nombre, tipoDispositivo, identificador, firmware, habitacionId);
    }
}
