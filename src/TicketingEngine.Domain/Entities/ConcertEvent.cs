using TicketingEngine.Domain.Events;
using TicketingEngine.Domain.Exceptions;

namespace TicketingEngine.Domain.Entities;

public class ConcertEvent
{
    private readonly List<Seat> _seats;
    private readonly List<object> _domainEvents = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTimeOffset StartsAt { get; private set; }
    public IReadOnlyList<Seat> Seats => _seats;
    public IReadOnlyList<object> DomainEvents => _domainEvents;

    public ConcertEvent(Guid id, string name, DateTimeOffset startsAt, IEnumerable<Seat>? seats = null)
    {
        Id = id;
        Name = name;
        StartsAt = startsAt;
        _seats = seats?.ToList() ?? new List<Seat>();
    }

    public DateTimeOffset LockSeat(Guid seatId, TimeSpan duration)
    {
        var seat = _seats.SingleOrDefault(s => s.Id == seatId)
            ?? throw new SeatLockException($"Seat {seatId} was not found for event {Id}.");

        if (seat.Status == SeatStatus.TemporarilyLocked || seat.Status == SeatStatus.Sold)
        {
            throw new SeatLockException($"Seat {seatId} is already locked or sold.");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new SeatLockException("Lock duration must be positive.");
        }

        seat.MarkLocked();
        var lockedUntilUtc = DateTimeOffset.UtcNow.Add(duration);
        _domainEvents.Add(new SeatLockedDomainEvent(Id, seat.Id, lockedUntilUtc));
        return lockedUntilUtc;
    }
}
