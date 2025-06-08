using System.Collections.Concurrent;
using System.Net;

namespace dotnet_rest_api.Middleware;

/// <summary>
/// Configuration options for rate limiting
/// </summary>
public class RateLimitOptions
{
    public Dictionary<string, RateLimitRule> Rules { get; set; } = new();
    public string DefaultRule { get; set; } = "default";
    public bool EnableIpWhitelist { get; set; } = true;
    public List<string> IpWhitelist { get; set; } = new();
    public string ClientIdHeader { get; set; } = "X-Client-Id";
    public bool EnableDistributedCache { get; set; } = false;
    public string RedisConnectionString { get; set; } = string.Empty;
}

/// <summary>
/// Rate limiting rule configuration
/// </summary>
public class RateLimitRule
{
    public int Limit { get; set; } = 100;
    public TimeSpan Period { get; set; } = TimeSpan.FromMinutes(1);
    public List<string> Endpoints { get; set; } = new();
    public List<string> HttpMethods { get; set; } = new();
    public bool ApplyPerEndpoint { get; set; } = false;
}

/// <summary>
/// Client rate limit information
/// </summary>
public class ClientRateLimit
{
    public string ClientId { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public DateTime WindowStart { get; set; }
    public DateTime LastRequest { get; set; }
    public string Rule { get; set; } = string.Empty;
}

/// <summary>
/// Rate limit result information
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
    public TimeSpan RetryAfter { get; set; }
    public string Rule { get; set; } = string.Empty;
}

/// <summary>
/// Interface for rate limit storage
/// </summary>
public interface IRateLimitStorage
{
    Task<ClientRateLimit?> GetClientRateLimit(string clientId, string rule);
    Task SetClientRateLimit(string clientId, string rule, ClientRateLimit rateLimit);
    Task IncrementClientRateLimit(string clientId, string rule);
    Task RemoveExpiredEntries();
}

/// <summary>
/// In-memory implementation of rate limit storage
/// </summary>
public class InMemoryRateLimitStorage : IRateLimitStorage
{
    private readonly ConcurrentDictionary<string, ClientRateLimit> _storage = new();
    private readonly Timer _cleanupTimer;

