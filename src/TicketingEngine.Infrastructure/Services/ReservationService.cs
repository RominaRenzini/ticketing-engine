using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private const int MaxConcurrencyRetries = 3;

    private readonly IReservationPublisher _reservationPublisher;
    private readonly IConcertEventRepository _concertEventRepository;

    public ReservationService(IReservationPublisher reservationPublisher, IConcertEventRepository concertEventRepository)
    {
        _reservationPublisher = reservationPublisher;
        _concertEventRepository = concertEventRepository;
    }

    public async Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await TryReserveAsync(eventId, row, number, cancellationToken);
            }
            catch (InvalidOperationException) when (attempt < MaxConcurrencyRetries)
            {
                // Another writer updated this event between our read and write (optimistic concurrency
                // conflict in MongoConcertEventRepository.UpdateAsync); retry with a fresh read.
            }
        }
    }

    private async Task<Seat> TryReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken)
    {
        var existingConcertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken);
        var isNewEvent = existingConcertEvent is null;
        var concertEvent = existingConcertEvent ?? new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow);

        var seat = concertEvent.Seats.SingleOrDefault(existingSeat =>
            string.Equals(existingSeat.Row, row, StringComparison.OrdinalIgnoreCase)
            && existingSeat.Number == number);

        if (seat is null)
        {
            seat = new Seat(row, number, 100m);
            concertEvent.AddSeat(seat);
        }

        var seatToLock = seat!;
        var lockedUntilUtc = concertEvent.LockSeat(seatToLock.Id, TimeSpan.FromMinutes(5));

        if (isNewEvent)
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
