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
/// Interfaz mínima de persistencia (ISP). Los handlers que solo persisten
/// dependen de esto, no de la interfaz de transacción completa.
/// </summary>
public interface ISaveChanges : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Extiende ISaveChanges con gestión de transacciones explícitas (ISP).
/// Solo lo usan los controladores que coordinan operaciones complejas.
/// </summary>
public interface IUnitOfWork : ISaveChanges
{
    Task BeginTransactionAsync();
    Task CommitAsync();
    Task RollbackAsync();
}
