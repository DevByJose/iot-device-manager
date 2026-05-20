namespace IoT.Application.DTOs;

/// <summary>
/// DTO para lectura de sensor individual.
/// </summary>
public sealed record LecturaSensorDto(double Valor, string Unidad, DateTime Timestamp);
