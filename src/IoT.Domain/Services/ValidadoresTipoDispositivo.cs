namespace IoT.Domain.Services;

/// <summary>
/// Contrato para validar comandos por tipo de dispositivo (OCP).
/// Agregar un nuevo tipo = crear una nueva clase que implemente esta interfaz.
/// No se modifica ninguna clase existente.
/// </summary>
public interface IValidadorTipoDispositivo
{
    string Tipo { get; }
    bool EsComandoValido(string comando);
}

public class ValidadorSmartlight : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "TurnOn", "TurnOff", "SetColor", "SetSchedule" };
    public string Tipo => "Smartlight";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}

public class ValidadorCamera : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "StartRecording", "CaptureSnapshot", "TurnOn", "TurnOff" };
    public string Tipo => "Camera";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}

public class ValidadorAlarm : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "Trigger", "Stop", "TurnOn", "TurnOff" };
    public string Tipo => "Alarm";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}
