using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Abstractions;

public interface IReservationService
{
    Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default);
}

public interface IConcertEventRepository
{
    Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
}
