namespace TicketingEngine.Domain.Entities;

public class ConcertEvent
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = string.Empty;
    public DateTimeOffset StartsAt { get; private set; }

    public ConcertEvent(string name, DateTimeOffset startsAt)
    {
        Name = name;
        StartsAt = startsAt;
    }
}
