namespace IoT.Domain.Services;

public class ValidadorCamera : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "StartRecording", "CaptureSnapshot", "TurnOn", "TurnOff" };
    public string Tipo => "Camera";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}
