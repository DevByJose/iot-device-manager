namespace IoT.Application.Commands;

/// <summary>
/// Intención de agregar una habitación a un hogar existente. Inmutable.
/// </summary>
public sealed record AgregarHabitacionCommand(int HogarId, string Nombre);
