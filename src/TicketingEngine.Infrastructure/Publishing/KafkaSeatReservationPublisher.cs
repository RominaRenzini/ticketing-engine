using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;

namespace TicketingEngine.Infrastructure.Publishing;

public sealed class KafkaSeatReservationPublisher : IReservationPublisher
{
    public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"Publishing seat lock for event {integrationEvent.EventId} seat {integrationEvent.SeatId}.");
        return Task.CompletedTask;
    }
}
