using dotnet_rest_api.Middleware;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for monitoring and managing rate limits
/// </summary>
public interface IRateLimitMonitoringService
{
    Task<RateLimitStatistics> GetStatistics(TimeSpan? period = null);
    Task<List<ClientRateLimitInfo>> GetTopClients(int count = 10);
    Task<List<RateLimitViolation>> GetRecentViolations(TimeSpan? period = null);
    Task ClearClientLimits(string clientId);
    Task ClearAllLimits();
    Task UpdateRuleConfiguration(string ruleName, RateLimitRule rule);
    Task<Dictionary<string, RateLimitRule>> GetActiveRules();
    void RecordRequest(string clientId, string rule);
    void RecordViolation(RateLimitViolation violation);
    Task RecordRateLimitHit(string clientId, string rule, string endpoint, bool allowed);
}

/// <summary>
/// Rate limit statistics
/// </summary>
public class RateLimitStatistics
{
    public int TotalRequests { get; set; }
    public int BlockedRequests { get; set; }
    public int UniqueClients { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double BlockRate => TotalRequests > 0 ? (double)BlockedRequests / TotalRequests * 100 : 0;
    public Dictionary<string, int> RequestsByRule { get; set; } = new();
    public Dictionary<string, int> ViolationsByRule { get; set; } = new();
}

/// <summary>
/// Client rate limit information
/// </summary>
public class ClientRateLimitInfo
{
    public string ClientId { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public int ViolationCount { get; set; }
    public DateTime LastRequest { get; set; }
    public string MostUsedRule { get; set; } = string.Empty;
    public bool IsCurrentlyLimited { get; set; }
}

/// <summary>
/// Rate limit violation information
/// </summary>
public class RateLimitViolation
{
    public string ClientId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string RemoteIp { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of rate limit monitoring service
/// </summary>
public class RateLimitMonitoringService : IRateLimitMonitoringService
{
    private readonly IRateLimitStorage _storage;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitMonitoringService> _logger;
    private readonly List<RateLimitViolation> _violations = new();
    private readonly Dictionary<string, int> _requestCounts = new();
    private readonly object _lockObject = new();

    public RateLimitMonitoringService(
        IRateLimitStorage storage, 
        RateLimitOptions options, 
        ILogger<RateLimitMonitoringService> logger)
    {
        _storage = storage;
        _options = options;
        _logger = logger;
    }

    public Task<RateLimitStatistics> GetStatistics(TimeSpan? period = null)
    {
        var periodStart = DateTime.UtcNow - (period ?? TimeSpan.FromHours(1));
        var periodEnd = DateTime.UtcNow;

        lock (_lockObject)
        {
            var recentViolations = _violations.Where(v => v.Timestamp >= periodStart).ToList();
            
            var stats = new RateLimitStatistics
            {
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                BlockedRequests = recentViolations.Count,
                TotalRequests = _requestCounts.Values.Sum(),
                UniqueClients = _requestCounts.Keys.Count,
                ViolationsByRule = recentViolations
                    .GroupBy(v => v.Rule)
                    .ToDictionary(g => g.Key, g => g.Count()),
                RequestsByRule = _requestCounts.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
            };

            return Task.FromResult(stats);
        }
    }

    public Task<List<ClientRateLimitInfo>> GetTopClients(int count = 10)
    {
        lock (_lockObject)
        {
            var clientInfos = _requestCounts
                .OrderByDescending(kvp => kvp.Value)
                .Take(count)
                .Select(kvp => new ClientRateLimitInfo
                {
                    ClientId = kvp.Key,
                    RequestCount = kvp.Value,
                    ViolationCount = _violations.Count(v => v.ClientId == kvp.Key),
                    LastRequest = _violations
                        .Where(v => v.ClientId == kvp.Key)
                        .OrderByDescending(v => v.Timestamp)
                        .FirstOrDefault()?.Timestamp ?? DateTime.MinValue,
                    MostUsedRule = _violations
                        .Where(v => v.ClientId == kvp.Key)
                        .GroupBy(v => v.Rule)
                        .OrderByDescending(g => g.Count())
                        .FirstOrDefault()?.Key ?? "default"
                })
                .ToList();

            return Task.FromResult(clientInfos);
        }
    }

    public Task<List<RateLimitViolation>> GetRecentViolations(TimeSpan? period = null)
    {
        var since = DateTime.UtcNow - (period ?? TimeSpan.FromHours(1));
        
        lock (_lockObject)
        {
            var recentViolations = _violations
                .Where(v => v.Timestamp >= since)
                .OrderByDescending(v => v.Timestamp)
                .ToList();

            return Task.FromResult(recentViolations);
        }
    }

    public async Task ClearClientLimits(string clientId)
    {
        foreach (var rule in _options.Rules.Keys)
        {
            await _storage.SetClientRateLimit(clientId, rule, new ClientRateLimit
            {
                ClientId = clientId,
                RequestCount = 0,
                WindowStart = DateTime.UtcNow,
                LastRequest = DateTime.UtcNow,
                Rule = rule
            });
        }

        lock (_lockObject)
        {
            _requestCounts.Remove(clientId);
            _violations.RemoveAll(v => v.ClientId == clientId);
        }

        _logger.LogInformation("Cleared rate limits for client {ClientId}", clientId);
    }

    public async Task ClearAllLimits()
    {
        // Note: This is a simplified implementation for in-memory storage
        // For distributed storage, you'd need to implement a clear all method
        await _storage.RemoveExpiredEntries();

        lock (_lockObject)
        {
            _requestCounts.Clear();
            _violations.Clear();
        }

        _logger.LogInformation("Cleared all rate limits");
    }

    public Task UpdateRuleConfiguration(string ruleName, RateLimitRule rule)
    {
        _options.Rules[ruleName] = rule;
        _logger.LogInformation("Updated rate limit rule {RuleName}: {Limit} requests per {Period}", 
            ruleName, rule.Limit, rule.Period);
        return Task.CompletedTask;
    }

    public Task<Dictionary<string, RateLimitRule>> GetActiveRules()
    {
        return Task.FromResult(_options.Rules.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }

    public void RecordRequest(string clientId, string rule)
    {
        lock (_lockObject)
        {
            _requestCounts[clientId] = _requestCounts.GetValueOrDefault(clientId, 0) + 1;
        }
    }

    public void RecordViolation(RateLimitViolation violation)
    {
        lock (_lockObject)
        {
            _violations.Add(violation);
            
            // Keep only recent violations to prevent memory issues
            if (_violations.Count > 10000)
            {
                var cutoff = DateTime.UtcNow.AddHours(-24);
                _violations.RemoveAll(v => v.Timestamp < cutoff);
            }
        }

        _logger.LogWarning("Rate limit violation recorded for client {ClientId} on {Endpoint} ({Method})", 
            violation.ClientId, violation.Endpoint, violation.Method);
    }

    public Task RecordRateLimitHit(string clientId, string rule, string endpoint, bool allowed)
    {
        RecordRequest(clientId, rule);

        if (!allowed)
        {
            var violation = new RateLimitViolation
            {
                ClientId = clientId,
                Rule = rule,
                Endpoint = endpoint,
                Method = "Unknown", // Would need to pass this from middleware
                Timestamp = DateTime.UtcNow,
                UserAgent = string.Empty,
                RemoteIp = clientId.StartsWith("ip:") ? clientId.Substring(3) : "unknown"
            };

            RecordViolation(violation);
        }

        return Task.CompletedTask;
    }
}