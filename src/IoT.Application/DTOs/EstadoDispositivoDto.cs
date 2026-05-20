namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar estado de dispositivo.
/// </summary>
public sealed record EstadoDispositivoDto(int DispositivoId, string Estado, double? UltimoValor,
    DateTime UltimaActualizacion, bool Conectado, int TotalAlertas);
