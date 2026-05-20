using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record AnomaliaDetectada(Guid EventId, DateTime OccurredOn, int DispositivoId, double ValorLeido, double UmbralMin, double UmbralMax) : IDomainEvent
{
    public AnomaliaDetectada(int dispositivoId, double valorLeido, double umbralMin, double umbralMax)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, valorLeido, umbralMin, umbralMax) { }
}
