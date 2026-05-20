using IoT.Domain.Entities;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Tests.Entities;

public class EscenaTests
{
    private static Escena CrearEscena(string nombre = "Buenas Noches", int hogarId = 1)
        => new Escena(1, new NombreEscena(nombre), hogarId);

    // ── Ejecutar ─────────────────────────────────────────────────────────────

    [Fact]
    public void Ejecutar_EscenaActivaConAcciones_RetornaAccionesOrdenadas()
    {
        var escena = CrearEscena();
        escena.AgregarAccion(1, orden: 2, dispositivoId: 10, "TurnOff");
        escena.AgregarAccion(2, orden: 1, dispositivoId: 20, "TurnOn");

        var acciones = escena.Ejecutar("manual");

        Assert.Equal(2, acciones.Count);
        Assert.Equal(1, acciones[0].Orden); // orden correcto
    }

    [Fact]
    public void Ejecutar_EscenaInactiva_LanzaExcepcion()
    {
        var escena = CrearEscena();
        escena.AgregarAccion(1, 1, 10, "TurnOn");
        escena.Desactivar();

        Assert.Throws<DomainException>(() => escena.Ejecutar("manual"));
    }

    [Fact]
    public void Ejecutar_SinAcciones_LanzaExcepcion()
    {
        var escena = CrearEscena();
        Assert.Throws<DomainException>(() => escena.Ejecutar("manual"));
    }

    [Fact]
    public void Ejecutar_EscenaValida_ElevaEventoEscenaEjecutada()
    {
        var escena = CrearEscena();
        escena.AgregarAccion(1, 1, 10, "TurnOn");
        escena.ClearDomainEvents();

        escena.Ejecutar("manual");

        Assert.Contains(escena.DomainEvents, e => e is EscenaEjecutada);
    }

    // ── ConfirmarCreacion ─────────────────────────────────────────────────────

    [Fact]
    public void ConfirmarCreacion_ElevaEscenaCreadaConIdCorrecto()
    {
        var escena = CrearEscena();
        escena.ClearDomainEvents();

        escena.ConfirmarCreacion();

        var evento = Assert.Single(escena.DomainEvents.OfType<EscenaCreada>());
        Assert.Equal(escena.Id, evento.EscenaId);
    }
}

public class ComandoDispositivoTests
{
    // ── Máquina de estados ────────────────────────────────────────────────────

    [Fact]
    public void MarcarEnviado_EstadoPendiente_CambiaAEnviado()
    {
        var cmd = new ComandoDispositivo(0, dispositivoId: 1, "TurnOn");
        cmd.MarcarEnviado();
        Assert.Equal("Enviado", cmd.Estado);
    }

    [Fact]
    public void MarcarEnviado_EstadoPendiente_ElevaEventoComandoEnviado()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        cmd.MarcarEnviado();
        Assert.Contains(cmd.DomainEvents, e => e is ComandoEnviado);
    }

    [Fact]
    public void MarcarEnviado_EstadoNoEsPendiente_LanzaExcepcion()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        cmd.MarcarEnviado();
        // intentar enviar de nuevo desde Enviado debe fallar
        Assert.Throws<DomainException>(() => cmd.MarcarEnviado());
    }

    [Fact]
    public void Confirmar_DesdeEnviado_CambiaAConfirmado()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        cmd.MarcarEnviado();
        cmd.Confirmar();
        Assert.Equal("Confirmado", cmd.Estado);
    }

    [Fact]
    public void Confirmar_DesdePendiente_LanzaExcepcion()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        // saltar el estado Enviado debe fallar
        Assert.Throws<DomainException>(() => cmd.Confirmar());
    }

    [Fact]
    public void MarcarFallido_CualquierEstado_CambiaAFallido()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        cmd.MarcarFallido("Dispositivo no disponible");
        Assert.Equal("Fallido", cmd.Estado);
        Assert.Equal("Dispositivo no disponible", cmd.MotivoFallo);
    }

    [Fact]
    public void MarcarFallido_ElevaEventoComandoFallido()
    {
        var cmd = new ComandoDispositivo(0, 1, "TurnOn");
        cmd.MarcarFallido("Sin conexión");
        Assert.Contains(cmd.DomainEvents, e => e is ComandoFallido);
    }
}
