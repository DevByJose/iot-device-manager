using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record DispositivoRegistrado(Guid EventId, DateTime OccurredOn, int DispositivoId, int HogarId, string TipoDispositivo) : IDomainEvent
{
    public DispositivoRegistrado(int dispositivoId, int hogarId, string tipoDispositivo)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, hogarId, tipoDispositivo) { }
}
