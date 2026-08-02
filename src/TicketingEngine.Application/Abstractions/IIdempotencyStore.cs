namespace TicketingEngine.Application.Abstractions;

public interface IIdempotencyStore<TResult>
{
    Task<(bool Found, TResult? Result)> TryGetAsync(string key, CancellationToken cancellationToken = default);
    Task StoreAsync(string key, TResult result, CancellationToken cancellationToken = default);
}
