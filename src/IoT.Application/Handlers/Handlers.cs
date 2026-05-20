using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta el registro de un hogar. No valida ni persiste (SRP).
/// </summary>
public class RegistrarHogarHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarHogarHandler(IHogarRepository hogarRepo, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _eventPublisher = eventPublisher;
    }

    public async Task<HogarDto> HandleAsync(RegistrarHogarCommand command)
    {
        var direccion = new DireccionFisica(command.Calle, command.Numero, command.Ciudad, command.Pais, command.CodigoPostal);
        var hogar = new Hogar(0, command.Nombre, direccion, command.ClienteId);

        await _hogarRepo.SaveAsync(hogar);
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(hogar);
    }
}

/// <summary>
/// Orquesta el registro de un dispositivo. Delega al servicio de dominio (SRP).
/// </summary>
public class RegistrarDispositivoHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly SvcRegistroDispositivo _svcRegistro;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarDispositivoHandler(IHogarRepository hogarRepo, SvcRegistroDispositivo svcRegistro,
        IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _svcRegistro = svcRegistro;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterDeviceResponse> HandleAsync(RegistrarDispositivoCommand command)
    {
        var hogar = await _hogarRepo.GetByIdAsync(command.HogarId)
            ?? throw new DomainException($"Hogar {command.HogarId} no encontrado.");

        var identificador = new IdentificadorFisico(command.IdentificadorFisico, command.TipoIdentificador);
        var firmware = new VersionFirmware(command.FirmwareMajor, command.FirmwareMinor, command.FirmwarePatch);

        var dispositivo = await _svcRegistro.RegistrarAsync(hogar, 0, command.Nombre,
            command.TipoDispositivo, identificador, firmware, command.HabitacionId);

        await _hogarRepo.SaveAsync(hogar);
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return new RegisterDeviceResponse(dispositivo.Id, dispositivo.Nombre, dispositivo.TipoDispositivo, true, "Dispositivo registrado exitosamente.");
    }
}

/// <summary>
/// Orquesta la creación de una nueva escena con sus acciones (SRP).
/// </summary>
public class CrearEscenaHandler
{
    private readonly IEscenaRepository _escenaRepo;
    private readonly IEventPublisher _eventPublisher;

    public CrearEscenaHandler(IEscenaRepository escenaRepo, IEventPublisher eventPublisher)
    {
        _escenaRepo = escenaRepo;
        _eventPublisher = eventPublisher;
    }

    public async Task<EscenaDto> HandleAsync(CrearEscenaCommand command)
    {
        var nombre = new NombreEscena(command.Nombre);
        var escena = new Escena(0, nombre, command.HogarId);

        foreach (var a in command.Acciones)
        {
            var parametro = a.ParametroNombre != null
                ? new ParametroComando(a.ParametroNombre, a.ParametroValor ?? string.Empty, "string")
                : null;
            escena.AgregarAccion(0, a.Orden, a.DispositivoId, a.Comando, parametro);
        }

        await _escenaRepo.SaveAsync(escena);
        await _eventPublisher.PublishAllAsync(escena.DomainEvents);
        escena.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(escena);
    }
}

/// <summary>
/// Orquesta la ejecución de una escena. El controller es responsable de la transacción (SRP).
/// </summary>
public class EjecutarEscenaHandler
{
    private readonly IEscenaRepository _escenaRepo;
    private readonly IComandoRepository _comandoRepo;
    private readonly SvcEjecucionEscena _svcEjecucion;
    private readonly IEventPublisher _eventPublisher;

    public EjecutarEscenaHandler(IEscenaRepository escenaRepo, IComandoRepository comandoRepo,
        SvcEjecucionEscena svcEjecucion, IEventPublisher eventPublisher)
    {
        _escenaRepo = escenaRepo;
        _comandoRepo = comandoRepo;
        _svcEjecucion = svcEjecucion;
        _eventPublisher = eventPublisher;
    }

