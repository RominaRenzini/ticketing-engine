namespace TicketingEngine.Domain.Exceptions;

public sealed class SeatLockException : Exception
{
    public SeatLockException(string message) : base(message)
    {
    }
}
