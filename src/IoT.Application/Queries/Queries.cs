namespace IoT.Application.Queries;

/// <summary>
/// Intención de lectura: obtener dispositivos de un hogar. Inmutable (SRP).
/// </summary>
public sealed record ObtenerDispositivosQuery(int HogarId);

/// <summary>
/// Intención de lectura: consultar estado actual de un dispositivo.
/// </summary>
public sealed record ConsultarEstadoQuery(int DispositivoId);

/// <summary>
/// Intención de lectura: obtener lecturas de telemetría en un rango.
/// </summary>
public sealed record ObtenerTelemetriaQuery(int DispositivoId, DateTime Desde, DateTime Hasta);

/// <summary>
/// Intención de lectura: obtener todos los hogares de un cliente.
/// </summary>
public sealed record ObtenerHogaresQuery(int ClienteId);
