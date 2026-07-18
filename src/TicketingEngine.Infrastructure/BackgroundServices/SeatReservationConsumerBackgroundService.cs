using Microsoft.Extensions.Hosting;
using TicketingEngine.Infrastructure.Services;

namespace TicketingEngine.Infrastructure.BackgroundServices;

public sealed class SeatReservationConsumerBackgroundService : BackgroundService
{
    private readonly ReservationStateStore _stateStore;

    public SeatReservationConsumerBackgroundService(ReservationStateStore stateStore)
    {
        _stateStore = stateStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Seat reservation consumer is ready.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _stateStore.ReconcileExpiredHolds(DateTimeOffset.UtcNow);
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
