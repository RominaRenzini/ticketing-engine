using TicketingEngine.Application.Models;

namespace TicketingEngine.Application.Commands;

public sealed record ReserveSeatsCommand(Guid EventId, string IdempotencyKey, IReadOnlyList<SeatCoordinate> Seats);
