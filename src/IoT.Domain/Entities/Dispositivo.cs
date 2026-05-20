using IoT.Domain.BuildingBlocks;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Entidad Dispositivo con lógica de negocio enriquecida.
/// </summary>
public class Dispositivo : Entity
{
    public string Nombre { get; private set; }
    public string TipoDispositivo { get; private set; }
    public IdentificadorFisico Identificador { get; private set; }
    public VersionFirmware Firmware { get; private set; }
    public int HabitacionId { get; private set; }
    public int HogarId { get; private set; }
    public string Estado { get; private set; }
    public DateTime UltimoContacto { get; private set; }
    public bool EstaConectado { get; private set; }

    private Dispositivo()
    {
        Nombre = string.Empty;
        TipoDispositivo = string.Empty;
        Identificador = null!;
        Firmware = null!;
        Estado = "Offline";
    }

    public Dispositivo(int id, string nombre, string tipoDispositivo,
        IdentificadorFisico identificador, VersionFirmware firmware,
        int habitacionId, int hogarId)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del dispositivo no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(tipoDispositivo))
            throw new DomainException("El tipo de dispositivo no puede estar vacío.");

        Id = id;
        Nombre = nombre;
        TipoDispositivo = tipoDispositivo;
        Identificador = identificador;
        Firmware = firmware;
        HabitacionId = habitacionId;
        HogarId = hogarId;
        Estado = "Offline";
        EstaConectado = false;
        UltimoContacto = DateTime.UtcNow;
    }

    public void Conectar()
    {
        Estado = "Online";
        EstaConectado = true;
        UltimoContacto = DateTime.UtcNow;
    }

    public void Desconectar()
    {
        Estado = "Offline";
        EstaConectado = false;
    }

    public void MarcarError(string motivo)
    {
        Estado = $"Error: {motivo}";
        EstaConectado = false;
    }

    public void MoverAHabitacion(int nuevaHabitacionId)
    {
        if (nuevaHabitacionId <= 0)
            throw new DomainException("La habitación destino no es válida.");
        HabitacionId = nuevaHabitacionId;
    }

    public void ActualizarFirmware(VersionFirmware nuevaVersion)
    {
        Firmware = nuevaVersion ?? throw new DomainException("La versión de firmware no puede ser nula.");
    }

    public bool PuedeEjecutarComando()
    {
        return EstaConectado && Estado == "Online";
    }
}
