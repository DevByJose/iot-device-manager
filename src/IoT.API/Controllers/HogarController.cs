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
