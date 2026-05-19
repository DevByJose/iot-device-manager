using IoT.Domain.BuildingBlocks;
using IoT.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace IoT.Infrastructure.Services;

/// <summary>
/// Implementa ICacheService con MemoryCache (DIP). El dominio no sabe qué caché se usa.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue) options.AbsoluteExpirationRelativeToNow = expiration;
        else options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
