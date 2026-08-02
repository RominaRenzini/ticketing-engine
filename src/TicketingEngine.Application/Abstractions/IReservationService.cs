using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Abstractions;

public interface IReservationService
{
    Task<Seat> ReserveAsync(Guid eventId, string row, int number, CancellationToken cancellationToken = default);
    Task<Reservation> ReserveSeatsAsync(Guid eventId, IEnumerable<SeatSelection> seatSelections, string? idempotencyKey = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AvailabilitySummary>> GetAvailabilityAsync(Guid eventId, CancellationToken cancellationToken = default);
}

public interface IConcertEventRepository
{
    Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
    Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default);
    Task<Reservation?> GetReservationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task SaveReservationAsync(Reservation reservation, CancellationToken cancellationToken = default);
    Task UpdateReservationAsync(Reservation reservation, CancellationToken cancellationToken = default);
}

public sealed record AvailabilitySummary(string Row, int TotalSeats, int AvailableSeats, int LockedSeats, int SoldSeats);
