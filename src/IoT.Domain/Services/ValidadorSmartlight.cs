namespace IoT.Domain.Services;

public class ValidadorSmartlight : IValidadorTipoDispositivo
{
    private static readonly HashSet<string> Comandos = new() { "TurnOn", "TurnOff", "SetColor", "SetSchedule" };
    public string Tipo => "Smartlight";
    public bool EsComandoValido(string comando) => Comandos.Contains(comando);
}
