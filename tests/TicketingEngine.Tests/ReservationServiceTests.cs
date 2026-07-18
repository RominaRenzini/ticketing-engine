using Xunit;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task ReserveAsync_ShouldLockSeat()
    {
        var service = new ReservationService(new StubReservationPublisher(), new ReservationStateStore());

        var seat = await service.ReserveAsync(Guid.NewGuid(), "A", 12);

        Assert.Equal("A", seat.Row);
        Assert.Equal(12, seat.Number);
        Assert.Equal("TemporarilyLocked", seat.Status.ToString());
        Assert.NotNull(seat.LockedUntilUtc);
    }

    [Fact]
    public async Task ReserveAsync_ShouldReleaseExpiredHold_WhenReconciled()
    {
        var store = new ReservationStateStore();
        var service = new ReservationService(new StubReservationPublisher(), store);
        var eventId = Guid.NewGuid();

        await service.ReserveAsync(eventId, "A", 12);
        store.ReconcileExpiredHolds(DateTimeOffset.UtcNow.AddMinutes(6));

        var concertEvent = store.GetEvent(eventId);

        Assert.NotNull(concertEvent);
        Assert.Single(concertEvent.Seats);
        Assert.Equal(SeatStatus.Available, concertEvent.Seats.Single().Status);
    }

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
