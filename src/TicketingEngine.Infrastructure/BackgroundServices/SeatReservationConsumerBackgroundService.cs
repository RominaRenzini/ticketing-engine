using Microsoft.Extensions.Hosting;

namespace TicketingEngine.Infrastructure.BackgroundServices;

public sealed class SeatReservationConsumerBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Seat reservation consumer is ready.");
        return Task.CompletedTask;
    }
}
