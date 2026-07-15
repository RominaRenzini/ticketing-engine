using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Commands;

public sealed record ReserveSeatCommand(Guid EventId, string Row, int Number);
