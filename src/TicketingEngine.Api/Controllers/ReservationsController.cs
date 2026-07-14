using Microsoft.AspNetCore.Mvc;
using TicketingEngine.Application.Abstractions;

namespace TicketingEngine.Api.Controllers;

[ApiController]
[Route("api/v1/events")]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost("{eventId:guid}/reserve")]
    public async Task<IActionResult> Reserve(Guid eventId, [FromBody] ReserveRequest request, CancellationToken cancellationToken)
    {
        var seat = await _reservationService.ReserveAsync(eventId, request.Row, request.Number, cancellationToken);
        return Ok(new { seat.Id, seat.Row, seat.Number, seat.Status });
    }
}

public sealed class ReserveRequest
{
    public string Row { get; init; } = string.Empty;
    public int Number { get; init; }
}
