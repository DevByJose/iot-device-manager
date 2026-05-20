using IoT.Application.DTOs;
using IoT.Domain.Entities;

namespace IoT.Application.Mappings;

/// <summary>
/// Mapeo explícito entre entidades de dominio y DTOs (SRP). Sin dependencias de librerías externas.
/// </summary>
public static class DomainToDtoMapper
{
    public static HogarDto ToDto(Hogar hogar) => new(
        hogar.Id, hogar.Nombre, hogar.Direccion.Ciudad, hogar.Direccion.Pais,
        hogar.Dispositivos.Count, hogar.Habitaciones.Count);

    public static DispositivoDto ToDto(Dispositivo dispositivo, string nombreHabitacion = "") => new(
        dispositivo.Id, dispositivo.Nombre, dispositivo.TipoDispositivo,
        dispositivo.Identificador.Valor, dispositivo.Estado, dispositivo.EstaConectado,
        dispositivo.Firmware.ToString(), nombreHabitacion);

    public static EscenaDto ToDto(Escena escena) => new(
        escena.Id, escena.Nombre.Valor, escena.Activa,
        escena.Acciones.Count, escena.Disparadores.Count);

    public static EstadoDispositivoDto ToDto(EstadoDispositivo estado) => new(
        estado.DispositivoId, estado.EstadoActual, estado.UltimoValorReportado,
        estado.UltimaActualizacion, estado.Conectado, estado.Alertas.Count);

    public static LecturaSensorDto ToDto(LecturaSensor lectura) => new(
        lectura.Valor, lectura.Unidad, lectura.Timestamp);

    public static IReadOnlyList<DispositivoDto> ToDtoList(IEnumerable<Dispositivo> dispositivos)
        => dispositivos.Select(d => ToDto(d)).ToList().AsReadOnly();

    public static IReadOnlyList<DispositivoDto> ToDtoList(IEnumerable<Dispositivo> dispositivos, IEnumerable<Habitacion> habitaciones)
    {
        var map = habitaciones.ToDictionary(h => h.Id, h => h.Nombre);
        return dispositivos
            .Select(d => ToDto(d, map.TryGetValue(d.HabitacionId, out var nombre) ? nombre : string.Empty))
            .ToList()
            .AsReadOnly();
    }

    public static IReadOnlyList<HogarDto> ToDtoList(IEnumerable<Hogar> hogares)
        => hogares.Select(ToDto).ToList().AsReadOnly();

    public static HabitacionDto ToDto(Habitacion habitacion) => new(
        habitacion.Id, habitacion.Nombre, habitacion.HogarId);

    public static IReadOnlyList<HabitacionDto> ToDtoList(IEnumerable<Habitacion> habitaciones)
        => habitaciones.Select(ToDto).ToList().AsReadOnly();
}
