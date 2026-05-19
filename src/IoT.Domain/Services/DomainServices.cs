using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Services;

/// <summary>
/// Coordina el registro de un Dispositivo verificando unicidad global del identificador (SRP).
/// </summary>
public class SvcRegistroDispositivo
{
    private readonly IDispositivoRepository _dispositivoRepo;

    public SvcRegistroDispositivo(IDispositivoRepository dispositivoRepo)
    {
        _dispositivoRepo = dispositivoRepo;
    }

    public async Task<Dispositivo> RegistrarAsync(Hogar hogar, int dispositivoId, string nombre,
        string tipoDispositivo, IdentificadorFisico identificador, VersionFirmware firmware, int habitacionId)
    {
        // Regla: Identificador físico único globalmente
        if (await _dispositivoRepo.ExisteIdentificadorAsync(identificador.Valor))
            throw new DomainException($"Ya existe un dispositivo con el identificador '{identificador.Valor}'.");

        // Delega al agregado raíz (Hogar) para mantener consistencia
        return hogar.RegistrarDispositivo(dispositivoId, nombre, tipoDispositivo, identificador, firmware, habitacionId);
    }
}

/// <summary>
/// Traduce una Escena en ComandoDispositivo secuenciales (SRP).
/// </summary>
public class SvcEjecucionEscena
{
    private readonly IDispositivoRepository _dispositivoRepo;

    public SvcEjecucionEscena(IDispositivoRepository dispositivoRepo)
    {
        _dispositivoRepo = dispositivoRepo;
    }

    public async Task<List<ComandoDispositivo>> EjecutarAsync(Escena escena, string origen)
    {
        var acciones = escena.Ejecutar(origen);
        var comandos = new List<ComandoDispositivo>();
        int comandoId = 1;

        foreach (var accion in acciones)
        {
            var dispositivo = await _dispositivoRepo.GetByIdAsync(accion.DispositivoId);
            if (dispositivo == null || !dispositivo.PuedeEjecutarComando())
            {
                // Política: si un dispositivo no está disponible, se marca fallo y continúa
                var comandoFallido = new ComandoDispositivo(comandoId++, accion.DispositivoId, accion.Comando);
                comandoFallido.MarcarFallido("Dispositivo no disponible");
                comandos.Add(comandoFallido);
                continue;
            }

            var parametros = accion.Parametro != null ? new List<ParametroComando> { accion.Parametro } : null;
            var comando = new ComandoDispositivo(comandoId++, accion.DispositivoId, accion.Comando, parametros);
            comando.MarcarEnviado();
            comandos.Add(comando);
        }

        return comandos;
    }
}

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

/// <summary>
/// Recibe LecturaSensor y consolida EstadoDispositivo (SRP).
/// </summary>
public class SvcConsolidacionEstado
{
    public void Consolidar(EstadoDispositivo estado, LecturaSensor lectura)
    {
        estado.ActualizarLectura(lectura);
    }
}

/// <summary>
/// Evalúa si una lectura cruza un UmbralSensor y genera AlertaEstado (SRP).
/// </summary>
public class SvcDeteccionAnomalia
{
    public AlertaEstado? Evaluar(EstadoDispositivo estado, UmbralSensor umbral, int alertaId)
    {
        return estado.DetectarAnomalia(umbral, alertaId);
    }
}
