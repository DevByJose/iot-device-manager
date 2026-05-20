using IoT.Domain.Entities;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para el agregado EstadoDispositivo.
/// </summary>
public interface IEstadoRepository
{
    Task<EstadoDispositivo?> GetByDispositivoIdAsync(int dispositivoId);
    Task<IReadOnlyList<LecturaSensor>> GetLecturasAsync(int dispositivoId, DateTime desde, DateTime hasta);
    Task SaveAsync(EstadoDispositivo estado);
}
