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

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class InMemoryConcertEventRepository : IConcertEventRepository
    {
        private readonly Dictionary<Guid, ConcertEvent> _store = new();

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
