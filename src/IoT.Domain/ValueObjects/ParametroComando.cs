using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Argumento que acompaña a un ComandoDispositivo. Inmutable.
/// </summary>
public sealed class ParametroComando : ValueObject
{
    public string Nombre { get; init; }
    public string Valor { get; init; }
    public string Tipo { get; init; }

    private ParametroComando() { Nombre = string.Empty; Valor = string.Empty; Tipo = string.Empty; }

    public ParametroComando(string nombre, string valor, string tipo)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del parámetro no puede estar vacío.");

        Nombre = nombre;
        Valor = valor ?? string.Empty;
        Tipo = tipo ?? "string";
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Nombre;
        yield return Valor;
        yield return Tipo;
    }
}
