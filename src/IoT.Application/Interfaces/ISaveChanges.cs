namespace IoT.Application.Interfaces;

/// <summary>
/// Interfaz mínima de persistencia (ISP). Los handlers que solo persisten
/// dependen de esto, no de la interfaz de transacción completa.
/// </summary>
public interface ISaveChanges : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
