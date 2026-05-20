using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Interfaces;

namespace IoT.Application.Handlers;

/// <summary>
/// Maneja la consulta de habitaciones de un hogar (SRP).
/// </summary>
public class ObtenerHabitacionesHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerHabitacionesHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<HabitacionDto>> HandleAsync(ObtenerHabitacionesQuery query)
    {
        var hogar = await _hogarRepo.GetByIdAsync(query.HogarId);
        if (hogar == null) return new List<HabitacionDto>().AsReadOnly();
        return DomainToDtoMapper.ToDtoList(hogar.Habitaciones);
    }
}
