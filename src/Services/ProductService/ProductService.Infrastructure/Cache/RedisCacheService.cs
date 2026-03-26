using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using ProductService.Application.Interfaces;
using StackExchange.Redis;

namespace ProductService.Infrastructure.Cache;

/// <summary>
/// Redis tabanlı cache servisi.
/// OCP: Yeni cache stratejisi eklemek için ICacheService yeni bir implementasyonla genişletilir.
/// Circuit Breaker pattern: Redis erişilemez olduğunda hata fırlatmak yerine
/// loglayıp devam eder (graceful degradation).
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public RedisCacheService(
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var bytes = await _cache.GetAsync(key, ct);
            if (bytes is null or { Length: 0 }) return default;
            return JsonSerializer.Deserialize<T>(bytes, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis GET başarısız. Key: {Key}", key);
            return default; // Cache miss — veritabanına düşer
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var bytes = JsonSerializer.SerializeToUtf8Bytes(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(10)
            };
            await _cache.SetAsync(key, bytes, options, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis SET başarısız. Key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await _cache.RemoveAsync(key, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Redis REMOVE başarısız. Key: {Key}", key); }
    }

    /// <summary>
    /// Prefix ile toplu cache temizleme — Cache Invalidation stratejisi.
    /// Örn: "products:" prefix'i tüm sayfalanmış liste cache'lerini temizler.
    /// </summary>
    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();

            if (keys.Length == 0) return;

            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync(keys);

            _logger.LogInformation(
                "Cache invalidation: {Count} key temizlendi. Prefix: {Prefix}",
                keys.Length, prefix);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis prefix REMOVE başarısız. Prefix: {Prefix}", prefix);
        }
    }
}
