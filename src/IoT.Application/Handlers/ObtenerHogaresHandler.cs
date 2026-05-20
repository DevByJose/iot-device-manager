using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Maneja la consulta de hogares de un cliente (SRP).
/// </summary>
public class ObtenerHogaresHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerHogaresHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<HogarDto>> HandleAsync(ObtenerHogaresQuery query)
    {
        var hogares = await _hogarRepo.GetByClienteIdAsync(query.ClienteId);
        return DomainToDtoMapper.ToDtoList(hogares);
    }
}
