using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace dotnet_rest_api.Services;

/// <summary>
/// Interface for caching service with both memory and distributed caching capabilities
/// </summary>
public interface ICacheService
{
    // Memory cache methods (for frequently accessed, small data)
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
    
    // Distributed cache methods (for larger data, multi-instance scenarios)
    Task<T?> GetDistributedAsync<T>(string key) where T : class;
    Task SetDistributedAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveDistributedAsync(string key);
    
    // Cache warming and invalidation
    Task InvalidateUserDataAsync(Guid userId);
    Task InvalidateProjectDataAsync(Guid projectId);
    Task InvalidateUserRoleDataAsync();
    Task InvalidateByPatternAsync(string pattern);
    Task WarmUpCacheAsync();
}

/// <summary>
/// Cache service implementation with memory and distributed caching
/// </summary>
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    
    // Cache key prefixes for organization
    private const string USER_PREFIX = "user:";
    private const string PROJECT_PREFIX = "project:";
    private const string TASK_PREFIX = "task:";
    private const string ROLE_PREFIX = "role:";
    private const string STATS_PREFIX = "stats:";
    
    // Default cache durations
    private static readonly TimeSpan DefaultMemoryCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan DefaultDistributedCacheDuration = TimeSpan.FromHours(1);
    private static readonly TimeSpan RoleCacheDuration = TimeSpan.FromHours(24); // Roles change rarely
    private static readonly TimeSpan UserCacheDuration = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan ProjectCacheDuration = TimeSpan.FromMinutes(20);
    private static readonly TimeSpan TaskCacheDuration = TimeSpan.FromMinutes(10);
    
    public CacheService(
        IMemoryCache memoryCache, 
        IDistributedCache distributedCache, 
        ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    #region Memory Cache Methods

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            if (_memoryCache.TryGetValue(key, out var cachedValue))
            {
                _logger.LogDebug("Cache hit for key: {Key}", key);
                return cachedValue as T;
            }
            
            _logger.LogDebug("Cache miss for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from memory cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? GetDefaultCacheDuration(key),
                SlidingExpiration = TimeSpan.FromMinutes(5), // Reset expiration on access
                Priority = GetCachePriority(key)
            };

            _memoryCache.Set(key, value, options);
            _logger.LogDebug("Set memory cache for key: {Key}, expires in: {Expiration}", 
                key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting memory cache for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);
            _logger.LogDebug("Removed from memory cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from memory cache for key: {Key}", key);
        }
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            // Memory cache doesn't support pattern removal directly
            // This would require keeping track of keys, which is complex
            // For now, we'll log this as a limitation
            _logger.LogWarning("Pattern-based removal not implemented for memory cache: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pattern removal for memory cache: {Pattern}", pattern);
        }
    }

    #endregion

    #region Distributed Cache Methods

    public async Task<T?> GetDistributedAsync<T>(string key) where T : class
    {
        try
        {
            var cachedData = await _distributedCache.GetStringAsync(key);
            if (cachedData != null)
            {
                _logger.LogDebug("Distributed cache hit for key: {Key}", key);
                return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
            }
            
            _logger.LogDebug("Distributed cache miss for key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from distributed cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetDistributedAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, _jsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? GetDefaultDistributedCacheDuration(key),
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            await _distributedCache.SetStringAsync(key, json, options);
            _logger.LogDebug("Set distributed cache for key: {Key}, expires in: {Expiration}", 
                key, options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting distributed cache for key: {Key}", key);
        }
    }

    public async Task RemoveDistributedAsync(string key)
    {
        try
        {
            await _distributedCache.RemoveAsync(key);
            _logger.LogDebug("Removed from distributed cache: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing from distributed cache for key: {Key}", key);
        }
    }

    #endregion

    #region Cache Invalidation Methods

    public async Task InvalidateUserDataAsync(Guid userId)
    {
        try
        {
            var keys = new[]
            {
                $"{USER_PREFIX}{userId}",
                $"{USER_PREFIX}{userId}:profile",
                $"{USER_PREFIX}{userId}:projects",
                $"{USER_PREFIX}{userId}:tasks",
                $"{STATS_PREFIX}user:{userId}"
            };

            var tasks = keys.Select(async key =>
            {
                await RemoveAsync(key);
                await RemoveDistributedAsync(key);
            });

            await Task.WhenAll(tasks);
            _logger.LogInformation("Invalidated cache for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating user cache for: {UserId}", userId);
        }
    }

    public async Task InvalidateProjectDataAsync(Guid projectId)
    {
        try
        {
            var keys = new[]
            {
                $"{PROJECT_PREFIX}{projectId}",
                $"{PROJECT_PREFIX}{projectId}:details",
                $"{PROJECT_PREFIX}{projectId}:tasks",
                $"{PROJECT_PREFIX}{projectId}:images",
                $"{STATS_PREFIX}project:{projectId}",
                "projects:list", // Invalidate project lists
                "projects:stats"
            };

            var tasks = keys.Select(async key =>
            {
                await RemoveAsync(key);
                await RemoveDistributedAsync(key);
            });

            await Task.WhenAll(tasks);
            _logger.LogInformation("Invalidated cache for project: {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating project cache for: {ProjectId}", projectId);
        }
    }

    public async Task InvalidateUserRoleDataAsync()
    {
        try
        {
            var keys = new[]
            {
                $"{ROLE_PREFIX}all",
                $"{ROLE_PREFIX}list",
                "users:by-role",
                "roles:permissions"
            };

            var tasks = keys.Select(async key =>
            {
                await RemoveAsync(key);
                await RemoveDistributedAsync(key);
            });

            await Task.WhenAll(tasks);
            _logger.LogInformation("Invalidated role cache data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating role cache data");
        }
    }

    public async Task InvalidateByPatternAsync(string pattern)
    {
        try
        {
            _logger.LogDebug("Invalidating cache entries matching pattern: {Pattern}", pattern);
            
            // For memory cache, we need to track keys or use reflection to get them
            // This is a simplified implementation - in production you might want to maintain a key registry
            await RemoveByPatternAsync(pattern);
            
            _logger.LogInformation("Invalidated cache entries matching pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task WarmUpCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting cache warm-up process");
            
            // This method would be called during application startup
            // to pre-populate frequently accessed data
            
            // Example: Pre-load roles, project statuses, etc.
            // Implementation would depend on having access to data services
            
            _logger.LogInformation("Cache warm-up completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warm-up");
        }
    }

    #endregion

    #region Private Helper Methods

    private TimeSpan GetDefaultCacheDuration(string key)
    {
        return key switch
        {
            var k when k.StartsWith(ROLE_PREFIX) => RoleCacheDuration,
            var k when k.StartsWith(USER_PREFIX) => UserCacheDuration,
            var k when k.StartsWith(PROJECT_PREFIX) => ProjectCacheDuration,
            var k when k.StartsWith(TASK_PREFIX) => TaskCacheDuration,
            _ => DefaultMemoryCacheDuration
        };
    }

    private TimeSpan GetDefaultDistributedCacheDuration(string key)
    {
        return key switch
        {
            var k when k.StartsWith(ROLE_PREFIX) => TimeSpan.FromHours(48),
            var k when k.StartsWith(USER_PREFIX) => TimeSpan.FromHours(2),
            var k when k.StartsWith(PROJECT_PREFIX) => TimeSpan.FromHours(1),
            var k when k.StartsWith(TASK_PREFIX) => TimeSpan.FromMinutes(30),
            _ => DefaultDistributedCacheDuration
        };
    }

    private CacheItemPriority GetCachePriority(string key)
    {
        return key switch
        {
            var k when k.StartsWith(ROLE_PREFIX) => CacheItemPriority.High,
            var k when k.StartsWith(USER_PREFIX) => CacheItemPriority.Normal,
            var k when k.StartsWith(PROJECT_PREFIX) => CacheItemPriority.Normal,
            var k when k.StartsWith(TASK_PREFIX) => CacheItemPriority.Low,
            _ => CacheItemPriority.Normal
        };
    }

    #endregion
}

/// <summary>
/// Cache configuration options
/// </summary>
public class CacheOptions
{
    public bool EnableMemoryCache { get; set; } = true;
    public bool EnableDistributedCache { get; set; } = true;
    public int MemoryCacheSizeLimitMB { get; set; } = 100;
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(15);
    public bool EnableCacheStatistics { get; set; } = true;
}
