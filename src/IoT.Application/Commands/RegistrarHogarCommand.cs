namespace IoT.Application.Commands;

/// <summary>
/// Intención de registrar un nuevo hogar. Inmutable (SRP).
/// </summary>
public sealed record RegistrarHogarCommand(
    string Nombre, string Calle, string Numero, string Ciudad,
    string Pais, string CodigoPostal, int ClienteId);
