using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Services;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Tests.Services;

public class SvcValidacionComandoTests
{
    private readonly SvcValidacionComando _svc = new(new IValidadorTipoDispositivo[]
    {
        new ValidadorSmartlight(),
        new ValidadorCamera(),
        new ValidadorAlarm()
    });

    private static Dispositivo CrearDispositivo(string tipo, bool conectado = true)
    {
        var id = new IdentificadorFisico("AA:BB:CC:00:00:01", "MAC");
        var fw = new VersionFirmware(1, 0, 0);
        var disp = new Dispositivo(1, "Dispositivo Test", tipo, id, fw, habitacionId: 1, hogarId: 1);
        if (conectado) disp.Conectar();
        return disp;
    }

    // ── Dispositivo no disponible ─────────────────────────────────────────────

    [Fact]
    public void Validar_DispositivoDesconectado_LanzaExcepcion()
    {
        var disp = CrearDispositivo("Smartlight", conectado: false);
        Assert.Throws<DomainException>(() => _svc.Validar(disp, "TurnOn"));
    }

    // ── Smartlight ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("TurnOn")]
    [InlineData("TurnOff")]
    [InlineData("SetColor")]
    [InlineData("SetSchedule")]
    public void Validar_SmartlightComandoValido_NoLanzaExcepcion(string comando)
    {
        var disp = CrearDispositivo("Smartlight");
        var ex = Record.Exception(() => _svc.Validar(disp, comando));
        Assert.Null(ex);
    }

    [Fact]
    public void Validar_SmartlightComandoInvalido_LanzaExcepcion()
    {
        var disp = CrearDispositivo("Smartlight");
        Assert.Throws<DomainException>(() => _svc.Validar(disp, "StartRecording"));
    }

    // ── Camera ────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("StartRecording")]
    [InlineData("CaptureSnapshot")]
    [InlineData("TurnOn")]
    [InlineData("TurnOff")]
    public void Validar_CameraComandoValido_NoLanzaExcepcion(string comando)
    {
        var disp = CrearDispositivo("Camera");
        var ex = Record.Exception(() => _svc.Validar(disp, comando));
        Assert.Null(ex);
    }

    [Fact]
    public void Validar_CameraComandoInvalido_LanzaExcepcion()
    {
        var disp = CrearDispositivo("Camera");
        Assert.Throws<DomainException>(() => _svc.Validar(disp, "Trigger"));
    }

    // ── Alarm ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("Trigger")]
    [InlineData("Stop")]
    [InlineData("TurnOn")]
    [InlineData("TurnOff")]
    public void Validar_AlarmComandoValido_NoLanzaExcepcion(string comando)
    {
        var disp = CrearDispositivo("Alarm");
        var ex = Record.Exception(() => _svc.Validar(disp, comando));
        Assert.Null(ex);
    }

    [Fact]
    public void Validar_AlarmComandoInvalido_LanzaExcepcion()
    {
        var disp = CrearDispositivo("Alarm");
        Assert.Throws<DomainException>(() => _svc.Validar(disp, "SetColor"));
    }
}
