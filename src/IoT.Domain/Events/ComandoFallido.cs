using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record ComandoFallido(Guid EventId, DateTime OccurredOn, int ComandoId, string Motivo) : IDomainEvent
{
    public ComandoFallido(int comandoId, string motivo)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId, motivo) { }
}
