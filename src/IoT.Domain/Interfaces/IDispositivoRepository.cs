using IoT.Domain.Entities;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para consulta de dispositivos (ISP - segregado del agregado).
/// </summary>
public interface IDispositivoRepository
{
    Task<Dispositivo?> GetByIdAsync(int id);
    Task<IReadOnlyList<Dispositivo>> GetByHogarIdAsync(int hogarId);
    Task<bool> ExisteIdentificadorAsync(string identificadorFisico);
    Task SaveAsync(Dispositivo dispositivo);
    Task DeleteAsync(int id);
}
