using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record DispositivoDesinstalado(Guid EventId, DateTime OccurredOn, int DispositivoId, int HogarId) : IDomainEvent
{
    public DispositivoDesinstalado(int dispositivoId, int hogarId)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, hogarId) { }
}
