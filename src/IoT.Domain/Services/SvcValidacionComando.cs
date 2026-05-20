using IoT.Domain.Entities;
using IoT.Domain.Exceptions;

namespace IoT.Domain.Services;

/// <summary>
/// Verifica disponibilidad del dispositivo y delega la validación de comando
/// al validador correspondiente al tipo (OCP — cerrado a modificación, abierto a extensión).
/// </summary>
public class SvcValidacionComando
{
    private readonly IReadOnlyDictionary<string, IValidadorTipoDispositivo> _validadores;

    public SvcValidacionComando(IEnumerable<IValidadorTipoDispositivo> validadores)
    {
        _validadores = validadores.ToDictionary(v => v.Tipo, StringComparer.OrdinalIgnoreCase);
    }

    public void Validar(Dispositivo dispositivo, string comando)
    {
        if (!dispositivo.PuedeEjecutarComando())
            throw new DomainException(
                $"El dispositivo '{dispositivo.Nombre}' no admite comandos en estado '{dispositivo.Estado}'.");

        if (_validadores.TryGetValue(dispositivo.TipoDispositivo, out var validador)
            && !validador.EsComandoValido(comando))
        {
            throw new DomainException(
                $"El comando '{comando}' no es válido para dispositivos tipo '{dispositivo.TipoDispositivo}'.");
        }
    }
}
