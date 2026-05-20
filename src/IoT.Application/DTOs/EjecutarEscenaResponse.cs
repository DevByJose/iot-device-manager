namespace IoT.Application.DTOs;

/// <summary>
/// Respuesta al ejecutar una escena.
/// </summary>
public sealed record EjecutarEscenaResponse(int EscenaId, int ComandosEnviados, int ComandosFallidos, List<string> Detalles);
