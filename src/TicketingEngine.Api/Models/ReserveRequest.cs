namespace TicketingEngine.Api.Models;

public sealed class ReserveRequest
{
    public string Row { get; init; } = string.Empty;
    public int Number { get; init; }
    public List<SeatSelectionRequest> Seats { get; init; } = new();
}

public sealed class SeatSelectionRequest
{
    public string Row { get; init; } = string.Empty;
    public int Number { get; init; }
}
