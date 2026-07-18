using Xunit;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Domain.Events;
using TicketingEngine.Domain.Exceptions;

namespace TicketingEngine.Tests;

public class ConcertEventTests
{
    [Fact]
    public void LockSeat_ShouldLockAnAvailableSeatAndEmitADomainEvent()
    {
        var seat = new Seat("A", 1, 100m);
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow, new[] { seat });

        var lockedUntil = concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5));

        Assert.Equal(SeatStatus.TemporarilyLocked, seat.Status);
        Assert.True(lockedUntil > DateTimeOffset.UtcNow);
        Assert.Single(concertEvent.DomainEvents);
        Assert.IsType<SeatLockedDomainEvent>(concertEvent.DomainEvents[0]);
    }

    [Fact]
    public void LockSeat_ShouldThrow_WhenSeatIsAlreadyLocked()
    {
        var seat = new Seat("A", 2, 100m);
        seat.MarkLocked(DateTimeOffset.UtcNow.AddMinutes(5));
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow, new[] { seat });

        var exception = Assert.Throws<SeatLockException>(() => concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5)));

        Assert.Contains("already locked", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ReleaseExpiredHold_ShouldMakeSeatAvailable_WhenTheHoldHasExpired()
    {
        var seat = new Seat("A", 3, 100m);
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow, new[] { seat });

        concertEvent.LockSeat(seat.Id, TimeSpan.FromMilliseconds(1));
        Thread.Sleep(20);

        var released = concertEvent.ReleaseExpiredHold(seat.Id, DateTimeOffset.UtcNow);

        Assert.True(released);
        Assert.Equal(SeatStatus.Available, seat.Status);
        Assert.Null(seat.LockedUntilUtc);
    }
}
