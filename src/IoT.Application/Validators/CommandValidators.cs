using IoT.Application.Commands;
using IoT.Domain.Exceptions;

namespace IoT.Application.Validators;

/// <summary>
/// Validación de forma de datos de entrada. NO valida reglas de negocio (SRP).
/// </summary>
public static class CommandValidators
{
    public static void Validate(RegistrarDispositivoCommand command)
    {
        if (command.HogarId <= 0)
            throw new DomainException("El ID del hogar debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(command.Nombre))
            throw new DomainException("El nombre del dispositivo es obligatorio.");
        if (string.IsNullOrWhiteSpace(command.TipoDispositivo))
            throw new DomainException("El tipo de dispositivo es obligatorio.");
        if (string.IsNullOrWhiteSpace(command.IdentificadorFisico))
            throw new DomainException("El identificador físico es obligatorio.");
        if (command.HabitacionId <= 0)
            throw new DomainException("El ID de la habitación debe ser mayor a 0.");
    }

    public static void Validate(CrearEscenaCommand command)
    {
        if (command.HogarId <= 0)
            throw new DomainException("El ID del hogar debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(command.Nombre))
            throw new DomainException("El nombre de la escena es obligatorio.");
        if (command.Acciones == null || command.Acciones.Count == 0)
            throw new DomainException("La escena debe tener al menos una acción.");
    }

    public static void Validate(RegistrarHogarCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Nombre))
            throw new DomainException("El nombre del hogar es obligatorio.");
        if (string.IsNullOrWhiteSpace(command.Ciudad))
            throw new DomainException("La ciudad es obligatoria.");
        if (string.IsNullOrWhiteSpace(command.Pais))
            throw new DomainException("El país es obligatorio.");
        if (command.ClienteId <= 0)
            throw new DomainException("El ID del cliente debe ser mayor a 0.");
    }

    public static void Validate(EjecutarEscenaCommand command)
    {
        if (command.EscenaId <= 0)
            throw new DomainException("El ID de la escena debe ser mayor a 0.");
        if (string.IsNullOrWhiteSpace(command.Origen))
            throw new DomainException("El origen de la ejecución es obligatorio.");
    }
}