    public async Task<EjecutarEscenaResponse> HandleAsync(EjecutarEscenaCommand command)
    {
        var escena = await _escenaRepo.GetByIdAsync(command.EscenaId)
            ?? throw new DomainException($"Escena {command.EscenaId} no encontrada.");

        var comandos = await _svcEjecucion.EjecutarAsync(escena, command.Origen);

        await _escenaRepo.SaveAsync(escena);
        await _comandoRepo.SaveAllAsync(comandos);

        await _eventPublisher.PublishAllAsync(escena.DomainEvents);
        escena.ClearDomainEvents();

        foreach (var cmd in comandos)
        {
            await _eventPublisher.PublishAllAsync(cmd.DomainEvents);
            cmd.ClearDomainEvents();
        }

        var enviados = comandos.Count(c => c.Estado == "Enviado");
        var fallidos = comandos.Count(c => c.Estado == "Fallido");
        var detalles = comandos.Select(c => $"[{c.Estado}] Dispositivo {c.DispositivoId}: {c.Comando}").ToList();

        return new EjecutarEscenaResponse(escena.Id, enviados, fallidos, detalles);
    }
}

/// <summary>
/// Maneja la consulta de estado de un dispositivo. Lee de caché si está disponible (SRP).
/// </summary>
public class ConsultarEstadoHandler
{
    private readonly IEstadoRepository _estadoRepo;
    private readonly ICacheService _cache;

    public ConsultarEstadoHandler(IEstadoRepository estadoRepo, ICacheService cache)
    {
        _estadoRepo = estadoRepo;
        _cache = cache;
    }

    public async Task<EstadoDispositivoDto?> HandleAsync(ConsultarEstadoQuery query)
    {
        var cacheKey = $"estado:dispositivo:{query.DispositivoId}";
        var cached = await _cache.GetAsync<EstadoDispositivoDto>(cacheKey);
        if (cached != null) return cached;

        var estado = await _estadoRepo.GetByDispositivoIdAsync(query.DispositivoId);
        if (estado == null) return null;

        var dto = DomainToDtoMapper.ToDto(estado);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(30));
        return dto;
    }
}

/// <summary>
/// Maneja la consulta de dispositivos de un hogar (SRP).
/// </summary>
public class ObtenerDispositivosHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerDispositivosHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<DispositivoDto>> HandleAsync(ObtenerDispositivosQuery query)
    {
        var hogar = await _hogarRepo.GetByIdAsync(query.HogarId);
        if (hogar == null) return new List<DispositivoDto>().AsReadOnly();
        return DomainToDtoMapper.ToDtoList(hogar.Dispositivos, hogar.Habitaciones);
    }
}

/// <summary>
/// Maneja la consulta de hogares de un cliente (SRP).
/// </summary>
public class ObtenerHogaresHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerHogaresHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<HogarDto>> HandleAsync(ObtenerHogaresQuery query)
    {
        var hogares = await _hogarRepo.GetByClienteIdAsync(query.ClienteId);
        return DomainToDtoMapper.ToDtoList(hogares);
    }
}

/// <summary>
/// Orquesta agregar una habitación a un hogar existente (DDD).
/// </summary>
public class AgregarHabitacionHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly IEventPublisher _eventPublisher;

    public AgregarHabitacionHandler(IHogarRepository hogarRepo, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _eventPublisher = eventPublisher;
    }

    public async Task<HabitacionDto> HandleAsync(AgregarHabitacionCommand command)
    {
        var hogar = await _hogarRepo.GetByIdAsync(command.HogarId)
            ?? throw new DomainException($"Hogar {command.HogarId} no encontrado.");

        var habitacion = hogar.AgregarHabitacion(0, command.Nombre);

        await _hogarRepo.SaveAsync(hogar);
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return DomainToDtoMapper.ToDto(habitacion);
    }
}

/// <summary>
/// Maneja la consulta de habitaciones de un hogar (SRP).
/// </summary>
public class ObtenerHabitacionesHandler
{
    private readonly IHogarRepository _hogarRepo;

    public ObtenerHabitacionesHandler(IHogarRepository hogarRepo)
    {
        _hogarRepo = hogarRepo;
    }

    public async Task<IReadOnlyList<HabitacionDto>> HandleAsync(ObtenerHabitacionesQuery query)
    {
        var hogar = await _hogarRepo.GetByIdAsync(query.HogarId);
        if (hogar == null) return new List<HabitacionDto>().AsReadOnly();
        return DomainToDtoMapper.ToDtoList(hogar.Habitaciones);
    }
}
