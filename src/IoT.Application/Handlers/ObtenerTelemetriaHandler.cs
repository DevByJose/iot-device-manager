using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Consulta el historial de lecturas de un dispositivo en un rango de fechas (SRP).
/// </summary>
public class ObtenerTelemetriaHandler
{
    private readonly IEstadoRepository _estadoRepo;

    public ObtenerTelemetriaHandler(IEstadoRepository estadoRepo)
    {
        _estadoRepo = estadoRepo;
    }

    public async Task<IReadOnlyList<LecturaSensorDto>> HandleAsync(ObtenerTelemetriaQuery query)
    {
        var lecturas = await _estadoRepo.GetLecturasAsync(query.DispositivoId, query.Desde, query.Hasta);
        return lecturas.Select(DomainToDtoMapper.ToDto).ToList().AsReadOnly();
    }
}
