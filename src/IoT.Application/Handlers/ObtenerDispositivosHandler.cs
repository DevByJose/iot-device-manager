using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Maneja la consulta de dispositivos de un hogar (SRP).
/// </summary>
public class ObtenerDispositivosHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerDispositivosHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<DispositivoDto>> HandleAsync(ObtenerDispositivosQuery query)
    {
        var hogar = await _hogarRepo.GetByIdAsync(query.HogarId);
        if (hogar == null) return new List<DispositivoDto>().AsReadOnly();
        return DomainToDtoMapper.ToDtoList(hogar.Dispositivos, hogar.Habitaciones);
    }
}
