using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record HogarRegistrado(Guid EventId, DateTime OccurredOn, int HogarId, string Ciudad, string Pais) : IDomainEvent
{
    public HogarRegistrado(int hogarId, string ciudad, string pais)
        : this(Guid.NewGuid(), DateTime.UtcNow, hogarId, ciudad, pais) { }
}

public sealed record DispositivoRegistrado(Guid EventId, DateTime OccurredOn, int DispositivoId, int HogarId, string TipoDispositivo) : IDomainEvent
{
    public DispositivoRegistrado(int dispositivoId, int hogarId, string tipoDispositivo)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, hogarId, tipoDispositivo) { }
}

public sealed record DispositivoDesinstalado(Guid EventId, DateTime OccurredOn, int DispositivoId, int HogarId) : IDomainEvent
{
    public DispositivoDesinstalado(int dispositivoId, int hogarId)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, hogarId) { }
}

public sealed record EscenaCreada(Guid EventId, DateTime OccurredOn, int EscenaId, string Nombre) : IDomainEvent
{
    public EscenaCreada(int escenaId, string nombre)
        : this(Guid.NewGuid(), DateTime.UtcNow, escenaId, nombre) { }
}

public sealed record EscenaEjecutada(Guid EventId, DateTime OccurredOn, int EscenaId, string Origen, int TotalAcciones) : IDomainEvent
{
    public EscenaEjecutada(int escenaId, string origen, int totalAcciones)
        : this(Guid.NewGuid(), DateTime.UtcNow, escenaId, origen, totalAcciones) { }
}

public sealed record ComandoEnviado(Guid EventId, DateTime OccurredOn, int ComandoId, int DispositivoId, string Comando) : IDomainEvent
{
    public ComandoEnviado(int comandoId, int dispositivoId, string comando)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId, dispositivoId, comando) { }
}

public sealed record ComandoConfirmado(Guid EventId, DateTime OccurredOn, int ComandoId) : IDomainEvent
{
    public ComandoConfirmado(int comandoId)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId) { }
}

public sealed record ComandoFallido(Guid EventId, DateTime OccurredOn, int ComandoId, string Motivo) : IDomainEvent
{
    public ComandoFallido(int comandoId, string motivo)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId, motivo) { }
}

public sealed record EstadoCambiado(Guid EventId, DateTime OccurredOn, int DispositivoId, string EstadoAnterior, string EstadoNuevo) : IDomainEvent
{
    public EstadoCambiado(int dispositivoId, string estadoAnterior, string estadoNuevo)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, estadoAnterior, estadoNuevo) { }
}

public sealed record AnomaliaDetectada(Guid EventId, DateTime OccurredOn, int DispositivoId, double ValorLeido, double UmbralMin, double UmbralMax) : IDomainEvent
{
    public AnomaliaDetectada(int dispositivoId, double valorLeido, double umbralMin, double umbralMax)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, valorLeido, umbralMin, umbralMax) { }
}

public sealed record DispositivoDesconectado(Guid EventId, DateTime OccurredOn, int DispositivoId, DateTime UltimoContacto) : IDomainEvent
{
    public DispositivoDesconectado(int dispositivoId, DateTime ultimoContacto)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, ultimoContacto) { }
}
