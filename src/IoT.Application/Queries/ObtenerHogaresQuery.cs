namespace IoT.Application.Queries;

/// <summary>
/// Intención de lectura: obtener todos los hogares de un cliente.
/// </summary>
public sealed record ObtenerHogaresQuery(int ClienteId);
