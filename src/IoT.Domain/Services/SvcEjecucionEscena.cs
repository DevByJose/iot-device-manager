using IoT.Domain.Entities;
using IoT.Domain.Interfaces;

namespace IoT.Domain.Services;

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

            var parametros = accion.Parametro != null ? new List<ValueObjects.ParametroComando> { accion.Parametro } : null;
            var comando = new ComandoDispositivo(comandoId++, accion.DispositivoId, accion.Comando, parametros);
            comando.MarcarEnviado();
            comandos.Add(comando);
        }

        return comandos;
    }
}
