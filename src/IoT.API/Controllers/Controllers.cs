using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Queries;
using IoT.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

/// <summary>
/// Controller delgado para Hogares. Solo despacha a handlers (SRP).
/// No contiene lógica de negocio ni accede a repositorios directamente.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HogarController : ControllerBase
{
    private readonly RegistrarHogarHandler _registrarHandler;
    private readonly ObtenerHogaresHandler _obtenerHandler;

    public HogarController(RegistrarHogarHandler registrarHandler, ObtenerHogaresHandler obtenerHandler)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarHogarCommand command)
    {
        var result = await _registrarHandler.HandleAsync(command);
        return CreatedAtAction(nameof(Registrar), new { id = result.Id }, result);
    }

    [HttpGet("cliente/{clienteId}")]
    public async Task<IActionResult> ObtenerPorCliente(int clienteId)
    {
        var result = await _obtenerHandler.HandleAsync(new ObtenerHogaresQuery(clienteId));
        return Ok(result);
    }
}

/// <summary>
/// Controller delgado para Dispositivos (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DispositivoController : ControllerBase
{
    private readonly RegistrarDispositivoHandler _registrarHandler;
    private readonly ObtenerDispositivosHandler _obtenerHandler;

    public DispositivoController(RegistrarDispositivoHandler registrarHandler, ObtenerDispositivosHandler obtenerHandler)
    {
        _registrarHandler = registrarHandler;
        _obtenerHandler = obtenerHandler;
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarDispositivoCommand command)
    {
        var result = await _registrarHandler.HandleAsync(command);
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
/// Controller delgado para Escenas (SRP).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EscenaController : ControllerBase
{
    private readonly EjecutarEscenaHandler _ejecutarHandler;

    public EscenaController(EjecutarEscenaHandler ejecutarHandler)
    {
        _ejecutarHandler = ejecutarHandler;
    }

    [HttpPost("{escenaId}/ejecutar")]
    public async Task<IActionResult> Ejecutar(int escenaId, [FromQuery] string origen = "manual")
    {
        var result = await _ejecutarHandler.HandleAsync(new EjecutarEscenaCommand(escenaId, origen));
        return Ok(result);
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
