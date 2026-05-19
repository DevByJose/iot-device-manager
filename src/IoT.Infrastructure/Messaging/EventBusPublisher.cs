using IoT.Domain.BuildingBlocks;
using IoT.Domain.Interfaces;

namespace IoT.Infrastructure.Messaging;

/// <summary>
/// Implementa IEventPublisher. Simula un bus de eventos (RabbitMQ/Kafka).
/// En producción se reemplazaría por una implementación real (OCP).
/// </summary>
public class EventBusPublisher : IEventPublisher
{
    public Task PublishAsync(IDomainEvent domainEvent)
    {
        // En producción: serializar y enviar a RabbitMQ/Kafka
        Console.WriteLine($"[EVENT BUS] Evento publicado: {domainEvent.GetType().Name} | Id: {domainEvent.EventId} | {domainEvent.OccurredOn:O}");
        return Task.CompletedTask;
    }

    public async Task PublishAllAsync(IEnumerable<IDomainEvent> domainEvents)
    {
        foreach (var @event in domainEvents)
        {
            await PublishAsync(@event);
        }
    }
}
