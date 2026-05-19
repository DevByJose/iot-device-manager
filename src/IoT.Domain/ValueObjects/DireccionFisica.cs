using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Dirección postal del Hogar. Inmutable, valida en constructor.
/// </summary>
public sealed class DireccionFisica : ValueObject
{
    public string Calle { get; private set; }
    public string Numero { get; private set; }
    public string Ciudad { get; private set; }
    public string Pais { get; private set; }
    public string CodigoPostal { get; private set; }

    private DireccionFisica() { Calle = string.Empty; Numero = string.Empty; Ciudad = string.Empty; Pais = string.Empty; CodigoPostal = string.Empty; }

    public DireccionFisica(string calle, string numero, string ciudad, string pais, string codigoPostal)
    {
        if (string.IsNullOrWhiteSpace(pais))
            throw new DomainException("El país es obligatorio en la dirección física.");
        if (string.IsNullOrWhiteSpace(ciudad))
            throw new DomainException("La ciudad es obligatoria en la dirección física.");

        Calle = calle ?? string.Empty;
        Numero = numero ?? string.Empty;
        Ciudad = ciudad;
        Pais = pais;
        CodigoPostal = codigoPostal ?? string.Empty;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Calle;
        yield return Numero;
        yield return Ciudad;
        yield return Pais;
        yield return CodigoPostal;
    }
}
