using StackExchange.Redis;
using System.Text.Json;

namespace dotnet_rest_api.Middleware;

/// <summary>
/// Redis-based rate limiting storage for distributed applications
/// </summary>
public class RedisRateLimitStorage : IRateLimitStorage
{
    private readonly IDatabase _database;
    private readonly ILogger<RedisRateLimitStorage> _logger;
    private readonly string _keyPrefix;

    public RedisRateLimitStorage(IConnectionMultiplexer redis, ILogger<RedisRateLimitStorage> logger)
    {
        _database = redis.GetDatabase();
        _logger = logger;
        _keyPrefix = "ratelimit:";
    }

    public async Task<ClientRateLimit?> GetClientRateLimit(string clientId, string rule)
    {
        try
        {
            var key = GetKey(clientId, rule);
            var data = await _database.StringGetAsync(key);
            
            if (!data.HasValue)
            {
                return null;
            }

            return JsonSerializer.Deserialize<ClientRateLimit>(data.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get rate limit data for client {ClientId}, rule {Rule}", clientId, rule);
            
            // Return null on error to allow request to proceed
            return null;
        }
    }

    public async Task SetClientRateLimit(string clientId, string rule, ClientRateLimit rateLimit)
    {
        try
        {
            var key = GetKey(clientId, rule);
            var json = JsonSerializer.Serialize(rateLimit);
            
            // Set with expiration of 2 hours to prevent memory leaks
            await _database.StringSetAsync(key, json, TimeSpan.FromHours(2));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set rate limit data for client {ClientId}, rule {Rule}", clientId, rule);
            // Don't throw - rate limiting should fail open
        }
    }

    public async Task IncrementClientRateLimit(string clientId, string rule)
    {
        try
        {
            var key = GetKey(clientId, rule);
            
            // Try to get existing rate limit
            var existing = await GetClientRateLimit(clientId, rule);
            
            if (existing == null)
            {
                // Create new rate limit entry
                existing = new ClientRateLimit
                {
                    ClientId = clientId,
                    RequestCount = 1,
                    WindowStart = DateTime.UtcNow,
                    LastRequest = DateTime.UtcNow,
                    Rule = rule
                };
            }
            else
            {
                // Update existing entry
                existing.RequestCount++;
                existing.LastRequest = DateTime.UtcNow;
            }

            await SetClientRateLimit(clientId, rule, existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to increment rate limit for client {ClientId}, rule {Rule}", clientId, rule);
            // Don't throw - rate limiting should fail open
        }
    }

    public async Task RemoveExpiredEntries()
    {
        try
        {
            var server = _database.Multiplexer.GetServer(_database.Multiplexer.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_keyPrefix}*");
            
            var expiredKeys = new List<RedisKey>();
            var cutoffTime = DateTime.UtcNow.AddHours(-2); // Remove entries older than 2 hours
            
            foreach (var key in keys)
            {
                try
                {
                    var data = await _database.StringGetAsync(key);
                    if (data.HasValue)
                    {
                        var rateLimit = JsonSerializer.Deserialize<ClientRateLimit>(data.ToString());
                        if (rateLimit != null && rateLimit.LastRequest < cutoffTime)
                        {
                            expiredKeys.Add(key);
                        }
                    }
                }
                catch
                {
                    // If we can't deserialize, consider it expired
                    expiredKeys.Add(key);
                }
            }
            
            if (expiredKeys.Count > 0)
            {
                await _database.KeyDeleteAsync(expiredKeys.ToArray());
                _logger.LogDebug("Removed {Count} expired rate limit entries", expiredKeys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove expired rate limit entries");
        }
    }

    private string GetKey(string clientId, string rule)
    {
        return $"{_keyPrefix}{clientId}:{rule}";
    }
}

/// <summary>
/// Extension methods for Redis rate limiting configuration
/// </summary>
public static class RedisRateLimitExtensions
{
    /// <summary>
    /// Adds Redis-based rate limiting storage
    /// </summary>
    public static IServiceCollection AddRedisRateLimit(this IServiceCollection services, string connectionString)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
        });
        
        services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(connectionString));
            
        services.AddSingleton<IRateLimitStorage, RedisRateLimitStorage>();
        
        return services;
    }

    /// <summary>
    /// Adds Redis-based rate limiting storage with configuration
    /// </summary>
    public static IServiceCollection AddRedisRateLimit(this IServiceCollection services, Action<ConfigurationOptions> configure)
    {
        var options = new ConfigurationOptions();
        configure(options);
        
        services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect(options));
            
        services.AddSingleton<IRateLimitStorage, RedisRateLimitStorage>();
        
        return services;
    }
}
