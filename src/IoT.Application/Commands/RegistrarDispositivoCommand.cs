namespace IoT.Application.Commands;

/// <summary>
/// Intención de registrar un nuevo dispositivo en un hogar. Inmutable.
/// </summary>
public sealed record RegistrarDispositivoCommand(
    int HogarId, string Nombre, string TipoDispositivo,
    string IdentificadorFisico, string TipoIdentificador,
    int FirmwareMajor, int FirmwareMinor, int FirmwarePatch,
    int HabitacionId);
