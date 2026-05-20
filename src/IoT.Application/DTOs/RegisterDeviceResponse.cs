namespace IoT.Application.DTOs;

/// <summary>
/// Respuesta al registrar un dispositivo.
/// </summary>
public sealed record RegisterDeviceResponse(int DispositivoId, string Nombre, string TipoDispositivo, bool Success, string Message);
