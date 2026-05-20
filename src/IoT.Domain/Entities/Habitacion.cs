using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.Entities;

/// <summary>
/// Entidad hija del agregado Hogar. Agrupación lógica de dispositivos.
/// </summary>
public class Habitacion : Entity
{
    public string Nombre { get; private set; }
    public int HogarId { get; private set; }

    private Habitacion() { Nombre = string.Empty; } // EF Core

    public Habitacion(int id, string nombre, int hogarId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre de la habitación no puede estar vacío.");

        Id = id;
        Nombre = nombre;
        HogarId = hogarId;
    }

    public void CambiarNombre(string nuevoNombre)
    {
        if (string.IsNullOrWhiteSpace(nuevoNombre))
            throw new DomainException("El nuevo nombre de la habitación no puede estar vacío.");
        Nombre = nuevoNombre;
    }
}
