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
