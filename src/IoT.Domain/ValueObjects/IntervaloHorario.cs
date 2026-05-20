using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Ventana de tiempo para disparadores horarios. Inmutable.
/// </summary>
public sealed class IntervaloHorario : ValueObject
{
    public TimeOnly HoraInicio { get; init; }
    public TimeOnly HoraFin { get; init; }
    public IReadOnlyList<DayOfWeek> DiasSemana { get; init; }

    private IntervaloHorario() { DiasSemana = new List<DayOfWeek>().AsReadOnly(); }

    public IntervaloHorario(TimeOnly horaInicio, TimeOnly horaFin, IEnumerable<DayOfWeek> diasSemana)
    {
        if (horaInicio >= horaFin)
            throw new DomainException("La hora de inicio debe ser anterior a la hora de fin.");
        var dias = diasSemana?.ToList() ?? new List<DayOfWeek>();
        if (dias.Count == 0)
            throw new DomainException("Debe especificarse al menos un día de la semana.");

        HoraInicio = horaInicio;
        HoraFin = horaFin;
        DiasSemana = dias.AsReadOnly();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return HoraInicio;
        yield return HoraFin;
        foreach (var dia in DiasSemana) yield return dia;
    }
}
