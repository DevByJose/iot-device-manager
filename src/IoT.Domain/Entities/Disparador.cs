using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Entidad hija: condición que activa una Escena automáticamente.
/// </summary>
public class Disparador : Entity
{
    public string Tipo { get; private set; } // Horario, GeoLocalizacion, Sensor
    public CondicionDisparador Condicion { get; private set; }
    public IntervaloHorario? Horario { get; private set; }

    private Disparador() { Tipo = string.Empty; Condicion = null!; }

    public Disparador(int id, string tipo, CondicionDisparador condicion, IntervaloHorario? horario = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo de disparador no puede estar vacío.");

        Id = id;
        Tipo = tipo;
        Condicion = condicion ?? throw new DomainException("La condición del disparador es obligatoria.");
        Horario = horario;
    }

    public bool EsHorario() => Tipo.Equals("Horario", StringComparison.OrdinalIgnoreCase);
}
