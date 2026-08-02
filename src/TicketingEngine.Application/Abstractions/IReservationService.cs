using TicketingEngine.Application.Models;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Abstractions;

public interface IReservationService
{
    Task<IReadOnlyList<Seat>> ReserveSeatsAsync(
        Guid eventId,
        string idempotencyKey,
        IReadOnlyList<SeatCoordinate> seats,
        CancellationToken cancellationToken = default);

    Task<AvailabilitySummary> GetAvailabilitySummaryAsync(
        Guid eventId,
        CancellationToken cancellationToken = default);
}

public interface IConcertEventRepository
{
    Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
}
