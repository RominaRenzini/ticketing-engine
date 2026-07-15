using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationPublisher _reservationPublisher;

    public ReservationService(IReservationPublisher reservationPublisher)
    {
        _reservationPublisher = reservationPublisher;
    }

    public async Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default)
    {
        var seat = new Seat(row, number, 100m);
        var concertEvent = new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow, new[] { seat });
        var lockedUntilUtc = concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5));

        await _reservationPublisher.PublishAsync(
            new SeatLockedIntegrationEvent(eventId, seat.Id, lockedUntilUtc),
            cancellationToken);

        return seat;
    }
}
