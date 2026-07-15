using TicketingEngine.Application.Events;

namespace TicketingEngine.Application.Abstractions;

public interface IReservationPublisher
{
    Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
