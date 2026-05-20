using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record DispositivoDesconectado(Guid EventId, DateTime OccurredOn, int DispositivoId, DateTime UltimoContacto) : IDomainEvent
{
    public DispositivoDesconectado(int dispositivoId, DateTime ultimoContacto)
        : this(Guid.NewGuid(), DateTime.UtcNow, dispositivoId, ultimoContacto) { }
}
