using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Api.Models;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

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

    [HttpPost("reserve")]
    public async Task<IActionResult> Reserve([FromQuery] Guid eventId, [FromBody] ReserveRequest request, CancellationToken cancellationToken)
    {
        if (request.Seats.Count > 0)
        {
            var selections = request.Seats.Select(seat => new SeatSelection(seat.Row, seat.Number)).ToArray();
            var reservation = await _reservationService.ReserveSeatsAsync(eventId, selections, request.Row, cancellationToken);
            return Accepted(new
            {
                reservation.Id,
                reservation.EventId,
                reservation.Status,
                reservation.SeatCount,
                Seats = reservation.SeatSelections.Select(seat => new { seat.Row, seat.Number })
            });
        }

        if (string.IsNullOrWhiteSpace(request.Row))
        {
            return BadRequest("Row is required.");
        }

        var seat = await _reservationService.ReserveAsync(eventId, request.Row, request.Number, cancellationToken);
        return Accepted(new { seat.Id, seat.Row, seat.Number, seat.Status });
    }

    [HttpGet("{eventId}/availability")]
    public async Task<IActionResult> GetAvailability([FromRoute] Guid eventId, CancellationToken cancellationToken)
    {
        var availability = await _reservationService.GetAvailabilityAsync(eventId, cancellationToken);
        return Ok(availability);
    }
}
