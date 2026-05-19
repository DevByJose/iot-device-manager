using IoT.Domain.BuildingBlocks;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Dato puntual reportado por un dispositivo en un momento dado.
/// </summary>
public class LecturaSensor : Entity
{
    public int DispositivoId { get; private set; }
    public double Valor { get; private set; }
    public string Unidad { get; private set; }
    public DateTime Timestamp { get; private set; }

    private LecturaSensor() { Unidad = string.Empty; }

    public LecturaSensor(int id, int dispositivoId, double valor, string unidad, DateTime timestamp)
    {
        Id = id;
        DispositivoId = dispositivoId;
        Valor = valor;
        Unidad = unidad ?? throw new DomainException("La unidad de lectura es obligatoria.");
        Timestamp = timestamp;
    }
}

/// <summary>
/// Materialización de una condición que cruza un umbral relevante.
/// </summary>
public class AlertaEstado : Entity
{
    public int DispositivoId { get; private set; }
    public string Tipo { get; private set; }
    public string Descripcion { get; private set; }
    public double ValorLeido { get; private set; }
    public DateTime Generada { get; private set; }
    public bool Resuelta { get; private set; }

    private AlertaEstado() { Tipo = string.Empty; Descripcion = string.Empty; }

    public AlertaEstado(int id, int dispositivoId, string tipo, string descripcion, double valorLeido)
    {
        Id = id;
        DispositivoId = dispositivoId;
        Tipo = tipo;
        Descripcion = descripcion;
        ValorLeido = valorLeido;
        Generada = DateTime.UtcNow;
        Resuelta = false;
    }

    public void Resolver() => Resuelta = true;
}

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
