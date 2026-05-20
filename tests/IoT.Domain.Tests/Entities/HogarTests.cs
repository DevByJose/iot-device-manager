using IoT.Domain.Entities;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Tests.Entities;

public class HogarTests
{
    private static Hogar CrearHogar(int id = 1)
    {
        var direccion = new DireccionFisica("Calle 10", "45", "Medellín", "Colombia", "050001");
        return new Hogar(id, "Casa Principal", direccion, clienteId: 1);
    }

    private static IdentificadorFisico CrearId(string valor = "AA:BB:CC:DD:EE:FF")
        => new IdentificadorFisico(valor, "MAC");

    private static VersionFirmware CrearFirmware()
        => new VersionFirmware(1, 0, 0);

    // ── Constructor ──────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_NombreVacio_LanzaExcepcion()
    {
        var dir = new DireccionFisica("Calle 10", "45", "Medellín", "Colombia", "050001");
        Assert.Throws<DomainException>(() => new Hogar(1, "", dir, 1));
    }

    [Fact]
    public void Constructor_DireccionNula_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => new Hogar(1, "Casa", null!, 1));
    }

    // ── AgregarHabitacion ────────────────────────────────────────────────────

    [Fact]
    public void AgregarHabitacion_NombreNuevo_AgregaAlColeccion()
    {
        var hogar = CrearHogar();
        var hab = hogar.AgregarHabitacion(0, "Sala");
        Assert.Single(hogar.Habitaciones);
        Assert.Equal("Sala", hab.Nombre);
    }

    [Fact]
    public void AgregarHabitacion_NombreDuplicado_LanzaExcepcion()
    {
        var hogar = CrearHogar();
        hogar.AgregarHabitacion(0, "Sala");
        Assert.Throws<DomainException>(() => hogar.AgregarHabitacion(0, "Sala"));
    }

    [Fact]
    public void AgregarHabitacion_NombreDuplicadoCaseInsensitive_LanzaExcepcion()
    {
        var hogar = CrearHogar();
        hogar.AgregarHabitacion(0, "sala");
        Assert.Throws<DomainException>(() => hogar.AgregarHabitacion(0, "SALA"));
    }

    // ── RegistrarDispositivo ─────────────────────────────────────────────────

    [Fact]
    public void RegistrarDispositivo_HabitacionValida_AgregaDispositivo()
    {
        var hogar = CrearHogar(1);
        var hab = hogar.AgregarHabitacion(1, "Sala");

        var disp = hogar.RegistrarDispositivo(0, "Luz Sala", "Smartlight",
            CrearId(), CrearFirmware(), hab.Id);

        Assert.Single(hogar.Dispositivos);
        Assert.Equal("Luz Sala", disp.Nombre);
    }

    [Fact]
    public void RegistrarDispositivo_HabitacionAjena_LanzaExcepcion()
    {
        var hogar = CrearHogar(1);
        // habitacionId=99 no pertenece a este hogar
        Assert.Throws<DomainException>(() =>
            hogar.RegistrarDispositivo(0, "Luz", "Smartlight", CrearId(), CrearFirmware(), habitacionId: 99));
    }

    [Fact]
    public void RegistrarDispositivo_ElevaEventoDispositivoRegistrado()
    {
        var hogar = CrearHogar(1);
        var hab = hogar.AgregarHabitacion(1, "Sala");
        hogar.RegistrarDispositivo(0, "Luz", "Smartlight", CrearId(), CrearFirmware(), hab.Id);

        Assert.Contains(hogar.DomainEvents, e => e is DispositivoRegistrado);
    }

    // ── ConectarDispositivo / DesconectarDispositivo ──────────────────────────

    [Fact]
    public void ConectarDispositivo_DispositivoExistente_CambiaEstadoOnline()
    {
        var hogar = CrearHogar(1);
        var hab = hogar.AgregarHabitacion(1, "Sala");
        hogar.RegistrarDispositivo(1, "Cam", "Camera", CrearId(), CrearFirmware(), hab.Id);

        hogar.ConectarDispositivo(1);

        var disp = hogar.Dispositivos.First(d => d.Id == 1);
        Assert.Equal("Online", disp.Estado);
        Assert.True(disp.EstaConectado);
    }

    [Fact]
    public void DesconectarDispositivo_DispositivoInexistente_LanzaExcepcion()
    {
        var hogar = CrearHogar(1);
        Assert.Throws<DomainException>(() => hogar.DesconectarDispositivo(999));
    }

    // ── DesinstalarDispositivo ────────────────────────────────────────────────

    [Fact]
    public void DesinstalarDispositivo_DispositivoExistente_LoEliminaYElevaEvento()
    {
        var hogar = CrearHogar(1);
        var hab = hogar.AgregarHabitacion(1, "Sala");
        hogar.RegistrarDispositivo(1, "Luz", "Smartlight", CrearId(), CrearFirmware(), hab.Id);

        hogar.DesinstalarDispositivo(1);

        Assert.Empty(hogar.Dispositivos);
        Assert.Contains(hogar.DomainEvents, e => e is DispositivoDesinstalado);
    }
}
