using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using TicketingEngine.Application.Abstractions;
using TicketingEngine.Domain.Entities;

namespace TicketingEngine.Infrastructure.Persistence;

public sealed class MongoConcertEventRepository : IConcertEventRepository
{
    private const string ConcertEventsCollectionName = "concert_events";
    private const string ReservationsCollectionName = "reservations";
    private readonly IMongoCollection<ConcertEventDocument> _concertEventsCollection;
    private readonly IMongoCollection<ReservationDocument> _reservationsCollection;

    public MongoConcertEventRepository(IMongoDbContext mongoDbContext)
    {
        _concertEventsCollection = mongoDbContext.GetCollection<ConcertEventDocument>(ConcertEventsCollectionName);
        _reservationsCollection = mongoDbContext.GetCollection<ReservationDocument>(ReservationsCollectionName);
    }

    public async Task<ConcertEvent?> GetByIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var document = await _concertEventsCollection
            .Find(concertEvent => concertEvent.Id == eventId)
            .SingleOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDomain(document);
    }

    public async Task<IReadOnlyCollection<ConcertEvent>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var documents = await _concertEventsCollection
            .Find(FilterDefinition<ConcertEventDocument>.Empty)
            .ToListAsync(cancellationToken);

        return documents.Select(ToDomain).ToArray();
    }

    public async Task SaveAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(concertEvent);
        document.Version = 1;
        document.CreatedAtUtc = DateTime.UtcNow;
        document.UpdatedAtUtc = document.CreatedAtUtc;

        await _concertEventsCollection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(ConcertEvent concertEvent, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var nextDocument = ToDocument(concertEvent);
        var existing = await _concertEventsCollection
            .Find(concertEventDocument => concertEventDocument.Id == concertEvent.Id)
            .Project(concertEventDocument => new { concertEventDocument.Version, concertEventDocument.CreatedAtUtc })
            .SingleOrDefaultAsync(cancellationToken);

        if (existing is null)
        {
            nextDocument.Version = 1;
            nextDocument.CreatedAtUtc = now;
            nextDocument.UpdatedAtUtc = now;
            await _concertEventsCollection.InsertOneAsync(nextDocument, cancellationToken: cancellationToken);
            return;
        }

        nextDocument.Version = existing.Version + 1;
        nextDocument.CreatedAtUtc = existing.CreatedAtUtc;
        nextDocument.UpdatedAtUtc = now;

        var filter = Builders<ConcertEventDocument>.Filter.Where(document =>
            document.Id == nextDocument.Id && document.Version == existing.Version);

        var result = await _concertEventsCollection.ReplaceOneAsync(filter, nextDocument, cancellationToken: cancellationToken);
        if (result.ModifiedCount == 0)
        {
            throw new InvalidOperationException($"Concurrent update detected for event {concertEvent.Id}.");
        }
    }

    public async Task<Reservation?> GetReservationByIdempotencyKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        var document = await _reservationsCollection
            .Find(reservation => reservation.IdempotencyKey == idempotencyKey)
            .SingleOrDefaultAsync(cancellationToken);

        return document is null ? null : ToDomain(document);
    }

    public async Task SaveReservationAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(reservation);
        await _reservationsCollection.InsertOneAsync(document, cancellationToken: cancellationToken);
    }

    public async Task UpdateReservationAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        var document = ToDocument(reservation);
        var filter = Builders<ReservationDocument>.Filter.Where(item => item.Id == reservation.Id);
        await _reservationsCollection.ReplaceOneAsync(filter, document, cancellationToken: cancellationToken);
    }

    private static ConcertEventDocument ToDocument(ConcertEvent concertEvent)
    {
        return new ConcertEventDocument
        {
            Id = concertEvent.Id,
            Name = concertEvent.Name,
            StartsAtUtc = concertEvent.StartsAt.UtcDateTime,
            Seats = concertEvent.Seats
                .Select(seat => new SeatDocument
                {
                    Id = seat.Id,
                    Row = seat.Row,
                    Number = seat.Number,
                    Price = seat.Price,
                    Status = seat.Status,
                    LockedUntilUtc = seat.LockedUntilUtc?.UtcDateTime
                })
                .ToList()
        };
    }

    private static ConcertEvent ToDomain(ConcertEventDocument document)
    {
        var seats = document.Seats.Select(seatDocument =>
            Seat.Rehydrate(
                seatDocument.Id,
                seatDocument.Row,
                seatDocument.Number,
                seatDocument.Price,
                seatDocument.Status,
                seatDocument.LockedUntilUtc is null
                    ? null
                    : new DateTimeOffset(DateTime.SpecifyKind(seatDocument.LockedUntilUtc.Value, DateTimeKind.Utc))));

        return new ConcertEvent(
            document.Id,
            document.Name,
            new DateTimeOffset(DateTime.SpecifyKind(document.StartsAtUtc, DateTimeKind.Utc)),
            seats);
    }

    private static ReservationDocument ToDocument(Reservation reservation)
    {
        return new ReservationDocument
        {
            Id = reservation.Id,
            EventId = reservation.EventId,
            Status = reservation.Status,
            SeatSelections = reservation.SeatSelections.Select(selection => new SeatSelectionDocument
            {
                Row = selection.Row,
                Number = selection.Number
            }).ToList(),
            IdempotencyKey = reservation.IdempotencyKey
        };
    }

    private static Reservation ToDomain(ReservationDocument document)
    {
        return Reservation.Rehydrate(
            document.Id,
            document.EventId,
            document.Status,
            document.SeatSelections.Select(selection => new SeatSelection(selection.Row, selection.Number)).ToArray(),
            document.IdempotencyKey);
    }

    private sealed class ConcertEventDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
        public DateTime StartsAtUtc { get; set; }
        public List<SeatDocument> Seats { get; set; } = new();
        public long Version { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime UpdatedAtUtc { get; set; }
    }

    private sealed class SeatDocument
    {
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        public string Row { get; set; } = string.Empty;
        public int Number { get; set; }
        public decimal Price { get; set; }
        public SeatStatus Status { get; set; }
        public DateTime? LockedUntilUtc { get; set; }
    }

    private sealed class ReservationDocument
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }

        [BsonRepresentation(BsonType.String)]
        public Guid EventId { get; set; }

        public ReservationStatus Status { get; set; }
        public List<SeatSelectionDocument> SeatSelections { get; set; } = new();
        public string? IdempotencyKey { get; set; }
    }

    private sealed class SeatSelectionDocument
    {
        public string Row { get; set; } = string.Empty;
        public int Number { get; set; }
    }
}
