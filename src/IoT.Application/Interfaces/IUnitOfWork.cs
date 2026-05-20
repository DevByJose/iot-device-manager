namespace IoT.Application.Interfaces;

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
