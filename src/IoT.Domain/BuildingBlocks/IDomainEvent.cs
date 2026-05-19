namespace IoT.Domain.BuildingBlocks;

/// <summary>
/// Interfaz marcadora para eventos de dominio (ISP - Interface Segregation).
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
