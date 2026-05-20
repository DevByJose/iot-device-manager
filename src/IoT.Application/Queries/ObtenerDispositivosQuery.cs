namespace IoT.Application.Queries;

/// <summary>
/// Intención de lectura: obtener dispositivos de un hogar. Inmutable (SRP).
/// </summary>
public sealed record ObtenerDispositivosQuery(int HogarId);
