using IoT.Domain.Entities;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para ComandoDispositivo. Separado del agregado Escena (ISP).
/// </summary>
public interface IComandoRepository
{
    Task SaveAllAsync(IEnumerable<ComandoDispositivo> comandos);
    Task<IReadOnlyList<ComandoDispositivo>> GetByDispositivoIdAsync(int dispositivoId);
}
