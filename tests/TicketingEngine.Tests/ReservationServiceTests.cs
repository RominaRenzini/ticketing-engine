using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task ReserveAsync_ShouldLockSeat()
    {
        var service = new ReservationService();

        var seat = await service.ReserveAsync(Guid.NewGuid(), "A", 12);

        Assert.Equal("A", seat.Row);
        Assert.Equal(12, seat.Number);
        Assert.Equal("TemporarilyLocked", seat.Status.ToString());
    }
}