    public InMemoryRateLimitStorage()
    {
        // Clean up expired entries every 5 minutes
        _cleanupTimer = new Timer(async _ => await RemoveExpiredEntries(), null, 
            TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<ClientRateLimit?> GetClientRateLimit(string clientId, string rule)
    {
        var key = $"{clientId}:{rule}";
        _storage.TryGetValue(key, out var rateLimit);
        return Task.FromResult(rateLimit);
    }

    public Task SetClientRateLimit(string clientId, string rule, ClientRateLimit rateLimit)
    {
        var key = $"{clientId}:{rule}";
        _storage[key] = rateLimit;
        return Task.CompletedTask;
    }

    public Task IncrementClientRateLimit(string clientId, string rule)
    {
        var key = $"{clientId}:{rule}";
        _storage.AddOrUpdate(key, 
            new ClientRateLimit
            {
                ClientId = clientId,
                RequestCount = 1,
                WindowStart = DateTime.UtcNow,
                LastRequest = DateTime.UtcNow,
                Rule = rule
            },
            (k, existing) =>
            {
                existing.RequestCount++;
                existing.LastRequest = DateTime.UtcNow;
                return existing;
            });
        return Task.CompletedTask;
    }

    public Task RemoveExpiredEntries()
    {
        var expiredKeys = _storage
            .Where(kvp => DateTime.UtcNow - kvp.Value.WindowStart > TimeSpan.FromHours(1))
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _storage.TryRemove(key, out _);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
}

/// <summary>
/// Rate limiting service
/// </summary>
public interface IRateLimitService
{
    Task<RateLimitResult> CheckRateLimit(string clientId, string endpoint, string method);
    string GetClientIdentifier(HttpContext context);
    string GetApplicableRule(string endpoint, string method);
}

/// <summary>
/// Implementation of rate limiting service
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly RateLimitOptions _options;
    private readonly IRateLimitStorage _storage;
    private readonly ILogger<RateLimitService> _logger;

    public RateLimitService(RateLimitOptions options, IRateLimitStorage storage, ILogger<RateLimitService> logger)
    {
        _options = options;
        _storage = storage;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckRateLimit(string clientId, string endpoint, string method)
    {
        var rule = GetApplicableRule(endpoint, method);
        var ruleConfig = _options.Rules.GetValueOrDefault(rule) ?? _options.Rules[_options.DefaultRule];

        var clientRateLimit = await _storage.GetClientRateLimit(clientId, rule);
        var now = DateTime.UtcNow;

        // Initialize or reset window if needed
        if (clientRateLimit == null || now - clientRateLimit.WindowStart >= ruleConfig.Period)
        {
            clientRateLimit = new ClientRateLimit
            {
                ClientId = clientId,
                RequestCount = 1,
                WindowStart = now,
                LastRequest = now,
                Rule = rule
            };
            await _storage.SetClientRateLimit(clientId, rule, clientRateLimit);

            return new RateLimitResult
            {
                IsAllowed = true,
                Limit = ruleConfig.Limit,
                Remaining = ruleConfig.Limit - 1,
                ResetTime = clientRateLimit.WindowStart.Add(ruleConfig.Period),
                Rule = rule
            };
        }

        // Check if limit exceeded
        if (clientRateLimit.RequestCount >= ruleConfig.Limit)
        {
            var resetTime = clientRateLimit.WindowStart.Add(ruleConfig.Period);
            var retryAfter = resetTime - now;

            return new RateLimitResult
            {
                IsAllowed = false,
                Limit = ruleConfig.Limit,
                Remaining = 0,
                ResetTime = resetTime,
                RetryAfter = retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero,
                Rule = rule
            };
        }

        // Increment and allow
        await _storage.IncrementClientRateLimit(clientId, rule);
        
        return new RateLimitResult
        {
            IsAllowed = true,
            Limit = ruleConfig.Limit,
            Remaining = ruleConfig.Limit - (clientRateLimit.RequestCount + 1),
            ResetTime = clientRateLimit.WindowStart.Add(ruleConfig.Period),
            Rule = rule
        };
    }

    public string GetClientIdentifier(HttpContext context)
    {
        // Check for custom client ID header
        if (context.Request.Headers.TryGetValue(_options.ClientIdHeader, out var clientIdHeader))
        {
            var clientId = clientIdHeader.FirstOrDefault();
            if (!string.IsNullOrEmpty(clientId))
            {
                return $"client:{clientId}";
            }
        }

        // Check for authenticated user
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst("sub")?.Value ?? 
                        context.User.FindFirst("id")?.Value ?? 
                        context.User.Identity.Name;
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }
        }

        // Fall back to IP address
        var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        
        // Handle forwarded headers for reverse proxies
        if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
        {
            var firstIp = forwardedFor.FirstOrDefault()?.Split(',').FirstOrDefault()?.Trim();
            if (!string.IsNullOrEmpty(firstIp) && IPAddress.TryParse(firstIp, out _))
            {
                remoteIp = firstIp;
            }
        }
        else if (context.Request.Headers.TryGetValue("X-Real-IP", out var realIp))
        {
            var ip = realIp.FirstOrDefault();
            if (!string.IsNullOrEmpty(ip) && IPAddress.TryParse(ip, out _))
            {
                remoteIp = ip;
            }
        }

        return $"ip:{remoteIp}";
    }

    public string GetApplicableRule(string endpoint, string method)
    {
        // Check for specific endpoint rules
        foreach (var rule in _options.Rules)
        {
            var ruleConfig = rule.Value;
            
            // Check if this rule applies to the endpoint
            if (ruleConfig.Endpoints.Any() && !ruleConfig.Endpoints.Any(e => 
                endpoint.StartsWith(e, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            // Check if this rule applies to the HTTP method
            if (ruleConfig.HttpMethods.Any() && !ruleConfig.HttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            return rule.Key;
        }

        return _options.DefaultRule;
    }
}
