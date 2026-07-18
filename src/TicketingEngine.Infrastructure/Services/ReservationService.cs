using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationPublisher _reservationPublisher;
    private readonly IConcertEventRepository _concertEventRepository;

    public ReservationService(IReservationPublisher reservationPublisher, IConcertEventRepository concertEventRepository)
    {
        _reservationPublisher = reservationPublisher;
        _concertEventRepository = concertEventRepository;
    }

    public async Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default)
    {
        var concertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken)
            ?? new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow);

        var seat = concertEvent.Seats.SingleOrDefault(existingSeat =>
            string.Equals(existingSeat.Row, row, StringComparison.OrdinalIgnoreCase)
            && existingSeat.Number == number);

        var isNewSeat = seat is null;
        if (isNewSeat)
        {
            seat = new Seat(row, number, 100m);
            concertEvent.AddSeat(seat);
        }

        var seatToLock = seat!;
        var lockedUntilUtc = concertEvent.LockSeat(seatToLock.Id, TimeSpan.FromMinutes(5));

        if (isNewSeat)
        {
            await _concertEventRepository.SaveAsync(concertEvent, cancellationToken);
        }
        else
        {
            await _concertEventRepository.UpdateAsync(concertEvent, cancellationToken);
        }

        await _reservationPublisher.PublishAsync(
            new SeatLockedIntegrationEvent(eventId, seatToLock.Id, lockedUntilUtc),
            cancellationToken);

        return seatToLock;
    }
}
