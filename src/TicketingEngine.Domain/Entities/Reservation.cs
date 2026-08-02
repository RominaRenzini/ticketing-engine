namespace TicketingEngine.Domain.Entities;

public sealed class Reservation
{
    private readonly List<SeatSelection> _seatSelections;

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public ReservationStatus Status { get; private set; }
    public IReadOnlyList<SeatSelection> SeatSelections => _seatSelections;
    public int SeatCount => _seatSelections.Count;
    public string? IdempotencyKey { get; private set; }

    public Reservation(Guid id, Guid eventId, IEnumerable<SeatSelection> seatSelections, string? idempotencyKey = null)
    {
        Id = id;
        EventId = eventId;
        _seatSelections = seatSelections.ToList();
        IdempotencyKey = idempotencyKey;
        Status = ReservationStatus.Pending;
    }

    private Reservation(Guid id, Guid eventId, IReadOnlyList<SeatSelection> seatSelections, ReservationStatus status, string? idempotencyKey)
    {
        Id = id;
        EventId = eventId;
        _seatSelections = seatSelections.ToList();
        Status = status;
        IdempotencyKey = idempotencyKey;
    }

    public static Reservation Rehydrate(Guid id, Guid eventId, ReservationStatus status, IReadOnlyList<SeatSelection> seatSelections, string? idempotencyKey)
        => new(id, eventId, seatSelections, status, idempotencyKey);

    public void Confirm() => Status = ReservationStatus.Confirmed;

    public void Expire() => Status = ReservationStatus.Expired;

    public void Release() => Status = ReservationStatus.Released;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Expired,
    Released
}

public sealed record SeatSelection(string Row, int Number);
