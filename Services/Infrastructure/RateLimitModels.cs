namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Rate limit statistics
/// </summary>
public class RateLimitStatistics
{
    public int TotalRequests { get; set; }
    public int AllowedRequests { get; set; }
    public int BlockedRequests { get; set; }
    public int UniqueClients { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string Period { get; set; } = string.Empty;
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
/// Rate limit result
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RequestCount { get; set; }
    public int RequestLimit { get; set; }
    public int Limit { get; set; }
    public int Remaining { get; set; }
    public DateTime ResetTime { get; set; }
    public TimeSpan RetryAfter { get; set; }
    public string RuleName { get; set; } = string.Empty;
    public string Rule { get; set; } = string.Empty;
}

/// <summary>
/// Rate limit rule configuration
/// </summary>
public class RateLimitRule
{
    public string Name { get; set; } = string.Empty;
    public int RequestLimit { get; set; }
    public TimeSpan TimeWindow { get; set; }
    public string Pattern { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string Description { get; set; } = string.Empty;
}
