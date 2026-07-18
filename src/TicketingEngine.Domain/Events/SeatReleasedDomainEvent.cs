namespace TicketingEngine.Domain.Events;

public sealed record SeatReleasedDomainEvent(Guid EventId, Guid SeatId, DateTimeOffset ReleasedAtUtc);
