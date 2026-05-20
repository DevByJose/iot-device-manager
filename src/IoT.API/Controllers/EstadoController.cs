using IoT.Application.Handlers;
using IoT.Application.Queries;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

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
