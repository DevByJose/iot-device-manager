using IoT.Domain.BuildingBlocks;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// AGGREGATE ROOT: Hogar — Raíz del agregado de Inventario de Dispositivos.
/// Único punto de acceso a Habitación y Dispositivo.
/// </summary>
public class Hogar : AggregateRoot
{
    public string Nombre { get; private set; }
    public DireccionFisica Direccion { get; private set; }
    public GeoLocalizacion? Ubicacion { get; private set; }
    public int ClienteId { get; private set; }

    private readonly List<Habitacion> _habitaciones = new();
    public IReadOnlyCollection<Habitacion> Habitaciones => _habitaciones.AsReadOnly();

    private readonly List<Dispositivo> _dispositivos = new();
    public IReadOnlyCollection<Dispositivo> Dispositivos => _dispositivos.AsReadOnly();

    private Hogar()
    {
        Nombre = string.Empty;
        Direccion = null!;
    }

    public Hogar(int id, string nombre, DireccionFisica direccion, int clienteId, GeoLocalizacion? ubicacion = null)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new DomainException("El nombre del hogar no puede estar vacío.");

        Id = id;
        Nombre = nombre;
        Direccion = direccion ?? throw new DomainException("La dirección es obligatoria.");
        ClienteId = clienteId;
        Ubicacion = ubicacion;

        AddDomainEvent(new HogarRegistrado(id, direccion.Ciudad, direccion.Pais));
    }

    public Habitacion AgregarHabitacion(int habitacionId, string nombre)
    {
        if (_habitaciones.Any(h => h.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException($"Ya existe una habitación con el nombre '{nombre}' en este hogar.");

        var habitacion = new Habitacion(habitacionId, nombre, Id);
        _habitaciones.Add(habitacion);
        return habitacion;
    }

    public Dispositivo RegistrarDispositivo(int dispositivoId, string nombre, string tipoDispositivo,
        IdentificadorFisico identificador, VersionFirmware firmware, int habitacionId)
    {
        // Validar que la habitación pertenezca a este hogar (invariante del agregado)
        if (!_habitaciones.Any(h => h.Id == habitacionId))
            throw new DomainException("La habitación especificada no pertenece a este hogar.");

        var dispositivo = new Dispositivo(dispositivoId, nombre, tipoDispositivo,
            identificador, firmware, habitacionId, Id);

        _dispositivos.Add(dispositivo);
        AddDomainEvent(new DispositivoRegistrado(dispositivoId, Id, tipoDispositivo));
        return dispositivo;
    }

    public void DesinstalarDispositivo(int dispositivoId)
    {
        var dispositivo = _dispositivos.FirstOrDefault(d => d.Id == dispositivoId)
            ?? throw new DomainException($"El dispositivo {dispositivoId} no existe en este hogar.");

        _dispositivos.Remove(dispositivo);
        AddDomainEvent(new DispositivoDesinstalado(dispositivoId, Id));
    }

    public void MoverDispositivo(int dispositivoId, int habitacionDestinoId)
    {
        var dispositivo = _dispositivos.FirstOrDefault(d => d.Id == dispositivoId)
            ?? throw new DomainException($"El dispositivo {dispositivoId} no existe en este hogar.");

        if (!_habitaciones.Any(h => h.Id == habitacionDestinoId))
            throw new DomainException("La habitación destino no pertenece a este hogar.");

        dispositivo.MoverAHabitacion(habitacionDestinoId);
    }

    public void ConectarDispositivo(int dispositivoId)
    {
        var dispositivo = _dispositivos.FirstOrDefault(d => d.Id == dispositivoId)
            ?? throw new DomainException($"El dispositivo {dispositivoId} no existe en este hogar.");
        dispositivo.Conectar();
    }

    public void DesconectarDispositivo(int dispositivoId)
    {
        var dispositivo = _dispositivos.FirstOrDefault(d => d.Id == dispositivoId)
            ?? throw new DomainException($"El dispositivo {dispositivoId} no existe en este hogar.");
        dispositivo.Desconectar();
    }

    public void EstablecerUbicacion(GeoLocalizacion ubicacion)
    {
        Ubicacion = ubicacion ?? throw new DomainException("La ubicación no puede ser nula.");
    }
}
