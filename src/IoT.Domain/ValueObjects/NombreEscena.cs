using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Nombre legible de una Escena asignado por el usuario. Inmutable.
/// </summary>
public sealed class NombreEscena : ValueObject
{
    public string Valor { get; private set; }

    private NombreEscena() { Valor = string.Empty; }

    public NombreEscena(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El nombre de la escena no puede estar vacío.");
        if (valor.Length < 3 || valor.Length > 60)
            throw new DomainException("El nombre de la escena debe tener entre 3 y 60 caracteres.");

        Valor = valor;
    }

    public override string ToString() => Valor;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }
}
