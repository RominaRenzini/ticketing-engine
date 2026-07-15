namespace TicketingEngine.Application.Events;

public sealed record SeatLockedIntegrationEvent(Guid EventId, Guid SeatId, DateTimeOffset LockedUntilUtc);
