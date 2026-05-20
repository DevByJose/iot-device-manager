using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record ComandoConfirmado(Guid EventId, DateTime OccurredOn, int ComandoId) : IDomainEvent
{
    public ComandoConfirmado(int comandoId)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId) { }
}
