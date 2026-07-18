using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Api.Models;
using TicketingEngine.Application.Abstractions;

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
        if (string.IsNullOrWhiteSpace(request.Row))
        {
            return BadRequest("Row is required.");
        }

        var seat = await _reservationService.ReserveAsync(eventId, request.Row, request.Number, cancellationToken);
        return Accepted(new { seat.Id, seat.Row, seat.Number, seat.Status });
    }
}
