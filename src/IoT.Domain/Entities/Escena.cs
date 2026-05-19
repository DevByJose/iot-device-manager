using IoT.Domain.BuildingBlocks;
using IoT.Domain.Events;
using IoT.Domain.Exceptions;
using IoT.Domain.ValueObjects;

namespace IoT.Domain.Entities;

/// <summary>
/// Entidad hija: instrucción individual dentro de una Escena.
/// </summary>
public class AccionEscena : Entity
{
    public int Orden { get; private set; }
    public int DispositivoId { get; private set; }
    public string Comando { get; private set; }
    public ParametroComando? Parametro { get; private set; }

    private AccionEscena() { Comando = string.Empty; }

    public AccionEscena(int id, int orden, int dispositivoId, string comando, ParametroComando? parametro = null)
    {
        if (string.IsNullOrWhiteSpace(comando))
            throw new DomainException("El comando de la acción no puede estar vacío.");

        Id = id;
        Orden = orden;
        DispositivoId = dispositivoId;
        Comando = comando;
        Parametro = parametro;
    }
}

/// <summary>
/// Entidad hija: condición que activa una Escena automáticamente.
/// </summary>
public class Disparador : Entity
{
    public string Tipo { get; private set; } // Horario, GeoLocalizacion, Sensor
    public CondicionDisparador Condicion { get; private set; }
    public IntervaloHorario? Horario { get; private set; }

    private Disparador() { Tipo = string.Empty; Condicion = null!; }

    public Disparador(int id, string tipo, CondicionDisparador condicion, IntervaloHorario? horario = null)
    {
        if (string.IsNullOrWhiteSpace(tipo))
            throw new DomainException("El tipo de disparador no puede estar vacío.");

        Id = id;
        Tipo = tipo;
        Condicion = condicion ?? throw new DomainException("La condición del disparador es obligatoria.");
        Horario = horario;
    }

    public bool EsHorario() => Tipo.Equals("Horario", StringComparison.OrdinalIgnoreCase);
}

/// <summary>
/// AGGREGATE ROOT: Escena — Raíz del agregado de Control y Automatización.
/// Invariante: solo se accede a AcciónEscena y Disparador a través de Escena (la raíz).
/// </summary>
public class Escena : AggregateRoot
{
    public NombreEscena Nombre { get; private set; }
    public int HogarId { get; private set; }
    public bool Activa { get; private set; }

    private readonly List<AccionEscena> _acciones = new();
    public IReadOnlyCollection<AccionEscena> Acciones => _acciones.AsReadOnly();

    private readonly List<Disparador> _disparadores = new();
    public IReadOnlyCollection<Disparador> Disparadores => _disparadores.AsReadOnly();

    private Escena() { Nombre = null!; }

    public Escena(int id, NombreEscena nombre, int hogarId)
    {
        Id = id;
        Nombre = nombre ?? throw new DomainException("El nombre de la escena es obligatorio.");
        HogarId = hogarId;
        Activa = true;

        AddDomainEvent(new EscenaCreada(id, nombre.Valor));
    }

    public void AgregarAccion(int accionId, int orden, int dispositivoId, string comando, ParametroComando? parametro = null)
    {
        var accion = new AccionEscena(accionId, orden, dispositivoId, comando, parametro);
        _acciones.Add(accion);
    }

    public void AgregarDisparador(int disparadorId, string tipo, CondicionDisparador condicion, IntervaloHorario? horario = null)
    {
        var disparador = new Disparador(disparadorId, tipo, condicion, horario);
        _disparadores.Add(disparador);
    }

    /// <summary>
    /// Ejecuta la escena: valida que esté activa y tenga acciones, luego eleva evento.
    /// </summary>
    public IReadOnlyList<AccionEscena> Ejecutar(string origen)
    {
        if (!Activa)
            throw new DomainException("No se puede ejecutar una escena inactiva.");
        if (_acciones.Count == 0)
            throw new DomainException("La escena debe tener al menos una acción para ejecutarse.");

        var accionesOrdenadas = _acciones.OrderBy(a => a.Orden).ToList();
        AddDomainEvent(new EscenaEjecutada(Id, origen, accionesOrdenadas.Count));
        return accionesOrdenadas.AsReadOnly();
    }

    public void Activar() => Activa = true;

    public void Desactivar() => Activa = false;

    public void CambiarNombre(NombreEscena nuevoNombre)
    {
        Nombre = nuevoNombre ?? throw new DomainException("El nombre de la escena es obligatorio.");
    }
}
