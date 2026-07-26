using Microsoft.Extensions.Options;
using Mongo2Go;
using TicketingEngine.Domain.Entities;
using TicketingEngine.Infrastructure.Persistence;
using Xunit;

namespace TicketingEngine.Tests;

public sealed class MongoRunnerFixture : IDisposable
{
    public MongoDbRunner Runner { get; } = MongoDbRunner.Start(singleNodeReplSet: true);

    public void Dispose() => Runner.Dispose();
}

[CollectionDefinition(nameof(MongoRunnerCollection))]
public sealed class MongoRunnerCollection : ICollectionFixture<MongoRunnerFixture>
{
}

[Collection(nameof(MongoRunnerCollection))]
public class MongoConcertEventRepositoryTests
{
    private readonly MongoConcertEventRepository _repository;

    public MongoConcertEventRepositoryTests(MongoRunnerFixture fixture)
    {
        var options = Options.Create(new MongoDbOptions
        {
            ConnectionString = fixture.Runner.ConnectionString,
            DatabaseName = $"ticketing_engine_tests_{Guid.NewGuid():N}"
        });

        _repository = new MongoConcertEventRepository(new MongoDbContext(options));
    }

    [Fact]
    public async Task SaveAsync_ThenGetByIdAsync_ShouldRoundTripSeatState()
    {
        var eventId = Guid.NewGuid();
        var concertEvent = new ConcertEvent(eventId, "Save Test", DateTimeOffset.UtcNow.AddDays(30));
        var seat = new Seat("A", 1, 100m);
        concertEvent.AddSeat(seat);
        concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5));

        await _repository.SaveAsync(concertEvent);
        var loaded = await _repository.GetByIdAsync(eventId);

        Assert.NotNull(loaded);
        Assert.Equal("Save Test", loaded!.Name);
        var loadedSeat = Assert.Single(loaded.Seats);
        Assert.Equal(SeatStatus.TemporarilyLocked, loadedSeat.Status);
        Assert.NotNull(loadedSeat.LockedUntilUtc);
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistExpiredReleaseTransition()
    {
        var eventId = Guid.NewGuid();
        var concertEvent = new ConcertEvent(eventId, "Release Test", DateTimeOffset.UtcNow.AddDays(30));
        var seat = new Seat("B", 2, 100m);
        concertEvent.AddSeat(seat);
        concertEvent.LockSeat(seat.Id, TimeSpan.FromMinutes(5));
        await _repository.SaveAsync(concertEvent);

        var loaded = await _repository.GetByIdAsync(eventId);
        var released = loaded!.ReleaseExpiredHold(seat.Id, DateTimeOffset.UtcNow.AddMinutes(6));
        await _repository.UpdateAsync(loaded);

        var afterRelease = await _repository.GetByIdAsync(eventId);
        var releasedSeat = Assert.Single(afterRelease!.Seats);

        Assert.True(released);
        Assert.Equal(SeatStatus.Available, releasedSeat.Status);
        Assert.Null(releasedSeat.LockedUntilUtc);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEveryPersistedEvent()
    {
        var firstEventId = Guid.NewGuid();
        var secondEventId = Guid.NewGuid();
        await _repository.SaveAsync(new ConcertEvent(firstEventId, "Event One", DateTimeOffset.UtcNow.AddDays(10)));
        await _repository.SaveAsync(new ConcertEvent(secondEventId, "Event Two", DateTimeOffset.UtcNow.AddDays(20)));

        var events = await _repository.GetAllAsync();

        Assert.Equal(2, events.Count);
        Assert.Contains(events, concertEvent => concertEvent.Id == firstEventId);
        Assert.Contains(events, concertEvent => concertEvent.Id == secondEventId);
    }

    [Fact]
    public async Task UpdateAsync_SilentlyDropsAnEarlierChange_WhenCallerReconcilesAgainstTheLatestVersionInstead()
    {
        // Characterization test: MongoConcertEventRepository.UpdateAsync re-reads the document's CURRENT
        // version at write time instead of comparing against the version the caller originally loaded.
        // It only throws when two writers read the same version at nearly the same instant; a writer that
        // reads later (after another writer already committed) is allowed to replace the whole document
        // with its own stale snapshot, silently discarding the intervening change. This test documents
        // that known gap rather than asserting the (incorrect) behavior described in the analysis.
        var eventId = Guid.NewGuid();
        await _repository.SaveAsync(new ConcertEvent(eventId, "Conflict Test", DateTimeOffset.UtcNow.AddDays(30)));

        var readerA = await _repository.GetByIdAsync(eventId);
        var readerB = await _repository.GetByIdAsync(eventId);

        readerA!.AddSeat(new Seat("A", 1, 100m));
        await _repository.UpdateAsync(readerA);

        readerB!.AddSeat(new Seat("B", 2, 100m));
        await _repository.UpdateAsync(readerB);

        var finalState = await _repository.GetByIdAsync(eventId);
        var remainingSeat = Assert.Single(finalState!.Seats);
        Assert.Equal("B", remainingSeat.Row);
    }
}
