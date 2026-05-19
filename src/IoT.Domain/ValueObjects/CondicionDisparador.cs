using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;

namespace IoT.Domain.ValueObjects;

/// <summary>
/// Expresión declarativa que evalúa un disparador. Inmutable.
/// </summary>
public sealed class CondicionDisparador : ValueObject
{
    private static readonly HashSet<string> OperadoresValidos = new() { ">", "<", "=", "!=", ">=", "<=" };

    public string OperandoIzquierdo { get; private set; }
    public string Operador { get; private set; }
    public string OperandoDerecho { get; private set; }

    private CondicionDisparador() { OperandoIzquierdo = string.Empty; Operador = string.Empty; OperandoDerecho = string.Empty; }

    public CondicionDisparador(string operandoIzquierdo, string operador, string operandoDerecho)
    {
        if (string.IsNullOrWhiteSpace(operandoIzquierdo))
            throw new DomainException("El operando izquierdo no puede estar vacío.");
        if (!OperadoresValidos.Contains(operador))
            throw new DomainException($"Operador '{operador}' no válido. Debe ser: {string.Join(", ", OperadoresValidos)}");
        if (string.IsNullOrWhiteSpace(operandoDerecho))
            throw new DomainException("El operando derecho no puede estar vacío.");

        OperandoIzquierdo = operandoIzquierdo;
        Operador = operador;
        OperandoDerecho = operandoDerecho;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OperandoIzquierdo;
        yield return Operador;
        yield return OperandoDerecho;
    }
}
