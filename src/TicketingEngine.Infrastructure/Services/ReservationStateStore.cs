using TicketingEngine.Domain.Entities;
using TicketingEngine.Domain.Events;

namespace TicketingEngine.Infrastructure.Services;

public sealed class ReservationStateStore
{
    private readonly Dictionary<Guid, ConcertEvent> _events = new();

    public void Save(ConcertEvent concertEvent)
    {
        _events[concertEvent.Id] = concertEvent;
    }

    public ConcertEvent? GetEvent(Guid eventId)
    {
        _events.TryGetValue(eventId, out var concertEvent);
        return concertEvent;
    }

    public void ReconcileExpiredHolds(DateTimeOffset now)
    {
        foreach (var concertEvent in _events.Values)
        {
            foreach (var seat in concertEvent.Seats)
            {
                if (seat.Status == SeatStatus.TemporarilyLocked && seat.LockedUntilUtc.HasValue && seat.LockedUntilUtc.Value <= now)
                {
                    concertEvent.ReleaseExpiredHold(seat.Id, now);
                }
            }
        }
    }
}
