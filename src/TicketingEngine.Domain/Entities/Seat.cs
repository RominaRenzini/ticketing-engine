namespace TicketingEngine.Domain.Entities;

public class Seat
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Row { get; private set; } = string.Empty;
    public int Number { get; private set; }
    public decimal Price { get; private set; }
    public SeatStatus Status { get; private set; }

    public Seat(string row, int number, decimal price)
    {
        Row = row;
        Number = number;
        Price = price;
        Status = SeatStatus.Available;
    }

    public void MarkLocked() => Status = SeatStatus.TemporarilyLocked;
    public void MarkSold() => Status = SeatStatus.Sold;
    public void MarkAvailable() => Status = SeatStatus.Available;
}

public enum SeatStatus
{
    Available,
    TemporarilyLocked,
    Sold
}
