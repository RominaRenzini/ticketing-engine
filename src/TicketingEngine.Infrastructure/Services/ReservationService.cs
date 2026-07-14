using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Services;

public class ReservationService : IReservationService
{
    public Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default)
    {
        var seat = new Seat(row, number, 100m);
        seat.MarkLocked();
        return Task.FromResult(seat);
    }
}
