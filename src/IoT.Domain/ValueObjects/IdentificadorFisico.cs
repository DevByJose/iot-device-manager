using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Identificador único del dispositivo físico (MAC, serial). Inmutable.
/// </summary>
public sealed class IdentificadorFisico : ValueObject
{
    public string Valor { get; private set; }
    public string TipoIdentificador { get; private set; }

    private IdentificadorFisico() { Valor = string.Empty; TipoIdentificador = string.Empty; }

    public IdentificadorFisico(string valor, string tipoIdentificador)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new DomainException("El identificador físico no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(tipoIdentificador))
            throw new DomainException("El tipo de identificador no puede estar vacío.");

        Valor = valor;
        TipoIdentificador = tipoIdentificador;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
        yield return TipoIdentificador;
    }
}
