using IoT.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace IoT.API.Middleware;

/// <summary>
/// Middleware para manejo global de excepciones. Intercepta DomainException → 400,
/// Exception genérica → 500 (SRP).
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Error de dominio: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";
            var response = JsonSerializer.Serialize(new { error = ex.Message, type = "DomainException" });
            await context.Response.WriteAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error interno del servidor: {Message}", ex.Message);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";
            var response = JsonSerializer.Serialize(new { error = "Error interno del servidor.", type = "InternalError" });
            await context.Response.WriteAsync(response);
        }
    }
}
