using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Queries;
using IoT.Application.Validators;
using IoT.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

/// <summary>
/// Controller delgado para Hogares. Valida en el boundary y maneja consistencia (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HogarController : ControllerBase
{
    private readonly RegistrarHogarHandler _registrarHandler;
    private readonly ObtenerHogaresHandler _obtenerHandler;
    private readonly AgregarHabitacionHandler _agregarHabitacionHandler;
    private readonly ObtenerHabitacionesHandler _obtenerHabitacionesHandler;
    private readonly IUnitOfWork _uow;

    public HogarController(
        RegistrarHogarHandler registrarHandler,
        ObtenerHogaresHandler obtenerHandler,
        AgregarHabitacionHandler agregarHabitacionHandler,
        ObtenerHabitacionesHandler obtenerHabitacionesHandler,
        IUnitOfWork uow)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
        _agregarHabitacionHandler = agregarHabitacionHandler;
        _obtenerHabitacionesHandler = obtenerHabitacionesHandler;
        _uow = uow;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarHogarCommand command)
    {
        CommandValidators.Validate(command);
        var result = await _registrarHandler.HandleAsync(command);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Registrar), new { id = result.Id }, result);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ObtenerPorCliente(int clienteId)
    {
        var result = await _obtenerHandler.HandleAsync(new ObtenerHogaresQuery(clienteId));
        return Ok(result);
    }

    [HttpPost("{hogarId}/habitacion")]
    public async Task<IActionResult> AgregarHabitacion(int hogarId, [FromBody] AgregarHabitacionCommand command)
    {
        var cmd = command with { HogarId = hogarId };
        CommandValidators.Validate(cmd);
        var result = await _agregarHabitacionHandler.HandleAsync(cmd);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(ObtenerHabitaciones), new { hogarId, id = result.Id }, result);
    }

    [HttpGet("{hogarId}/habitacion")]
    public async Task<IActionResult> ObtenerHabitaciones(int hogarId)
    {
        var result = await _obtenerHabitacionesHandler.HandleAsync(new ObtenerHabitacionesQuery(hogarId));
        return Ok(result);
    }
}

/// <summary>
/// Controller delgado para Dispositivos. Valida en el boundary y maneja consistencia (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DispositivoController : ControllerBase
{
    private readonly RegistrarDispositivoHandler _registrarHandler;
    private readonly ObtenerDispositivosHandler _obtenerHandler;
    private readonly IUnitOfWork _uow;

    public DispositivoController(RegistrarDispositivoHandler registrarHandler,
        ObtenerDispositivosHandler obtenerHandler, IUnitOfWork uow)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
        _uow = uow;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDispositivoCommand command)
    {
        CommandValidators.Validate(command);
        var result = await _registrarHandler.HandleAsync(command);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Registrar), new { id = result.DispositivoId }, result);
    }

    [HttpGet("hogar/{hogarId}")]
    public async Task<IActionResult> ObtenerPorHogar(int hogarId)
    {
        var result = await _obtenerHandler.HandleAsync(new ObtenerDispositivosQuery(hogarId));
        return Ok(result);
    }
}

/// <summary>
/// Controller delgado para Escenas. Maneja el ciclo transaccional completo (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EscenaController : ControllerBase
{
    private readonly CrearEscenaHandler _crearHandler;
    private readonly EjecutarEscenaHandler _ejecutarHandler;
    private readonly IUnitOfWork _uow;

    public EscenaController(CrearEscenaHandler crearHandler, EjecutarEscenaHandler ejecutarHandler, IUnitOfWork uow)
    {
        _crearHandler = crearHandler;
        _ejecutarHandler = ejecutarHandler;
        _uow = uow;
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] CrearEscenaCommand command)
    {
        CommandValidators.Validate(command);
        var result = await _crearHandler.HandleAsync(command);
        await _uow.SaveChangesAsync();
        return CreatedAtAction(nameof(Crear), new { id = result.Id }, result);
    }

    [HttpPost("{escenaId}/ejecutar")]
    public async Task<IActionResult> Ejecutar(int escenaId, [FromQuery] string origen = "manual")
    {
        var command = new EjecutarEscenaCommand(escenaId, origen);
        CommandValidators.Validate(command);
        await _uow.BeginTransactionAsync();
        try
        {
            var result = await _ejecutarHandler.HandleAsync(command);
            await _uow.CommitAsync();
            return Ok(result);
        }
        catch
        {
            await _uow.RollbackAsync();
            throw;
        }
    }
}

/// <summary>
/// Controller delgado para Estado de dispositivos (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EstadoController : ControllerBase
{
    private readonly ConsultarEstadoHandler _consultarHandler;

    public EstadoController(ConsultarEstadoHandler consultarHandler)
    {
        _consultarHandler = consultarHandler;
    }

    [HttpGet("{dispositivoId}")]
    public async Task<IActionResult> Consultar(int dispositivoId)
    {
        var result = await _consultarHandler.HandleAsync(new ConsultarEstadoQuery(dispositivoId));
        if (result == null) return NotFound();
        return Ok(result);
    }
}
