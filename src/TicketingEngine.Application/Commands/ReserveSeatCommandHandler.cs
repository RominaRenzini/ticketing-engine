using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Application.Commands;

public sealed class ReserveSeatCommandHandler
{
    private readonly IReservationService _reservationService;

    public ReserveSeatCommandHandler(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    public Task<Seat> Handle(ReserveSeatCommand request, CancellationToken cancellationToken)
        => _reservationService.ReserveAsync(request.EventId, request.Row, request.Number, cancellationToken);
}
