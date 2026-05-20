using IoT.Domain.BuildingBlocks;

namespace IoT.Domain.Interfaces;

/// <summary>
/// Contrato para publicación de eventos de dominio (DIP).
/// </summary>
public interface IEventPublisher
{
    Task PublishAsync(IDomainEvent domainEvent);
    Task PublishAllAsync(IEnumerable<IDomainEvent> domainEvents);
}
