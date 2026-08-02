namespace TicketingEngine.Api.Models;

public sealed class AvailabilitySummaryResponse
{
    public int Available { get; init; }
    public int TemporarilyLocked { get; init; }
    public int Sold { get; init; }
}
