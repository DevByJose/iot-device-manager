namespace IoT.Application.Queries;

/// <summary>
/// Intención de lectura: consultar estado actual de un dispositivo.
/// </summary>
public sealed record ConsultarEstadoQuery(int DispositivoId);
