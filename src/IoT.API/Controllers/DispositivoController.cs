using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Queries;
using IoT.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

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
