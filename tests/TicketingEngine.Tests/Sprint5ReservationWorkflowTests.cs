using Xunit;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Domain.Exceptions;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class Sprint5ReservationWorkflowTests
{
    [Fact]
    public async Task ReserveSeatsAsync_ShouldLockMultipleSeats_WhenAllAreAvailable()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        var reservation = await service.ReserveSeatsAsync(eventId, new[]
        {
            new SeatSelection("A", 1),
            new SeatSelection("A", 2)
        }, "idempotency-key-1");

        Assert.Equal(ReservationStatus.Pending, reservation.Status);
        Assert.Equal(2, reservation.SeatCount);

        var persistedEvent = await repository.GetByIdAsync(eventId);
        Assert.NotNull(persistedEvent);
        Assert.Equal(2, persistedEvent!.Seats.Count(seat => seat.Status == SeatStatus.TemporarilyLocked));
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldNotLockAnySeat_WhenAnySeatIsUnavailable()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 1);

        await Assert.ThrowsAsync<SeatLockException>(() => service.ReserveSeatsAsync(eventId, new[]
        {
            new SeatSelection("A", 1),
            new SeatSelection("A", 2)
        }, "idempotency-key-2"));

        var persistedEvent = await repository.GetByIdAsync(eventId);
        Assert.Equal(1, persistedEvent!.Seats.Count(seat => seat.Status == SeatStatus.TemporarilyLocked));
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldReturnSameReservation_ForSameIdempotencyKey()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        var first = await service.ReserveSeatsAsync(eventId, new[]
        {
            new SeatSelection("B", 3)
        }, "repeat-key");

        var second = await service.ReserveSeatsAsync(eventId, new[]
        {
            new SeatSelection("B", 3)
        }, "repeat-key");

        Assert.Equal(first.Id, second.Id);
        Assert.Equal(1, repository.ReservationCount);
    }

    [Fact]
    public async Task GetAvailabilityAsync_ShouldReflectLockedAndSoldSeats()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 1);

        var persistedEvent = await repository.GetByIdAsync(eventId);
        var lockedSeat = persistedEvent!.Seats.Single(seat => seat.Number == 1);
        var soldSeat = new Seat("A", 2, 100m);
        soldSeat.MarkSold();
        persistedEvent.AddSeat(soldSeat);
        var availableSeat = new Seat("A", 3, 100m);
        persistedEvent.AddSeat(availableSeat);
        await repository.UpdateAsync(persistedEvent);

        var availability = await service.GetAvailabilityAsync(eventId);
        var summary = Assert.Single(availability.Where(item => item.Row == "A"));

        Assert.Equal(3, summary.TotalSeats);
        Assert.Equal(1, summary.AvailableSeats);
        Assert.Equal(1, summary.LockedSeats);
        Assert.Equal(1, summary.SoldSeats);
    }

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryConcertEventRepository : IConcertEventRepository
    {
        private readonly Dictionary<Guid, ConcertEvent> _events = new();
        private readonly Dictionary<string, Reservation> _reservations = new(StringComparer.OrdinalIgnoreCase);

        public int ReservationCount => _reservations.Count;

        public Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _events.TryGetValue(eventId, out var concertEvent);
            return Task.FromResult(concertEvent is null ? null : Clone(concertEvent));
        }

        public Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<ConcertEvent>>(_events.Values.Select(Clone).ToArray());
        }

        public Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
        {
            _events[concertEvent.Id] = Clone(concertEvent);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
        {
            _events[concertEvent.Id] = Clone(concertEvent);
            return Task.CompletedTask;
        }

        public Task<Reservation?> GetReservationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
        {
            _reservations.TryGetValue(idempotencyKey, out var reservation);
            return Task.FromResult(reservation is null ? null : Clone(reservation));
        }

        public Task SaveReservationAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            _reservations[reservation.IdempotencyKey!] = Clone(reservation);
            return Task.CompletedTask;
        }

        public Task UpdateReservationAsync(Reservation reservation, CancellationToken cancellationToken = default)
        {
            _reservations[reservation.IdempotencyKey!] = Clone(reservation);
            return Task.CompletedTask;
        }

        private static ConcertEvent Clone(ConcertEvent source)
        {
            var seats = source.Seats.Select(seat => Seat.Rehydrate(seat.Id, seat.Row, seat.Number, seat.Price, seat.Status, seat.LockedUntilUtc));
            return new ConcertEvent(source.Id, source.Name, source.StartsAt, seats);
        }

        private static Reservation Clone(Reservation source)
        {
            return Reservation.Rehydrate(source.Id, source.EventId, source.Status, source.SeatSelections.Select(item => new SeatSelection(item.Row, item.Number)).ToArray(), source.IdempotencyKey);
        }
    }
}
