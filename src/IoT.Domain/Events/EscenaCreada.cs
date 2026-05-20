using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record EscenaCreada(Guid EventId, DateTime OccurredOn, int EscenaId, string Nombre) : IDomainEvent
{
    public EscenaCreada(int escenaId, string nombre)
        : this(Guid.NewGuid(), DateTime.UtcNow, escenaId, nombre) { }
}
