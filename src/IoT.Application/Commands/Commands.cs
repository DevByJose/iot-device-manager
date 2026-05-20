namespace IoT.Application.Commands;

/// <summary>
/// Intención de registrar un nuevo hogar. Inmutable (SRP).
/// </summary>
public sealed record RegistrarHogarCommand(
    string Nombre, string Calle, string Numero, string Ciudad,
    string Pais, string CodigoPostal, int ClienteId);

/// <summary>
/// Intención de registrar un nuevo dispositivo en un hogar. Inmutable.
/// </summary>
public sealed record RegistrarDispositivoCommand(
    int HogarId, string Nombre, string TipoDispositivo,
    string IdentificadorFisico, string TipoIdentificador,
    int FirmwareMajor, int FirmwareMinor, int FirmwarePatch,
    int HabitacionId);

/// <summary>
/// Intención de crear una nueva escena. Inmutable.
/// </summary>
public sealed record CrearEscenaCommand(
    int HogarId, string Nombre, List<AccionEscenaInput> Acciones);

public sealed record AccionEscenaInput(
    int Orden, int DispositivoId, string Comando, string? ParametroNombre = null, string? ParametroValor = null);

/// <summary>
/// Intención de ejecutar una escena existente. Inmutable.
/// </summary>
public sealed record EjecutarEscenaCommand(int EscenaId, string Origen);

/// <summary>
/// Intención de enviar un comando a un dispositivo. Inmutable.
/// </summary>
public sealed record EnviarComandoCommand(int DispositivoId, string Comando, Dictionary<string, string>? Parametros = null);

/// <summary>
/// Intención de agregar una habitación a un hogar existente. Inmutable.
/// </summary>
public sealed record AgregarHabitacionCommand(int HogarId, string Nombre);
