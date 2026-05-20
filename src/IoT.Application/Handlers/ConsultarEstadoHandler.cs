using IoT.Application.DTOs;
using IoT.Application.Interfaces;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Maneja la consulta de estado de un dispositivo. Lee de caché si está disponible (SRP).
/// </summary>
public class ConsultarEstadoHandler
{
    private readonly IEstadoRepository _estadoRepo;
    private readonly ICacheService _cache;

    public ConsultarEstadoHandler(IEstadoRepository estadoRepo, ICacheService cache)
    {
        _estadoRepo = estadoRepo;
        _cache = cache;
    }

    public async Task<EstadoDispositivoDto?> HandleAsync(ConsultarEstadoQuery query)
    {
        var cacheKey = $"estado:dispositivo:{query.DispositivoId}";
        var cached = await _cache.GetAsync<EstadoDispositivoDto>(cacheKey);
        if (cached != null) return cached;

        var estado = await _estadoRepo.GetByDispositivoIdAsync(query.DispositivoId);
        if (estado == null) return null;

        var dto = DomainToDtoMapper.ToDto(estado);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(30));
        return dto;
    }
}
