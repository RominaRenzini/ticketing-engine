namespace TicketingEngine.Domain.Events;

public sealed record SeatLockedDomainEvent(Guid EventId, Guid SeatId, DateTimeOffset LockedUntilUtc);
