using IoT.Domain.Entities;
using IoT.Domain.Events;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Tests.Entities;

public class EstadoDispositivoTests
{
    private static LecturaSensor CrearLectura(double valor = 22.5, DateTime? timestamp = null)
        => new LecturaSensor(0, dispositivoId: 1, valor, "°C", timestamp ?? DateTime.UtcNow);

    // ── ActualizarLectura ─────────────────────────────────────────────────────

    [Fact]
    public void ActualizarLectura_LecturaNueva_ActualizaEstadoYAgregaLectura()
    {
        var estado = new EstadoDispositivo(1, dispositivoId: 1);
        var lectura = CrearLectura(25.0);

        estado.ActualizarLectura(lectura);

        Assert.Equal(25.0, estado.UltimoValorReportado);
        Assert.Equal("Online", estado.EstadoActual);
        Assert.Single(estado.Lecturas);
    }

    [Fact]
    public void ActualizarLectura_LecturaAntigua_SeDescarta()
    {
        var estado = new EstadoDispositivo(1, 1);
        var ahora = DateTime.UtcNow;
        estado.ActualizarLectura(CrearLectura(25.0, ahora));

        // lectura con timestamp anterior a la última consolidada
        estado.ActualizarLectura(CrearLectura(30.0, ahora.AddSeconds(-10)));

        Assert.Equal(25.0, estado.UltimoValorReportado); // no cambió
        Assert.Single(estado.Lecturas);                   // solo la primera
    }

    [Fact]
    public void ActualizarLectura_CambioDeEstado_ElevaEventoEstadoCambiado()
    {
        var estado = new EstadoDispositivo(1, 1); // empieza Offline
        estado.ActualizarLectura(CrearLectura());

        Assert.Contains(estado.DomainEvents, e => e is EstadoCambiado);
    }

    [Fact]
    public void ActualizarLectura_MismoEstado_NoElevaEvento()
    {
        var estado = new EstadoDispositivo(1, 1);
        var ahora = DateTime.UtcNow;
        estado.ActualizarLectura(CrearLectura(22.0, ahora));
        estado.ClearDomainEvents();

        // segunda lectura: estado ya es Online, no debe re-elevarse
        estado.ActualizarLectura(CrearLectura(23.0, ahora.AddSeconds(5)));

        Assert.DoesNotContain(estado.DomainEvents, e => e is EstadoCambiado);
    }

    // ── DetectarAnomalia ──────────────────────────────────────────────────────

    [Fact]
    public void DetectarAnomalia_ValorFueraDeRango_CreaAlertaYElevaEvento()
    {
        var estado = new EstadoDispositivo(1, 1);
        estado.ActualizarLectura(CrearLectura(95.0));
        var umbral = new UmbralSensor(0.0, 50.0, "°C");

        var alerta = estado.DetectarAnomalia(umbral, alertaId: 0);

        Assert.NotNull(alerta);
        Assert.Single(estado.Alertas);
        Assert.Contains(estado.DomainEvents, e => e is AnomaliaDetectada);
    }

    [Fact]
    public void DetectarAnomalia_ValorDentroDeRango_NoGeneraAlerta()
    {
        var estado = new EstadoDispositivo(1, 1);
        estado.ActualizarLectura(CrearLectura(25.0));
        var umbral = new UmbralSensor(0.0, 50.0, "°C");

        var alerta = estado.DetectarAnomalia(umbral, alertaId: 0);

        Assert.Null(alerta);
        Assert.Empty(estado.Alertas);
    }

    // ── MarcarDesconectado ────────────────────────────────────────────────────

    [Fact]
    public void MarcarDesconectado_DispositivoConectado_ElevaEventoDesconectado()
    {
        var estado = new EstadoDispositivo(1, 1);
        estado.ActualizarLectura(CrearLectura()); // pone Conectado = true
        estado.ClearDomainEvents();

        estado.MarcarDesconectado();

        Assert.False(estado.Conectado);
        Assert.Contains(estado.DomainEvents, e => e is DispositivoDesconectado);
    }
}
