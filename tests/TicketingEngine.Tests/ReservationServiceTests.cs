using Xunit;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Application.Events;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task ReserveAsync_ShouldLockSeat()
    {
        var service = new ReservationService(new StubReservationPublisher());

        var seat = await service.ReserveAsync(Guid.NewGuid(), "A", 12);

        Assert.Equal("A", seat.Row);
        Assert.Equal(12, seat.Number);
        Assert.Equal("TemporarilyLocked", seat.Status.ToString());
    }

    private sealed class StubReservationPublisher : IReservationPublisher
    {
        public Task PublishAsync(SeatLockedIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
