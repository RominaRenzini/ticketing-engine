using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Application.Models;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private const int MaxConcurrencyRetries = 3;

    private readonly IReservationPublisher _reservationPublisher;
    private readonly IConcertEventRepository _concertEventRepository;
    private readonly IIdempotencyStore<IReadOnlyList<Seat>> _idempotencyStore;

    public ReservationService(
        IReservationPublisher reservationPublisher,
        IConcertEventRepository concertEventRepository,
        IIdempotencyStore<IReadOnlyList<Seat>> idempotencyStore)
    {
        _reservationPublisher = reservationPublisher;
        _concertEventRepository = concertEventRepository;
        _idempotencyStore = idempotencyStore;
    }

    public async Task<IReadOnlyList<Seat>> ReserveSeatsAsync(
        Guid eventId,
        string idempotencyKey,
        IReadOnlyList<SeatCoordinate> seats,
        CancellationToken cancellationToken = default)
    {
        var (found, cached) = await _idempotencyStore.TryGetAsync(idempotencyKey, cancellationToken);
        if (found)
        {
            return cached!;
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                var result = await TryReserveSeatsAsync(eventId, seats, cancellationToken);
                await _idempotencyStore.StoreAsync(idempotencyKey, result, cancellationToken);
                return result;
            }
            catch (InvalidOperationException) when (attempt < MaxConcurrencyRetries)
            {
                // Another writer updated this event between our read and write (optimistic concurrency
                // conflict in MongoConcertEventRepository.UpdateAsync); retry with a fresh read.
            }
        }
    }

    public async Task<AvailabilitySummary> GetAvailabilitySummaryAsync(
        Guid eventId,
        CancellationToken cancellationToken = default)
    {
        var concertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken);
        if (concertEvent is null)
        {
            return new AvailabilitySummary(Available: 0, TemporarilyLocked: 0, Sold: 0);
        }

        var available = concertEvent.Seats.Count(s => s.Status == SeatStatus.Available);
        var locked = concertEvent.Seats.Count(s => s.Status == SeatStatus.TemporarilyLocked);
        var sold = concertEvent.Seats.Count(s => s.Status == SeatStatus.Sold);

        return new AvailabilitySummary(Available: available, TemporarilyLocked: locked, Sold: sold);
    }

    private async Task<IReadOnlyList<Seat>> TryReserveSeatsAsync(
        Guid eventId,
        IReadOnlyList<SeatCoordinate> seatCoordinates,
        CancellationToken cancellationToken)
    {
        var existingConcertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken);
        var isNewEvent = existingConcertEvent is null;
        var concertEvent = existingConcertEvent ?? new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow);

        var seatsToLock = new List<Seat>(seatCoordinates.Count);

        foreach (var coordinate in seatCoordinates)
        {
            var seat = concertEvent.Seats.SingleOrDefault(existingSeat =>
                string.Equals(existingSeat.Row, coordinate.Row, StringComparison.OrdinalIgnoreCase)
                && existingSeat.Number == coordinate.Number);

            if (seat is null)
            {
                seat = new Seat(coordinate.Row, coordinate.Number, 100m);
                concertEvent.AddSeat(seat);
            }

            seatsToLock.Add(seat);
        }

        var lockedUntilUtc = concertEvent.LockSeats(
            seatsToLock.Select(s => s.Id).ToList(),
            TimeSpan.FromMinutes(5));

        if (isNewEvent)
        {
            await _concertEventRepository.SaveAsync(concertEvent, cancellationToken);
        }
        else
        {
            await _concertEventRepository.UpdateAsync(concertEvent, cancellationToken);
        }

        foreach (var seat in seatsToLock)
        {
            await _reservationPublisher.PublishAsync(
                new SeatLockedIntegrationEvent(eventId, seat.Id, lockedUntilUtc),
                cancellationToken);
        }

        return seatsToLock;
    }
}
