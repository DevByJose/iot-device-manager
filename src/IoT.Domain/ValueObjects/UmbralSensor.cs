using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Rango aceptable para una lectura de sensor. Inmutable.
/// </summary>
public sealed class UmbralSensor : ValueObject
{
    public double ValorMin { get; init; }
    public double ValorMax { get; init; }
    public string Unidad { get; init; }

    private UmbralSensor() { Unidad = string.Empty; }

    public UmbralSensor(double valorMin, double valorMax, string unidad)
    {
        if (valorMin >= valorMax)
            throw new DomainException("El valor mínimo debe ser menor al valor máximo del umbral.");
        if (string.IsNullOrWhiteSpace(unidad))
            throw new DomainException("La unidad del umbral no puede estar vacía.");

        ValorMin = valorMin;
        ValorMax = valorMax;
        Unidad = unidad;
    }

    public bool EstaFueraDeRango(double valor) => valor < ValorMin || valor > ValorMax;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return ValorMin;
        yield return ValorMax;
        yield return Unidad;
    }
}
