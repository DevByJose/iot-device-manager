using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Coordenadas geográficas para disparadores de ubicación. Inmutable.
/// </summary>
public sealed class GeoLocalizacion : ValueObject
{
    public double Latitud { get; private set; }
    public double Longitud { get; private set; }
    public double RadioMetros { get; private set; }

    private GeoLocalizacion() { }

    public GeoLocalizacion(double latitud, double longitud, double radioMetros)
    {
        if (latitud < -90 || latitud > 90)
            throw new DomainException("La latitud debe estar entre -90 y 90.");
        if (longitud < -180 || longitud > 180)
            throw new DomainException("La longitud debe estar entre -180 y 180.");
        if (radioMetros <= 0)
            throw new DomainException("El radio debe ser mayor a 0 metros.");

        Latitud = latitud;
        Longitud = longitud;
        RadioMetros = radioMetros;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitud;
        yield return Longitud;
        yield return RadioMetros;
    }
}
