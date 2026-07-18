using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationPublisher _reservationPublisher;
    private readonly ReservationStateStore _stateStore;

    public ReservationService(IReservationPublisher reservationPublisher, ReservationStateStore? stateStore = null)
    {
        _reservationPublisher = reservationPublisher;
        _stateStore = stateStore ?? new ReservationStateStore();
    }

    public async Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default)
    {
        var seat = new Seat(row, number, 100m);
        var concertEvent = _stateStore.GetEvent(eventId) ?? new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow);

        concertEvent.AddSeat(seat);
        var lockedUntilUtc = concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5));
        _stateStore.Save(concertEvent);

        await _reservationPublisher.PublishAsync(
            new SeatLockedIntegrationEvent(eventId, seat.Id, lockedUntilUtc),
            cancellationToken);

        return seat;
    }
}
