using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record ComandoEnviado(Guid EventId, DateTime OccurredOn, int ComandoId, int DispositivoId, string Comando) : IDomainEvent
{
    public ComandoEnviado(int comandoId, int dispositivoId, string comando)
        : this(Guid.NewGuid(), DateTime.UtcNow, comandoId, dispositivoId, comando) { }
}
