namespace IoT.Application.Interfaces;

/// <summary>
/// Contrato para servicio de caché. Definido en Application (preocupación técnica transversal),
/// implementado en Infrastructure (DIP).
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
}

/// <summary>
/// Contrato para Unit of Work — garantiza consistencia transaccional. Definido en Application
/// porque orquesta la persistencia de los casos de uso, implementado en Infrastructure (DIP).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
