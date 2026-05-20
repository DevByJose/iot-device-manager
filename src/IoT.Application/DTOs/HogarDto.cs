namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar datos de Hogar entre capas. Sin comportamiento (SRP).
/// </summary>
public sealed record HogarDto(int Id, string Nombre, string Ciudad, string Pais, int TotalDispositivos, int TotalHabitaciones);
