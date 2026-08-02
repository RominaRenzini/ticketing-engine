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

    [Fact]
    public void LockSeats_ShouldLockAllSeatsAtomically_AndEmitOneDomainEventPerSeat()
    {
        var seatA = new Seat("A", 1, 100m);
        var seatB = new Seat("A", 2, 100m);
        var seatC = new Seat("B", 1, 100m);
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow, new[] { seatA, seatB, seatC });

        var lockedUntil = concertEvent.LockSeats(new[] { seatA.Id, seatB.Id, seatC.Id }, TimeSpan.FromMinutes(5));

        Assert.Equal(SeatStatus.TemporarilyLocked, seatA.Status);
        Assert.Equal(SeatStatus.TemporarilyLocked, seatB.Status);
        Assert.Equal(SeatStatus.TemporarilyLocked, seatC.Status);
        Assert.True(lockedUntil > DateTimeOffset.UtcNow);
        Assert.Equal(3, concertEvent.DomainEvents.Count);
        Assert.All(concertEvent.DomainEvents, e => Assert.IsType<SeatLockedDomainEvent>(e));
    }

    [Fact]
    public void LockSeats_ShouldLockNoSeats_WhenAnySeatIsAlreadyLocked()
    {
        var seatA = new Seat("A", 1, 100m);
        var seatB = new Seat("A", 2, 100m);
        seatB.MarkLocked(DateTimeOffset.UtcNow.AddMinutes(5));
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow, new[] { seatA, seatB });

        var exception = Assert.Throws<SeatLockException>(() =>
            concertEvent.LockSeats(new[] { seatA.Id, seatB.Id }, TimeSpan.FromMinutes(5)));

        Assert.Contains("already locked", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(SeatStatus.Available, seatA.Status);
    }

    [Fact]
    public void LockSeats_ShouldThrow_WhenSeatListIsEmpty()
    {
        var concertEvent = new ConcertEvent(Guid.NewGuid(), "Test Event", DateTimeOffset.UtcNow);

        var exception = Assert.Throws<SeatLockException>(() =>
            concertEvent.LockSeats(Array.Empty<Guid>(), TimeSpan.FromMinutes(5)));

        Assert.Contains("at least one seat", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
