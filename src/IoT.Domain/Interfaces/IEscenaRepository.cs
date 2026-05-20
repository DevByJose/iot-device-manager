using IoT.Domain.Entities;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para el agregado Escena.
/// </summary>
public interface IEscenaRepository
{
    Task<Escena?> GetByIdAsync(int id);
    Task<IReadOnlyList<Escena>> GetByHogarIdAsync(int hogarId);
    Task SaveAsync(Escena escena);
    Task DeleteAsync(int id);
}
