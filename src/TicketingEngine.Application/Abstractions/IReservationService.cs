using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Abstractions;

public interface IReservationService
{
    Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default);
}
