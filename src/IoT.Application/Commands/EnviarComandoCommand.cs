namespace IoT.Application.Commands;

/// <summary>
/// Intención de enviar un comando a un dispositivo. Inmutable.
/// </summary>
public sealed record EnviarComandoCommand(int DispositivoId, string Comando, Dictionary<string, string>? Parametros = null);
