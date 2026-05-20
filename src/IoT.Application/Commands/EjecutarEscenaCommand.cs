namespace IoT.Application.Commands;

/// <summary>
/// Intención de ejecutar una escena existente. Inmutable.
/// </summary>
public sealed record EjecutarEscenaCommand(int EscenaId, string Origen);
