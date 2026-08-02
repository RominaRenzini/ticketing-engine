using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Domain.Exceptions;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    private const int MaxConcurrencyRetries = 3;
    private static readonly TimeSpan LockDuration = TimeSpan.FromMinutes(5);

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
            }
        }
    }

    public async Task<Reservation> ReserveSeatsAsync(Guid eventId, IEnumerable<SeatSelection> seatSelections, string? idempotencyKey = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            idempotencyKey = Guid.NewGuid().ToString("N");
        }

        var existingReservation = await _concertEventRepository.GetReservationByIdempotencyKeyAsync(idempotencyKey, cancellationToken);
        if (existingReservation is not null)
        {
            return existingReservation;
        }

        var selections = seatSelections.Distinct().ToArray();
        if (selections.Length == 0)
        {
            throw new SeatLockException("At least one seat selection is required.");
        }

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                return await TryReserveSeatsAsync(eventId, selections, idempotencyKey, cancellationToken);
            }
            catch (InvalidOperationException) when (attempt < MaxConcurrencyRetries)
            {
            }
        }
    }

    public async Task<IReadOnlyList<AvailabilitySummary>> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var concertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken);
        if (concertEvent is null)
        {
            return Array.Empty<AvailabilitySummary>();
        }

        return concertEvent.Seats
            .GroupBy(seat => seat.Row, StringComparer.OrdinalIgnoreCase)
            .Select(group => new AvailabilitySummary(
                group.Key,
                group.Count(),
                group.Count(seat => seat.Status == SeatStatus.Available),
                group.Count(seat => seat.Status == SeatStatus.TemporarilyLocked),
                group.Count(seat => seat.Status == SeatStatus.Sold)))
            .OrderBy(summary => summary.Row, StringComparer.OrdinalIgnoreCase)
            .ToArray();
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
        var lockedUntilUtc = concertEvent.LockSeat(seatToLock.Id, LockDuration);

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

    private async Task<Reservation> TryReserveSeatsAsync(Guid eventId, IReadOnlyList<SeatSelection> seatSelections, string idempotencyKey, CancellationToken cancellationToken)
    {
        var existingConcertEvent = await _concertEventRepository.GetByIdAsync(eventId, cancellationToken);
        var isNewEvent = existingConcertEvent is null;
        var concertEvent = existingConcertEvent ?? new ConcertEvent(eventId, "Reserved Event", DateTimeOffset.UtcNow);

        var reservation = new Reservation(Guid.NewGuid(), eventId, seatSelections, idempotencyKey);
        var seatsToLock = new List<Seat>();

        foreach (var selection in seatSelections)
        {
            var seat = concertEvent.Seats.SingleOrDefault(existingSeat =>
                string.Equals(existingSeat.Row, selection.Row, StringComparison.OrdinalIgnoreCase)
                && existingSeat.Number == selection.Number);

            if (seat is null)
            {
                seat = new Seat(selection.Row, selection.Number, 100m);
                concertEvent.AddSeat(seat);
            }

            if (seat.Status == SeatStatus.TemporarilyLocked || seat.Status == SeatStatus.Sold)
            {
                throw new SeatLockException($"Seat {selection.Row} {selection.Number} is already locked or sold.");
            }

            seatsToLock.Add(seat);
        }

        foreach (var seat in seatsToLock)
        {
            concertEvent.LockSeat(seat.Id, LockDuration);
        }

        if (isNewEvent)
        {
            await _concertEventRepository.SaveAsync(concertEvent, cancellationToken);
        }
        else
        {
            await _concertEventRepository.UpdateAsync(concertEvent, cancellationToken);
        }

        await _concertEventRepository.SaveReservationAsync(reservation, cancellationToken);

        foreach (var seat in seatsToLock)
        {
            await _reservationPublisher.PublishAsync(
                new SeatLockedIntegrationEvent(eventId, seat.Id, seat.LockedUntilUtc ?? DateTimeOffset.UtcNow.Add(LockDuration)),
                cancellationToken);
        }

        return reservation;
    }
}
