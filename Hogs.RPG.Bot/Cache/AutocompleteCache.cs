using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public static class AutocompleteCache<T>
{
    private record CacheEntry(T Value, DateTime Expiry);

    private static readonly Dictionary<ulong, CacheEntry> _cache = new();
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public static async Task<T> GetOrCreateAsync(
        ulong userId,
        TimeSpan duration,
        Func<Task<T>> factory)
    {
        await _lock.WaitAsync();
        try
        {
            if (_cache.TryGetValue(userId, out var entry) && DateTime.UtcNow < entry.Expiry)
                return entry.Value;

            var value = await factory();
            _cache[userId] = new CacheEntry(value, DateTime.UtcNow.Add(duration));
            return value;
        }
        finally
        {
            _lock.Release();
        }
    }

    public static void Invalidate(ulong userId)
    {
        _lock.Wait();
        try { _cache.Remove(userId); }
        finally { _lock.Release(); }
    }
}