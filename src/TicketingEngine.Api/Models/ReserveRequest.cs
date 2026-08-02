namespace TicketingEngine.Api.Models;

public sealed class SeatRequest
{
    public string Row { get; init; } = string.Empty;
    public int Number { get; init; }
}

public sealed class ReserveRequest
{
    public string IdempotencyKey { get; init; } = string.Empty;
    public IReadOnlyList<SeatRequest> Seats { get; init; } = Array.Empty<SeatRequest>();
}

