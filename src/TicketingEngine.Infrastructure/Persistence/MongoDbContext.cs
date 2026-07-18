using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TicketingEngine.Infrastructure.Persistence;

public interface IMongoDbContext
{
    IMongoCollection<TDocument> GetCollection<TDocument>(string name);
}

public sealed class MongoDbContext : IMongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbOptions> options)
    {
        var mongoOptions = options.Value;
        var mongoClient = new MongoClient(mongoOptions.ConnectionString);
        _database = mongoClient.GetDatabase(mongoOptions.DatabaseName);
    }

    public IMongoCollection<TDocument> GetCollection<TDocument>(string name)
    {
        return _database.GetCollection<TDocument>(name);
    }
}
