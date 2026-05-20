namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar datos de Dispositivo.
/// </summary>
public sealed record DispositivoDto(int Id, string Nombre, string TipoDispositivo, string Identificador,
    string Estado, bool Conectado, string Firmware, string Habitacion);
