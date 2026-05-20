using IoT.Application.Commands;
using IoT.Application.Handlers;
using IoT.Application.Interfaces;
using IoT.Application.Validators;
using Microsoft.AspNetCore.Mvc;

namespace IoT.API.Controllers;

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
