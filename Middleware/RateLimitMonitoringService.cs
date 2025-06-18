namespace dotnet_rest_api.Middleware;

/// <summary>
/// Interface for monitoring rate limit events
/// </summary>
public interface IRateLimitMonitoringService
{
    Task RecordRateLimitExceeded(string clientId, string endpoint, string method, DateTime timestamp);
    Task RecordRequest(string clientId, string endpoint, string method, DateTime timestamp);
    Task<RateLimitStatistics> GetStatistics(string clientId, TimeSpan? period = null);
    Task<Dictionary<string, int>> GetTopRateLimitedClients(int count = 10, TimeSpan? period = null);
}

/// <summary>
/// Rate limit statistics
/// </summary>
public class RateLimitStatistics
{
    public string ClientId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int RateLimitedRequests { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public double RateLimitPercentage => TotalRequests > 0 ? (double)RateLimitedRequests / TotalRequests * 100 : 0;
}

/// <summary>
/// Implementation of rate limit monitoring service
/// </summary>
public class RateLimitMonitoringService : IRateLimitMonitoringService
{
    private readonly ILogger<RateLimitMonitoringService> _logger;
    private readonly Dictionary<string, List<RateLimitEvent>> _events = new();
    private readonly object _lock = new();

    public RateLimitMonitoringService(ILogger<RateLimitMonitoringService> logger)
    {
        _logger = logger;
    }

    public Task RecordRateLimitExceeded(string clientId, string endpoint, string method, DateTime timestamp)
    {
        lock (_lock)
        {
            if (!_events.ContainsKey(clientId))
                _events[clientId] = new List<RateLimitEvent>();

            _events[clientId].Add(new RateLimitEvent
            {
                ClientId = clientId,
                Endpoint = endpoint,
                Method = method,
                Timestamp = timestamp,
                IsRateLimited = true
            });

            // Keep only last 1000 events per client
            if (_events[clientId].Count > 1000)
            {
                _events[clientId] = _events[clientId]
                    .OrderByDescending(e => e.Timestamp)
                    .Take(1000)
                    .ToList();
            }
        }

        _logger.LogWarning("Rate limit exceeded for client {ClientId} on {Method} {Endpoint} at {Timestamp}",
            clientId, method, endpoint, timestamp);

        return Task.CompletedTask;
    }

    public Task RecordRequest(string clientId, string endpoint, string method, DateTime timestamp)
    {
        lock (_lock)
        {
            if (!_events.ContainsKey(clientId))
                _events[clientId] = new List<RateLimitEvent>();

            _events[clientId].Add(new RateLimitEvent
            {
                ClientId = clientId,
                Endpoint = endpoint,
                Method = method,
                Timestamp = timestamp,
                IsRateLimited = false
            });

            // Keep only last 1000 events per client
            if (_events[clientId].Count > 1000)
            {
                _events[clientId] = _events[clientId]
                    .OrderByDescending(e => e.Timestamp)
                    .Take(1000)
                    .ToList();
            }
        }

        return Task.CompletedTask;
    }

    public Task<RateLimitStatistics> GetStatistics(string clientId, TimeSpan? period = null)
    {
        period ??= TimeSpan.FromHours(1); // Default to 1 hour
        var cutoff = DateTime.UtcNow.Subtract(period.Value);

        lock (_lock)
        {
            if (!_events.ContainsKey(clientId))
            {
                return Task.FromResult(new RateLimitStatistics
                {
                    ClientId = clientId,
                    PeriodStart = cutoff,
                    PeriodEnd = DateTime.UtcNow
                });
            }

            var relevantEvents = _events[clientId]
                .Where(e => e.Timestamp >= cutoff)
                .ToList();

            return Task.FromResult(new RateLimitStatistics
            {
                ClientId = clientId,
                TotalRequests = relevantEvents.Count,
                RateLimitedRequests = relevantEvents.Count(e => e.IsRateLimited),
                PeriodStart = cutoff,
                PeriodEnd = DateTime.UtcNow
            });
        }
    }

    public Task<Dictionary<string, int>> GetTopRateLimitedClients(int count = 10, TimeSpan? period = null)
    {
        period ??= TimeSpan.FromHours(1);
        var cutoff = DateTime.UtcNow.Subtract(period.Value);

        lock (_lock)
        {
            var result = new Dictionary<string, int>();

            foreach (var kvp in _events)
            {
                var rateLimitedCount = kvp.Value
                    .Where(e => e.Timestamp >= cutoff && e.IsRateLimited)
                    .Count();

                if (rateLimitedCount > 0)
                    result[kvp.Key] = rateLimitedCount;
            }

            return Task.FromResult(result
                .OrderByDescending(kvp => kvp.Value)
                .Take(count)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }
    }
}

/// <summary>
/// Rate limit event for monitoring
/// </summary>
internal class RateLimitEvent
{
    public string ClientId { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool IsRateLimited { get; set; }
}
