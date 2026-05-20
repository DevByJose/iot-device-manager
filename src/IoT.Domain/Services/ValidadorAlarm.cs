namespace IoT.Domain.Services;

public class ValidadorAlarm : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "Trigger", "Stop", "TurnOn", "TurnOff" };
    public string Tipo => "Alarm";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}
