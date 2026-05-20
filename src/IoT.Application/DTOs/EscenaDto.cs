namespace IoT.Application.DTOs;

/// <summary>
/// DTO para transportar datos de Escena.
/// </summary>
public sealed record EscenaDto(int Id, string Nombre, bool Activa, int TotalAcciones, int TotalDisparadores);
