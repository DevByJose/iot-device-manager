using IoT.Domain.BuildingBlocks;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: EstadoDispositivo — Raíz del agregado de Monitoreo del Estado.
/// Consolida lecturas de sensor y detecta anomalías.
/// </summary>
public class EstadoDispositivo : AggregateRoot
{
    public int DispositivoId { get; private set; }
    public string EstadoActual { get; private set; }
    public double? UltimoValorReportado { get; private set; }
    public DateTime UltimaActualizacion { get; private set; }
    public bool Conectado { get; private set; }

    private readonly List<LecturaSensor> _lecturas = new();
    public IReadOnlyCollection<LecturaSensor> Lecturas => _lecturas.AsReadOnly();

    private readonly List<AlertaEstado> _alertas = new();
    public IReadOnlyCollection<AlertaEstado> Alertas => _alertas.AsReadOnly();

    private EstadoDispositivo() { EstadoActual = "Desconocido"; }

    public EstadoDispositivo(int id, int dispositivoId)
    {
        Id = id;
        DispositivoId = dispositivoId;
        EstadoActual = "Offline";
        Conectado = false;
        UltimaActualizacion = DateTime.UtcNow;
    }

    /// <summary>
    /// Actualiza el estado con una nueva lectura. Eleva EstadoCambiado si hay cambio real.
    /// Solo acepta lecturas más recientes que la última consolidada.
    /// </summary>
    public void ActualizarLectura(LecturaSensor lectura)
    {
        if (lectura.Timestamp <= UltimaActualizacion)
            return; // Lectura duplicada o antigua, se descarta

        var estadoAnterior = EstadoActual;
        UltimoValorReportado = lectura.Valor;
        UltimaActualizacion = lectura.Timestamp;
        EstadoActual = "Online";
        Conectado = true;

        _lecturas.Add(lectura);

        if (estadoAnterior != EstadoActual)
        {
            AddDomainEvent(new EstadoCambiado(DispositivoId, estadoAnterior, EstadoActual));
        }
    }

    /// <summary>
    /// Evalúa si la última lectura cruza un umbral y genera una alerta.
    /// </summary>
    public AlertaEstado? DetectarAnomalia(UmbralSensor umbral, int alertaId)
    {
        if (UltimoValorReportado == null) return null;

        if (umbral.EstaFueraDeRango(UltimoValorReportado.Value))
        {
            var alerta = new AlertaEstado(alertaId, DispositivoId,
                "UmbralExcedido",
                $"Valor {UltimoValorReportado} fuera de rango [{umbral.ValorMin}, {umbral.ValorMax}] {umbral.Unidad}",
                UltimoValorReportado.Value);

            _alertas.Add(alerta);
            AddDomainEvent(new AnomaliaDetectada(DispositivoId, UltimoValorReportado.Value, umbral.ValorMin, umbral.ValorMax));
            return alerta;
        }

        return null;
    }

    /// <summary>
    /// Marca el dispositivo como desconectado si no reporta en el tiempo configurado.
    /// </summary>
    public void MarcarDesconectado()
    {
        if (Conectado)
        {
            var estadoAnterior = EstadoActual;
            Conectado = false;
            EstadoActual = "Desconectado";
            AddDomainEvent(new DispositivoDesconectado(DispositivoId, UltimaActualizacion));

            if (estadoAnterior != EstadoActual)
                AddDomainEvent(new EstadoCambiado(DispositivoId, estadoAnterior, EstadoActual));
        }
    }
}
