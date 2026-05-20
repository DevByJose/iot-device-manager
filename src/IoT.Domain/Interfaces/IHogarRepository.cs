using IoT.Domain.Entities;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato de repositorio para el agregado Hogar. Definido en dominio (DIP).
/// </summary>
public interface IHogarRepository
{
    Task<Hogar?> GetByIdAsync(int id);
    Task<IReadOnlyList<Hogar>> GetAllAsync();
    Task<IReadOnlyList<Hogar>> GetByClienteIdAsync(int clienteId);
    Task SaveAsync(Hogar hogar);
    Task DeleteAsync(int id);
}
