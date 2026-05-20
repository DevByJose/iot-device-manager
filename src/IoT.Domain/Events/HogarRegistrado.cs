using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Events;

public sealed record HogarRegistrado(Guid EventId, DateTime OccurredOn, int HogarId, string Ciudad, string Pais) : IDomainEvent
{
    public HogarRegistrado(int hogarId, string ciudad, string pais)
        : this(Guid.NewGuid(), DateTime.UtcNow, hogarId, ciudad, pais) { }
}
