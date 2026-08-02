using Xunit;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Application.Models;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class ReservationServiceTests
{
    private static ReservationService CreateService(
        InMemoryConcertEventRepository? repository = null,
        IIdempotencyStore<IReadOnlyList<Seat>>? idempotencyStore = null)
    {
        repository ??= new InMemoryConcertEventRepository();
        idempotencyStore ??= new InMemoryIdempotencyStore<IReadOnlyList<Seat>>();
        return new ReservationService(new StubReservationPublisher(), repository, idempotencyStore);
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldLockSeats_AndPersistThroughRepository()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        var seats = await service.ReserveSeatsAsync(eventId, "key-1", new[] { new SeatCoordinate("A", 12) });
        var persistedEvent = await repository.GetByIdAsync(eventId);

        var seat = Assert.Single(seats);
        Assert.Equal("A", seat.Row);
        Assert.Equal(12, seat.Number);
        Assert.Equal(SeatStatus.TemporarilyLocked, seat.Status);
        Assert.NotNull(seat.LockedUntilUtc);
        Assert.NotNull(persistedEvent);
        Assert.Single(persistedEvent!.Seats);
        Assert.Equal(SeatStatus.TemporarilyLocked, persistedEvent.Seats.Single().Status);
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldLockMultipleSeats_InASingleCall()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        var seats = await service.ReserveSeatsAsync(
            eventId,
            "key-multi",
            new[] { new SeatCoordinate("A", 1), new SeatCoordinate("A", 2), new SeatCoordinate("B", 1) });

        Assert.Equal(3, seats.Count);
        Assert.All(seats, s => Assert.Equal(SeatStatus.TemporarilyLocked, s.Status));
        Assert.All(seats, s => Assert.NotNull(s.LockedUntilUtc));

        var persistedEvent = await repository.GetByIdAsync(eventId);
        Assert.NotNull(persistedEvent);
        Assert.Equal(3, persistedEvent!.Seats.Count);
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldReturnCachedResult_ForDuplicateIdempotencyKey()
    {
        var repository = new InMemoryConcertEventRepository();
        var idempotencyStore = new InMemoryIdempotencyStore<IReadOnlyList<Seat>>();
        var service = CreateService(repository, idempotencyStore);
        var eventId = Guid.NewGuid();
        const string idempotencyKey = "idempotent-key-1";

        var firstResult = await service.ReserveSeatsAsync(eventId, idempotencyKey, new[] { new SeatCoordinate("A", 1) });
        var secondResult = await service.ReserveSeatsAsync(eventId, idempotencyKey, new[] { new SeatCoordinate("A", 1) });

        Assert.Same(firstResult, secondResult);
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldRejectDuplicateIdempotencyKey_WithoutCallingRepository()
    {
        var repository = new InMemoryConcertEventRepository();
        var idempotencyStore = new InMemoryIdempotencyStore<IReadOnlyList<Seat>>();
        var service = CreateService(repository, idempotencyStore);
        var eventId = Guid.NewGuid();
        const string key = "replay-key";

        await service.ReserveSeatsAsync(eventId, key, new[] { new SeatCoordinate("A", 1) });
        repository.TrackNextRead();
        await service.ReserveSeatsAsync(eventId, key, new[] { new SeatCoordinate("A", 1) });

        Assert.Equal(0, repository.ReadsAfterCheckpoint);
    }

    [Fact]
    public async Task Repository_ShouldPersistLockTransition_AndExpiredReleaseTransition()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        await service.ReserveSeatsAsync(eventId, "key-1", new[] { new SeatCoordinate("A", 12) });
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
    public async Task ReserveSeatsAsync_ShouldRetry_WhenUpdateHitsAConcurrencyConflict()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        await service.ReserveSeatsAsync(eventId, "key-1", new[] { new SeatCoordinate("A", 12) });

        repository.FailNextUpdateWith(new InvalidOperationException("Concurrent update detected."));
        var seats = await service.ReserveSeatsAsync(eventId, "key-2", new[] { new SeatCoordinate("B", 5) });

        Assert.Single(seats);
        Assert.Equal("B", seats[0].Row);
        Assert.Equal(1, repository.FailedUpdateCount);
    }

    [Fact]
    public async Task ReserveSeatsAsync_ShouldSurfaceConflict_WhenRetriesAreExhausted()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        await service.ReserveSeatsAsync(eventId, "key-1", new[] { new SeatCoordinate("A", 12) });

        repository.FailEveryUpdateWith(new InvalidOperationException("Concurrent update detected."));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.ReserveSeatsAsync(eventId, "key-2", new[] { new SeatCoordinate("B", 5) }));
    }

    [Fact]
    public async Task GetAvailabilitySummaryAsync_ShouldReturnZeroCounts_ForUnknownEvent()
    {
        var service = CreateService();
        var summary = await service.GetAvailabilitySummaryAsync(Guid.NewGuid());

        Assert.Equal(0, summary.Available);
        Assert.Equal(0, summary.TemporarilyLocked);
        Assert.Equal(0, summary.Sold);
    }

    [Fact]
    public async Task GetAvailabilitySummaryAsync_ShouldReflectCurrentSeatStatuses()
    {
        var repository = new InMemoryConcertEventRepository();
        var service = CreateService(repository);
        var eventId = Guid.NewGuid();

        await service.ReserveSeatsAsync(
            eventId,
            "key-1",
            new[] { new SeatCoordinate("A", 1), new SeatCoordinate("A", 2) });

        var concertEvent = await repository.GetByIdAsync(eventId);
        var seat = concertEvent!.Seats.First();
        seat.MarkSold();
        await repository.UpdateAsync(concertEvent);

        var summary = await service.GetAvailabilitySummaryAsync(eventId);

        Assert.Equal(0, summary.Available);
        Assert.Equal(1, summary.TemporarilyLocked);
        Assert.Equal(1, summary.Sold);
    }

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryConcertEventRepository : IConcertEventRepository
    {
        private readonly Dictionary<Guid, ConcertEvent> _store = new();
        private Exception? _nextUpdateFailure;
        private Exception? _everyUpdateFailure;
        private bool _trackingReads;

        public int FailedUpdateCount { get; private set; }
        public int ReadsAfterCheckpoint { get; private set; }

        public void FailNextUpdateWith(Exception exception) => _nextUpdateFailure = exception;
        public void FailEveryUpdateWith(Exception exception) => _everyUpdateFailure = exception;

        public void TrackNextRead()
        {
            _trackingReads = true;
            ReadsAfterCheckpoint = 0;
        }

        public Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
        {
            if (_trackingReads)
            {
                ReadsAfterCheckpoint++;
            }

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

        private static ConcertEvent Clone(ConcertEvent source)
        {
            var seats = source.Seats.Select(seat =>
                Seat.Rehydrate(seat.Id, seat.Row, seat.Number, seat.Price, seat.Status, seat.LockedUntilUtc));

            return new ConcertEvent(source.Id, source.Name, source.StartsAt, seats);
        }
    }
}
