namespace IoT.Application.Commands;

/// <summary>
/// Intención de crear una nueva escena. Inmutable.
/// </summary>
public sealed record CrearEscenaCommand(
    int HogarId, string Nombre, List<AccionEscenaInput> Acciones);
