using IoT.Application.Commands;
using IoT.Application.DTOs;
using IoT.Application.Mappings;
using IoT.Application.Queries;
using IoT.Application.Validators;
using IoT.Domain.Entities;
using IoT.Domain.Exceptions;
using IoT.Domain.Interfaces;
using IoT.Domain.Services;
using IoT.Domain.ValueObjects;

namespace IoT.Application.Handlers;

/// <summary>
/// Orquesta el registro de un hogar. No contiene lógica de negocio (SRP + OCP).
/// </summary>
public class RegistrarHogarHandler
{
    private readonly IHogarRepository _hogarRepo;
    private readonly IUnitOfWork _uow;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarHogarHandler(IHogarRepository hogarRepo, IUnitOfWork uow, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<HogarDto> HandleAsync(RegistrarHogarCommand command)
    {
        CommandValidators.Validate(command);

        var direccion = new DireccionFisica(command.Calle, command.Numero, command.Ciudad, command.Pais, command.CodigoPostal);
        var hogar = new Hogar(0, command.Nombre, direccion, command.ClienteId);

        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
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
    private readonly IUnitOfWork _uow;
    private readonly IEventPublisher _eventPublisher;

    public RegistrarDispositivoHandler(IHogarRepository hogarRepo, SvcRegistroDispositivo svcRegistro,
        IUnitOfWork uow, IEventPublisher eventPublisher)
    {
        _hogarRepo = hogarRepo;
        _svcRegistro = svcRegistro;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<RegisterDeviceResponse> HandleAsync(RegistrarDispositivoCommand command)
    {
        CommandValidators.Validate(command);

        var hogar = await _hogarRepo.GetByIdAsync(command.HogarId)
            ?? throw new DomainException($"Hogar {command.HogarId} no encontrado.");

        var identificador = new IdentificadorFisico(command.IdentificadorFisico, command.TipoIdentificador);
        var firmware = new VersionFirmware(command.FirmwareMajor, command.FirmwareMinor, command.FirmwarePatch);

        var dispositivo = await _svcRegistro.RegistrarAsync(hogar, 0, command.Nombre,
            command.TipoDispositivo, identificador, firmware, command.HabitacionId);

        await _hogarRepo.SaveAsync(hogar);
        await _uow.SaveChangesAsync();
        await _eventPublisher.PublishAllAsync(hogar.DomainEvents);
        hogar.ClearDomainEvents();

        return new RegisterDeviceResponse(dispositivo.Id, dispositivo.Nombre, dispositivo.TipoDispositivo, true, "Dispositivo registrado exitosamente.");
    }
}

/// <summary>
/// Orquesta la ejecución de una escena. Maneja transacción completa (SRP).
/// </summary>
public class EjecutarEscenaHandler
{
    private readonly IEscenaRepository _escenaRepo;
    private readonly SvcEjecucionEscena _svcEjecucion;
    private readonly IUnitOfWork _uow;
    private readonly IEventPublisher _eventPublisher;

    public EjecutarEscenaHandler(IEscenaRepository escenaRepo, SvcEjecucionEscena svcEjecucion,
        IUnitOfWork uow, IEventPublisher eventPublisher)
    {
        _escenaRepo = escenaRepo;
        _svcEjecucion = svcEjecucion;
        _uow = uow;
        _eventPublisher = eventPublisher;
    }

    public async Task<EjecutarEscenaResponse> HandleAsync(EjecutarEscenaCommand command)
    {
        CommandValidators.Validate(command);

        await _uow.BeginTransactionAsync();
        try
        {
            var escena = await _escenaRepo.GetByIdAsync(command.EscenaId)
                ?? throw new DomainException($"Escena {command.EscenaId} no encontrada.");

            var comandos = await _svcEjecucion.EjecutarAsync(escena, command.Origen);

            await _escenaRepo.SaveAsync(escena);
            await _uow.CommitAsync();
            await _eventPublisher.PublishAllAsync(escena.DomainEvents);
            escena.ClearDomainEvents();

            var enviados = comandos.Count(c => c.Estado == "Enviado");
            var fallidos = comandos.Count(c => c.Estado == "Fallido");
            var detalles = comandos.Select(c => $"[{c.Estado}] Dispositivo {c.DispositivoId}: {c.Comando}").ToList();

            return new EjecutarEscenaResponse(escena.Id, enviados, fallidos, detalles);
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
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
        return DomainToDtoMapper.ToDtoList(hogar.Dispositivos);
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
