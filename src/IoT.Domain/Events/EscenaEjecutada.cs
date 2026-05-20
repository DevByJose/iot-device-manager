using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record EscenaEjecutada(Guid EventId, DateTime OccurredOn, int EscenaId, string Origen, int TotalAcciones) : IDomainEvent
{
    public EscenaEjecutada(int escenaId, string origen, int totalAcciones)
        : this(Guid.NewGuid(), DateTime.UtcNow, escenaId, origen, totalAcciones) { }
}
