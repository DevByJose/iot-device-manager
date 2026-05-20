namespace IoT.Application.DTOs;

/// <summary>
/// DTO de respuesta al enviar un comando a un dispositivo.
/// </summary>
public sealed record ComandoDispositivoDto(int Id, int DispositivoId, string Comando, string Estado, DateTime CreadoEn);
