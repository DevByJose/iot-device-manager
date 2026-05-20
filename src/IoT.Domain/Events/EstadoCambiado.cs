using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record EstadoCambiado(Guid EventId, DateTime OccurredOn, int DispositivoId, string EstadoAnterior, string EstadoNuevo) : IDomainEvent
{
    public EstadoCambiado(int dispositivoId, string estadoAnterior, string estadoNuevo)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, estadoAnterior, estadoNuevo) { }
}
