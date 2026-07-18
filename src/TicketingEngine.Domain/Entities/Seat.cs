namespace TicketingEngine.Domain.Entities;

public class Seat
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Row { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public decimal Price { get; private set; }
    public SeatStatus Status { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }

    public Seat(string row, int number, decimal price)
    {
        Row = row;
        Number = number;
        Price = price;
        Status = SeatStatus.Available;
    }

    private Seat(Guid id, string row, int number, decimal price, SeatStatus status, DateTimeOffset? lockedUntilUtc)
    {
        Id = id;
        Row = row;
        Number = number;
        Price = price;
        Status = status;
        LockedUntilUtc = lockedUntilUtc;
    }

    public static Seat Rehydrate(Guid id, string row, int number, decimal price, SeatStatus status, DateTimeOffset? lockedUntilUtc)
    {
        return new Seat(id, row, number, price, status, lockedUntilUtc);
    }

    public void MarkLocked(DateTimeOffset lockedUntilUtc)
    {
        Status = SeatStatus.TemporarilyLocked;
        LockedUntilUtc = lockedUntilUtc;
    }

    public void MarkSold() => Status = SeatStatus.Sold;

    public void MarkAvailable()
    {
        Status = SeatStatus.Available;
        LockedUntilUtc = null;
    }
}

public enum SeatStatus
{
    Available,
    TemporarilyLocked,
    Sold
}
