namespace IoT.Application.Queries;

/// <summary>
/// Intención de lectura: obtener lecturas de telemetría en un rango.
/// </summary>
public sealed record ObtenerTelemetriaQuery(int DispositivoId, DateTime Desde, DateTime Hasta);
