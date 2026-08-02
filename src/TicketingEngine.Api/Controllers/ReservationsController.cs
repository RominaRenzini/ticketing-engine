using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Api.Models;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Models;

namespace TicketingEngine.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/events")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("{eventId}/reserve")]
    public async Task<IActionResult> Reserve(string eventId, [FromBody] ReserveRequest request, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(eventId, out var eventGuid))
        {
            return BadRequest("eventId must be a valid GUID.");
        }

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            return BadRequest("IdempotencyKey is required.");
        }

        if (request.Seats is null || request.Seats.Count == 0)
        {
            return BadRequest("At least one seat must be specified.");
        }

        var seatCoordinates = request.Seats
            .Select(s => new SeatCoordinate(s.Row, s.Number))
            .ToList();

        var seats = await _reservationService.ReserveSeatsAsync(eventGuid, request.IdempotencyKey, seatCoordinates, cancellationToken);
        return Accepted(seats.Select(s => new { s.Id, s.Row, s.Number, s.Status }));
    }

    [HttpGet("{eventId}/availability")]
    public async Task<IActionResult> GetAvailability(string eventId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(eventId, out var eventGuid))
        {
            return BadRequest("eventId must be a valid GUID.");
        }

        var summary = await _reservationService.GetAvailabilitySummaryAsync(eventGuid, cancellationToken);
        return Ok(new AvailabilitySummaryResponse
        {
            Available = summary.Available,
            TemporarilyLocked = summary.TemporarilyLocked,
            Sold = summary.Sold
        });
    }
}
