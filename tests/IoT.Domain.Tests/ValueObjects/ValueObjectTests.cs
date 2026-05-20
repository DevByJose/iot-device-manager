using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Tests.ValueObjects;

public class NombreEscenaTests
{
    [Fact]
    public void Constructor_NombreValido_CreaInstancia()
    {
        var nombre = new NombreEscena("Buenas Noches");
        Assert.Equal("Buenas Noches", nombre.Valor);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_NombreVacio_LanzaExcepcion(string? valor)
    {
        Assert.Throws<DomainException>(() => new NombreEscena(valor!));
    }

    [Fact]
    public void Constructor_NombreMenorDe3Caracteres_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => new NombreEscena("AB"));
    }

    [Fact]
    public void Constructor_NombreMayorDe60Caracteres_LanzaExcepcion()
    {
        var nombreLargo = new string('A', 61);
        Assert.Throws<DomainException>(() => new NombreEscena(nombreLargo));
    }

    [Fact]
    public void Igualdad_MismoValor_SonIguales()
    {
        var a = new NombreEscena("Despertar");
        var b = new NombreEscena("Despertar");
        Assert.Equal(a, b);
    }

    [Fact]
    public void Igualdad_ValoresDiferentes_NoSonIguales()
    {
        var a = new NombreEscena("Despertar");
        var b = new NombreEscena("Buenas Noches");
        Assert.NotEqual(a, b);
    }
}

public class DireccionFisicaTests
{
    [Fact]
    public void Constructor_DatosValidos_CreaInstancia()
    {
        var dir = new DireccionFisica("Calle 10", "45", "Medellín", "Colombia", "050001");
        Assert.Equal("Medellín", dir.Ciudad);
        Assert.Equal("Colombia", dir.Pais);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_CiudadVacia_LanzaExcepcion(string? ciudad)
    {
        Assert.Throws<DomainException>(() =>
            new DireccionFisica("Calle 10", "45", ciudad!, "Colombia", "050001"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Constructor_PaisVacio_LanzaExcepcion(string? pais)
    {
        Assert.Throws<DomainException>(() =>
            new DireccionFisica("Calle 10", "45", "Medellín", pais!, "050001"));
    }
}

public class VersionFirmwareTests
{
    [Fact]
    public void Constructor_VersionValida_CreaInstancia()
    {
        var version = new VersionFirmware(2, 1, 3);
        Assert.Equal("2.1.3", version.ToString());
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(0, -1, 0)]
    [InlineData(0, 0, -1)]
    public void Constructor_ComponenteNegativo_LanzaExcepcion(int major, int minor, int patch)
    {
        Assert.Throws<DomainException>(() => new VersionFirmware(major, minor, patch));
    }

    [Fact]
    public void Igualdad_MismaVersion_SonIguales()
    {
        var a = new VersionFirmware(1, 0, 0);
        var b = new VersionFirmware(1, 0, 0);
        Assert.Equal(a, b);
    }
}

public class IdentificadorFisicoTests
{
    [Fact]
    public void Constructor_ValorValido_CreaInstancia()
    {
        var id = new IdentificadorFisico("AA:BB:CC:DD:EE:FF", "MAC");
        Assert.Equal("AA:BB:CC:DD:EE:FF", id.Valor);
        Assert.Equal("MAC", id.TipoIdentificador);
    }

    [Fact]
    public void Constructor_ValorVacio_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => new IdentificadorFisico("", "MAC"));
    }

    [Fact]
    public void Constructor_TipoVacio_LanzaExcepcion()
    {
        Assert.Throws<DomainException>(() => new IdentificadorFisico("AA:BB:CC:DD:EE:FF", ""));
    }
}
