using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public interface ICacheService
{
    T? Get<T>(string key);
    Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null);
    void Set<T>(string key, T value, TimeSpan? expiration = null);
    void Remove(string key);
    void RemoveByPattern(string pattern);
    void Clear();
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly HashSet<string> _cacheKeys = new();
    private readonly object _lock = new();

    public CacheService(IMemoryCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public T? Get<T>(string key)
    {
        if (_cache.TryGetValue(key, out T? value))
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            return value;
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return default;
    }

    public async Task<T?> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null)
    {
        if (_cache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            return cachedValue;
        }

        _logger.LogDebug("Cache miss: {Key}, creating value", key);

        var value = await factory();

        if (value != null)
        {
            Set(key, value, expiration);
        }

        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30),
            SlidingExpiration = TimeSpan.FromMinutes(10)
        };

        _cache.Set(key, value, cacheOptions);

        lock (_lock)
        {
            _cacheKeys.Add(key);
        }

        _logger.LogDebug("Cache set: {Key}, Expiration: {Expiration}", key, expiration ?? TimeSpan.FromMinutes(30));
    }

    public void Remove(string key)
    {
        _cache.Remove(key);

        lock (_lock)
        {
            _cacheKeys.Remove(key);
        }

        _logger.LogInformation("Cache removed: {Key}", key);
    }

    public void RemoveByPattern(string pattern)
    {
        List<string> keysToRemove;

        lock (_lock)
        {
            keysToRemove = _cacheKeys
                .Where(k => k.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        foreach (var key in keysToRemove)
        {
            Remove(key);
        }

        _logger.LogInformation("Cache removed by pattern: {Pattern}, Count: {Count}", pattern, keysToRemove.Count);
    }

    public void Clear()
    {
        List<string> allKeys;

        lock (_lock)
        {
            allKeys = _cacheKeys.ToList();
            _cacheKeys.Clear();
        }

        foreach (var key in allKeys)
        {
            _cache.Remove(key);
        }

        _logger.LogWarning("Cache cleared: {Count} entries removed", allKeys.Count);
    }
}
