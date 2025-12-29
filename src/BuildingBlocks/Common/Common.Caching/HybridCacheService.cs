using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Common.Caching;

/// <summary>
/// Hybrid cache service with L1 (Memory) and L2 (Redis) caching
/// </summary>
public class HybridCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<HybridCacheService> _logger;
    private readonly TimeSpan _defaultExpiration = TimeSpan.FromMinutes(10);

    public HybridCacheService(
        IMemoryCache memoryCache,
        IConnectionMultiplexer redis,
        ILogger<HybridCacheService> logger)
    {
        _memoryCache = memoryCache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Try L1 cache first
        if (_memoryCache.TryGetValue(key, out T? cachedValue))
        {
            _logger.LogDebug("Cache hit (L1) for key: {Key}", key);
            return cachedValue;
        }

        // Try L2 cache (Redis)
        try
        {
            var db = _redis.GetDatabase();
            var redisValue = await db.StringGetAsync(key);

            if (redisValue.HasValue)
            {
                _logger.LogDebug("Cache hit (L2) for key: {Key}", key);
                var value = JsonSerializer.Deserialize<T>(redisValue.ToString());

                // Store in L1 cache
                if (value != null)
                {
                    _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));
                }

                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading from Redis cache for key: {Key}", key);
        }

        _logger.LogDebug("Cache miss for key: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        var expirationTime = expiration ?? _defaultExpiration;

        // Set in L1 cache
        _memoryCache.Set(key, value, expirationTime);

        // Set in L2 cache (Redis)
        try
        {
            var db = _redis.GetDatabase();
            var serializedValue = JsonSerializer.Serialize(value);
            await db.StringSetAsync(key, serializedValue, expirationTime);
            
            _logger.LogDebug("Cache set for key: {Key} with expiration: {Expiration}", 
                key, expirationTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing to Redis cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        // Remove from L1 cache
        _memoryCache.Remove(key);

        // Remove from L2 cache (Redis)
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(key);
            
            _logger.LogDebug("Cache removed for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from Redis cache for key: {Key}", key);
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, 
        TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);
        
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var value = await factory();
        
        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        // Check L1 cache
        if (_memoryCache.TryGetValue(key, out _))
        {
            return true;
        }

        // Check L2 cache (Redis)
        try
        {
            var db = _redis.GetDatabase();
            return await db.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence in Redis cache for key: {Key}", key);
            return false;
        }
    }
}
