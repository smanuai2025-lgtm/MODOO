using System.Text.Json;
using AiModoo.Core.Interfaces.Common;
using StackExchange.Redis;

namespace AiModoo.Infrastructure.Services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _db;
    private readonly bool _isAvailable;

    public CacheService(IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
        _isAvailable = redis?.IsConnected ?? false;
        _db = _isAvailable ? redis!.GetDatabase() : null;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        if (!_isAvailable || _db == null) return default;

        var value = await _db.StringGetAsync(key);
        if (value.IsNullOrEmpty) return default;

        return JsonSerializer.Deserialize<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        if (!_isAvailable || _db == null) return;

        var serialized = JsonSerializer.Serialize(value);
        await _db.StringSetAsync(key, serialized, expiry ?? TimeSpan.FromMinutes(30));
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        if (!_isAvailable || _db == null) return;

        await _db.KeyDeleteAsync(key);
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        if (!_isAvailable || _redis == null) return;

        var endpoints = _redis.GetEndPoints();
        var server = _redis.GetServer(endpoints[0]);
        var keys = server.Keys(pattern: $"{prefix}*");

        foreach (var key in keys)
        {
            await _db!.KeyDeleteAsync(key);
        }
    }
}
