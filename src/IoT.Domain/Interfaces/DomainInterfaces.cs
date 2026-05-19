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

/// <summary>
/// Contrato de repositorio para el agregado EstadoDispositivo.
/// </summary>
public interface IEstadoRepository
{
    Task<EstadoDispositivo?> GetByDispositivoIdAsync(int dispositivoId);
    Task<IReadOnlyList<LecturaSensor>> GetLecturasAsync(int dispositivoId, DateTime desde, DateTime hasta);
    Task SaveAsync(EstadoDispositivo estado);
}

/// <summary>
/// Contrato para publicación de eventos de dominio (DIP).
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(BuildingBlocks.IDomainEvent domainEvent);
    Task PublishAllAsync(IEnumerable<BuildingBlocks.IDomainEvent> domainEvents);
}

/// <summary>
/// Contrato para servicio de caché (ISP).
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}

/// <summary>
/// Contrato para Unit of Work — garantiza consistencia transaccional (SRP).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
