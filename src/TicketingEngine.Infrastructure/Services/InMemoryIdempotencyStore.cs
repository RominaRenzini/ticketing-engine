using System.Collections.Concurrent;
using TicketingEngine.Application.Abstractions;

namespace TicketingEngine.Infrastructure.Services;

public sealed class InMemoryIdempotencyStore<TResult> : IIdempotencyStore<TResult>
{
    private readonly ConcurrentDictionary<string, TResult> _store = new(StringComparer.Ordinal);

    public Task<(bool Found, TResult? Result)> TryGetAsync(string key, CancellationToken cancellationToken = default)
    {
        var found = _store.TryGetValue(key, out var result);
        return Task.FromResult((found, result));
    }

    public Task StoreAsync(string key, TResult result, CancellationToken cancellationToken = default)
    {
        _store.TryAdd(key, result);
        return Task.CompletedTask;
    }
}
