using Microsoft.Extensions.Hosting;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.BackgroundServices;

public sealed class SeatReservationConsumerBackgroundService : BackgroundService
{
    private readonly IConcertEventRepository _concertEventRepository;

    public SeatReservationConsumerBackgroundService(IConcertEventRepository concertEventRepository)
    {
        _concertEventRepository = concertEventRepository;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine("Seat reservation consumer is ready.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeOffset.UtcNow;
            var concertEvents = await _concertEventRepository.GetAllAsync(stoppingToken);

            foreach (var concertEvent in concertEvents)
            {
                var releasedAny = false;
                var lockedSeatIds = concertEvent.Seats
                    .Where(seat => seat.Status == SeatStatus.TemporarilyLocked && seat.LockedUntilUtc <= now)
                    .Select(seat => seat.Id)
                    .ToArray();

                foreach (var seatId in lockedSeatIds)
                {
                    releasedAny = concertEvent.ReleaseExpiredHold(seatId, now) || releasedAny;
                }

                if (releasedAny)
                {
                    await _concertEventRepository.UpdateAsync(concertEvent, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
