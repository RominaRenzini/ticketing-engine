using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Commands;

public sealed class ReserveSeatsCommandHandler
{
    private readonly IReservationService _reservationService;

    public ReserveSeatsCommandHandler(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public Task<IReadOnlyList<Seat>> Handle(ReserveSeatsCommand request, CancellationToken cancellationToken)
        => _reservationService.ReserveSeatsAsync(request.EventId, request.IdempotencyKey, request.Seats, cancellationToken);
}
