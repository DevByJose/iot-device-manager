using IoT.Domain.Entities;
using IoT.Domain.Exceptions;

namespace IoT.Domain.Services;

/// <summary>
/// Verifica que un ComandoDispositivo sea válido para el tipo de dispositivo (SRP).
/// </summary>
public class SvcValidacionComando
{
    private static readonly Dictionary<string, HashSet<string>> ComandosPorTipo = new()
    {
        ["Smartlight"] = new() { "TurnOn", "TurnOff", "SetColor", "SetSchedule" },
        ["Camera"] = new() { "StartRecording", "CaptureSnapshot", "TurnOn", "TurnOff" },
        ["Alarm"] = new() { "Trigger", "Stop", "TurnOn", "TurnOff" }
    };

    public void Validar(Dispositivo dispositivo, string comando)
    {
        if (!dispositivo.PuedeEjecutarComando())
            throw new DomainException($"El dispositivo '{dispositivo.Nombre}' no admite comandos en estado '{dispositivo.Estado}'.");

        if (ComandosPorTipo.TryGetValue(dispositivo.TipoDispositivo, out var comandosPermitidos))
        {
            if (!comandosPermitidos.Contains(comando))
                throw new DomainException($"El comando '{comando}' no es válido para dispositivos tipo '{dispositivo.TipoDispositivo}'.");
        }
    }
}
