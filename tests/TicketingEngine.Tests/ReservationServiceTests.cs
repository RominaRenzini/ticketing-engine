using Xunit;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task ReserveAsync_ShouldLockSeat_AndPersistThroughRepository()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        var seat = await service.ReserveAsync(eventId, "A", 12);
        var persistedEvent = await repository.GetByIdAsync(eventId);

        Assert.Equal("A", seat.Row);
        Assert.Equal(12, seat.Number);
        Assert.Equal(SeatStatus.TemporarilyLocked, seat.Status);
        Assert.NotNull(seat.LockedUntilUtc);
        Assert.NotNull(persistedEvent);
        Assert.Single(persistedEvent!.Seats);
        Assert.Equal(SeatStatus.TemporarilyLocked, persistedEvent.Seats.Single().Status);
    }

    [Fact]
    public async Task Repository_ShouldPersistLockTransition_AndExpiredReleaseTransition()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 12);
        var afterLock = await repository.GetByIdAsync(eventId);
        var seatAfterLock = afterLock!.Seats.Single();
        Assert.Equal(SeatStatus.TemporarilyLocked, seatAfterLock.Status);

        var released = afterLock.ReleaseExpiredHold(seatAfterLock.Id, DateTimeOffset.UtcNow.AddMinutes(6));
        await repository.UpdateAsync(afterLock);

        var afterRelease = await repository.GetByIdAsync(eventId);
        var seatAfterRelease = afterRelease!.Seats.Single();

        Assert.True(released);
        Assert.Equal(SeatStatus.Available, seatAfterRelease.Status);
        Assert.Null(seatAfterRelease.LockedUntilUtc);
    }

    [Fact]
    public async Task ReserveAsync_ShouldRetry_WhenUpdateHitsAConcurrencyConflict()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 12);

        repository.FailNextUpdateWith(new InvalidOperationException("Concurrent update detected."));
        var seat = await service.ReserveAsync(eventId, "B", 5);

        Assert.Equal("B", seat.Row);
        Assert.Equal(1, repository.FailedUpdateCount);
    }

    [Fact]
    public async Task ReserveAsync_ShouldSurfaceConflict_WhenRetriesAreExhausted()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = new ReservationService(new StubReservationPublisher(), repository);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 12);

        repository.FailEveryUpdateWith(new InvalidOperationException("Concurrent update detected."));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReserveAsync(eventId, "B", 5));
    }

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryConcertEventRepository : IConcertEventRepository
    {
        private readonly Dictionary<Guid, ConcertEvent> _store = new();
        private readonly Dictionary<string, Reservation> _reservations = new(StringComparer.OrdinalIgnoreCase);
        private Exception? _nextUpdateFailure;
        private Exception? _everyUpdateFailure;

        public int FailedUpdateCount { get; private set; }

        public void FailNextUpdateWith(Exception exception) => _nextUpdateFailure = exception;

        public void FailEveryUpdateWith(Exception exception) => _everyUpdateFailure = exception;

        public Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(eventId, out var concertEvent);
            return Task.FromResult(concertEvent is null ? null : Clone(concertEvent));
        }

        public Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyCollection<ConcertEvent> events = _store.Values.Select(Clone).ToArray();
            return Task.FromResult(events);
        }

        public Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
        {
            _store[concertEvent.Id] = Clone(concertEvent);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
        {
            if (_everyUpdateFailure is not null)
            {
                FailedUpdateCount++;
                throw _everyUpdateFailure;
            }

            if (_nextUpdateFailure is not null)
            {
                var failure = _nextUpdateFailure;
                _nextUpdateFailure = null;
                FailedUpdateCount++;
                throw failure;
            }

            _store[concertEvent.Id] = Clone(concertEvent);
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
            var seats = source.Seats.Select(seat =>
                Seat.Rehydrate(seat.Id, seat.Row, seat.Number, seat.Price, seat.Status, seat.LockedUntilUtc));

            return new ConcertEvent(source.Id, source.Name, source.StartsAt, seats);
        }

        private static Reservation Clone(Reservation source)
        {
            return Reservation.Rehydrate(
                source.Id,
                source.EventId,
                source.Status,
                source.SeatSelections.Select(selection => new SeatSelection(selection.Row, selection.Number)).ToArray(),
                source.IdempotencyKey);
        }
    }
}
