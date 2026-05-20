namespace IoT.Application.Commands;

public sealed record AccionEscenaInput(
    int Orden, int DispositivoId, string Comando, string? ParametroNombre = null, string? ParametroValor = null);
