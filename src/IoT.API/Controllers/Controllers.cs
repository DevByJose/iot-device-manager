using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Interfaces;
using IoT.Application.Queries;
using IoT.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

/// <summary>
/// Controller delgado para Hogares. Valida en el boundary (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HogarController : ControllerBase
{
    private readonly RegistrarHogarHandler _registrarHandler;
    private readonly ObtenerHogaresHandler _obtenerHandler;
    private readonly AgregarHabitacionHandler _agregarHabitacionHandler;
    private readonly ObtenerHabitacionesHandler _obtenerHabitacionesHandler;

    public HogarController(
        RegistrarHogarHandler registrarHandler,
        ObtenerHogaresHandler obtenerHandler,
        AgregarHabitacionHandler agregarHabitacionHandler,
        ObtenerHabitacionesHandler obtenerHabitacionesHandler)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
        _agregarHabitacionHandler = agregarHabitacionHandler;
        _obtenerHabitacionesHandler = obtenerHabitacionesHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarHogarCommand command)
    {
        CommandValidators.Validate(command);
        var result = await _registrarHandler.HandleAsync(command);
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
/// Controller delgado para Dispositivos. Valida en el boundary (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DispositivoController : ControllerBase
{
    private readonly RegistrarDispositivoHandler _registrarHandler;
    private readonly ObtenerDispositivosHandler _obtenerHandler;
    private readonly EnviarComandoHandler _enviarComandoHandler;
    private readonly ConectarDispositivoHandler _conectarHandler;
    private readonly DesconectarDispositivoHandler _desconectarHandler;

    public DispositivoController(
        RegistrarDispositivoHandler registrarHandler,
        ObtenerDispositivosHandler obtenerHandler,
        EnviarComandoHandler enviarComandoHandler,
        ConectarDispositivoHandler conectarHandler,
        DesconectarDispositivoHandler desconectarHandler)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
        _enviarComandoHandler = enviarComandoHandler;
        _conectarHandler = conectarHandler;
        _desconectarHandler = desconectarHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDispositivoCommand command)
    {
        CommandValidators.Validate(command);
        var result = await _registrarHandler.HandleAsync(command);
        return CreatedAtAction(nameof(Registrar), new { id = result.DispositivoId }, result);
    }

    [HttpGet("hogar/{hogarId}")]
    public async Task<IActionResult> ObtenerPorHogar(int hogarId)
    {
        var result = await _obtenerHandler.HandleAsync(new ObtenerDispositivosQuery(hogarId));
        return Ok(result);
    }

    [HttpPost("{id}/comando")]
    public async Task<IActionResult> EnviarComando(int id, [FromBody] EnviarComandoCommand command)
    {
        var cmd = command with { DispositivoId = id };
        CommandValidators.Validate(cmd);
        var result = await _enviarComandoHandler.HandleAsync(cmd);
        return Ok(result);
    }

    [HttpPut("{id}/conectar")]
    public async Task<IActionResult> Conectar(int id)
    {
        await _conectarHandler.HandleAsync(id);
        return NoContent();
    }

    [HttpPut("{id}/desconectar")]
    public async Task<IActionResult> Desconectar(int id)
    {
        await _desconectarHandler.HandleAsync(id);
        return NoContent();
    }
}

/// <summary>
/// Controller para Escenas. Maneja la transacción explícita en la ejecución (commits/rollbacks).
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
    private readonly ObtenerTelemetriaHandler _telemetriaHandler;

    public EstadoController(ConsultarEstadoHandler consultarHandler, ObtenerTelemetriaHandler telemetriaHandler)
    {
        _consultarHandler = consultarHandler;
        _telemetriaHandler = telemetriaHandler;
    }

    [HttpGet("{dispositivoId}")]
    public async Task<IActionResult> Consultar(int dispositivoId)
    {
        var result = await _consultarHandler.HandleAsync(new ConsultarEstadoQuery(dispositivoId));
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{dispositivoId}/telemetria")]
    public async Task<IActionResult> ObtenerTelemetria(int dispositivoId,
        [FromQuery] DateTime desde, [FromQuery] DateTime hasta)
    {
        var result = await _telemetriaHandler.HandleAsync(
            new ObtenerTelemetriaQuery(dispositivoId, desde, hasta));
        return Ok(result);
    }
}
