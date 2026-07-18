namespace TicketingEngine.Infrastructure.Persistence;

public sealed class MongoDbOptions
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "ticketing_engine";
}
