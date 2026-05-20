namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar datos de Habitacion.
/// </summary>
public sealed record HabitacionDto(int Id, string Nombre, int HogarId);
