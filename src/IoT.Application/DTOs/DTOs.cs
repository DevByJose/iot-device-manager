namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar datos de Hogar entre capas. Sin comportamiento (SRP).
/// </summary>
public sealed record HogarDto(int Id, string Nombre, string Ciudad, string Pais, int TotalDispositivos, int TotalHabitaciones);

/// <summary>
/// DTO para transportar datos de Dispositivo.
/// </summary>
public sealed record DispositivoDto(int Id, string Nombre, string TipoDispositivo, string Identificador,
    string Estado, bool Conectado, string Firmware, string Habitacion);

/// <summary>
/// DTO para transportar datos de Escena.
/// </summary>
public sealed record EscenaDto(int Id, string Nombre, bool Activa, int TotalAcciones, int TotalDisparadores);

/// <summary>
/// DTO para transportar estado de dispositivo.
/// </summary>
public sealed record EstadoDispositivoDto(int DispositivoId, string Estado, double? UltimoValor,
    DateTime UltimaActualizacion, bool Conectado, int TotalAlertas);

/// <summary>
/// DTO para lectura de sensor individual.
/// </summary>
public sealed record LecturaSensorDto(double Valor, string Unidad, DateTime Timestamp);

/// <summary>
/// Respuesta al registrar un dispositivo.
/// </summary>
public sealed record RegisterDeviceResponse(int DispositivoId, string Nombre, string TipoDispositivo, bool Success, string Message);

/// <summary>
/// Respuesta al ejecutar una escena.
/// </summary>
public sealed record EjecutarEscenaResponse(int EscenaId, int ComandosEnviados, int ComandosFallidos, List<string> Detalles);

/// <summary>
/// DTO para transportar datos de Habitacion.
/// </summary>
public sealed record HabitacionDto(int Id, string Nombre, int HogarId);

/// <summary>
/// DTO de respuesta al enviar un comando a un dispositivo.
/// </summary>
public sealed record ComandoDispositivoDto(int Id, int DispositivoId, string Comando, string Estado, DateTime CreadoEn);
