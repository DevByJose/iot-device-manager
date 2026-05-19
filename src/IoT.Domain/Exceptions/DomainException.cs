namespace IoT.Domain.Exceptions;

/// <summary>
/// Excepción base del dominio. Se lanza cuando se viola una invariante de negocio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
    public DomainException(string message, Exception inner) : base(message, inner) { }
}
