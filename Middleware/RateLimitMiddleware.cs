using System.Net;
using System.Text.Json;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;

namespace dotnet_rest_api.Middleware;

/// <summary>
/// Rate limiting middleware that enforces request limits per client
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitService _rateLimitService;
    private readonly IRateLimitMonitoringService _monitoringService;
    private readonly RateLimitOptions _options;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(
        RequestDelegate next,
        IRateLimitService rateLimitService,
        IRateLimitMonitoringService monitoringService,
        RateLimitOptions options,
        ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _rateLimitService = rateLimitService;
        _monitoringService = monitoringService;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Rate limiting middleware invoked for {Path}", context.Request.Path);
        
        // Skip rate limiting for health checks and certain endpoints
        if (ShouldSkipRateLimit(context))
        {
            _logger.LogInformation("Skipping rate limiting for {Path}", context.Request.Path);
            await _next(context);
            return;
        }

        var clientId = _rateLimitService.GetClientIdentifier(context);
        _logger.LogInformation("Rate limiting check for client {ClientId} on {Path}", clientId, context.Request.Path);
        
        // Check IP whitelist
        if (_options.EnableIpWhitelist && IsWhitelistedClient(clientId))
        {
            _logger.LogInformation("Client {ClientId} is whitelisted, skipping rate limiting", clientId);
            await _next(context);
            return;
        }

        var endpoint = context.Request.Path.Value ?? "/";
        var method = context.Request.Method;

        // Check for custom rate limit rule from attribute
        var customRule = context.Items["RateLimit.Rule"] as string;
        var ruleToUse = customRule ?? _rateLimitService.GetApplicableRule(endpoint, method);

        try
        {
            var rateLimitResult = await _rateLimitService.CheckRateLimit(clientId, endpoint, method);

            // Record metrics
            if (rateLimitResult.IsAllowed)
            {
                await _monitoringService.RecordRequest(clientId, endpoint, method, DateTime.UtcNow);
            }
            else
            {
                await _monitoringService.RecordRateLimitExceeded(clientId, endpoint, method, DateTime.UtcNow);
            }

            // Add rate limit headers
            AddRateLimitHeaders(context, rateLimitResult);
            _logger.LogInformation("Rate limit headers added. Limit: {Limit}, Remaining: {Remaining}, IsAllowed: {IsAllowed}", 
                rateLimitResult.Limit, rateLimitResult.Remaining, rateLimitResult.IsAllowed);

            if (!rateLimitResult.IsAllowed)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} on endpoint {Endpoint} ({Method}). Rule: {Rule}", 
                    clientId, endpoint, method, rateLimitResult.Rule);

                await HandleRateLimitExceeded(context, rateLimitResult);
                return;
            }

            _logger.LogDebug("Rate limit check passed for client {ClientId} on endpoint {Endpoint} ({Method}). Remaining: {Remaining}", 
                clientId, endpoint, method, rateLimitResult.Remaining);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking rate limit for client {ClientId}", clientId);
            
            // On error, allow the request to proceed to avoid breaking the API
            await _next(context);
        }
    }

    private bool ShouldSkipRateLimit(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        
        // Skip health checks
        if (path?.Contains("/health") == true)
            return true;

        // Skip swagger endpoints in development
        if (path?.Contains("/swagger") == true)
            return true;

        // Skip static files
        if (path?.Contains("/files") == true)
            return true;

        return false;
    }

    private bool IsWhitelistedClient(string clientId)
    {
        _logger.LogInformation("IsWhitelistedClient check: EnableIpWhitelist={EnableIpWhitelist}, IpWhitelist.Count={Count}, ClientId={ClientId}", 
            _options.EnableIpWhitelist, _options.IpWhitelist.Count, clientId);
            
        if (!_options.EnableIpWhitelist || !_options.IpWhitelist.Any())
        {
            _logger.LogInformation("IP whitelist disabled or empty, allowing rate limiting");
            return false;
        }

        // Extract IP from client ID if it's an IP-based identifier
        if (clientId.StartsWith("ip:"))
        {
            var ip = clientId.Substring(3);
            bool isWhitelisted = _options.IpWhitelist.Contains(ip) || 
                   (_options.IpWhitelist.Contains("127.0.0.1") && IsLocalhost(ip));
            _logger.LogInformation("IP {IP} whitelist check result: {IsWhitelisted}", ip, isWhitelisted);
            return isWhitelisted;
        }

        return false;
    }

    private bool IsLocalhost(string ip)
    {
        return ip == "127.0.0.1" || ip == "::1" || ip == "localhost";
    }

    private void AddRateLimitHeaders(HttpContext context, RateLimitResult result)
    {
        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();
        context.Response.Headers["X-RateLimit-Reset"] = ((DateTimeOffset)result.ResetTime).ToUnixTimeSeconds().ToString();
        context.Response.Headers["X-RateLimit-Rule"] = result.Rule;

        if (!result.IsAllowed)
        {
            context.Response.Headers["Retry-After"] = ((int)result.RetryAfter.TotalSeconds).ToString();
        }
    }

    private async Task HandleRateLimitExceeded(HttpContext context, RateLimitResult result)
    {
        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        var errorResponse = ApiResponse<object>.CreateRateLimitErrorResponse(
            result.Limit,
            result.ResetTime,
            result.RetryAfter,
            context.Request.Path.Value,
            context.TraceIdentifier
        );

        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// Extension methods for adding rate limiting to the application
/// </summary>
public static class RateLimitExtensions
{
    /// <summary>
    /// Adds rate limiting services to the DI container
    /// </summary>
    public static IServiceCollection AddRateLimit(this IServiceCollection services, IConfiguration configuration)
    {
        var rateLimitSection = configuration.GetSection("RateLimit");
        var options = new RateLimitOptions();
        rateLimitSection.Bind(options);

        // Debug logging to see what was actually loaded
        Console.WriteLine($"[DEBUG] Rate limiting configuration loaded:");
        Console.WriteLine($"[DEBUG] EnableIpWhitelist: {options.EnableIpWhitelist}");
        Console.WriteLine($"[DEBUG] IpWhitelist count: {options.IpWhitelist.Count}");
        Console.WriteLine($"[DEBUG] IpWhitelist items: [{string.Join(", ", options.IpWhitelist)}]");
        Console.WriteLine($"[DEBUG] Rules count: {options.Rules.Count}");

        // Set up default rules if none are configured
        if (!options.Rules.Any())
        {
            options.Rules["default"] = new RateLimitRule
            {
                Limit = 2000,
                Period = TimeSpan.FromMinutes(1)
            };

            options.Rules["auth"] = new RateLimitRule
            {
                Limit = 100,
                Period = TimeSpan.FromMinutes(1),
                Endpoints = new List<string> { "/api/v1/auth" }
            };

            options.Rules["upload"] = new RateLimitRule
            {
                Limit = 200,
                Period = TimeSpan.FromMinutes(1),
                Endpoints = new List<string> { "/api/v1/images" },
                HttpMethods = new List<string> { "POST" }
            };

            // Rate limiting for DELETE operations
            options.Rules["delete-operations"] = new RateLimitRule
            {
                Limit = 50,  // Increased from 10 to 50 delete operations per minute
                Period = TimeSpan.FromMinutes(1),
                HttpMethods = new List<string> { "DELETE" }
            };

            // Very restrictive rate limiting for critical DELETE operations
            options.Rules["critical-delete"] = new RateLimitRule
            {
                Limit = 20,   // Increased from 3 to 20 critical deletes per 5 minutes
                Period = TimeSpan.FromMinutes(5),
                HttpMethods = new List<string> { "DELETE" }
            };
        }

        services.AddSingleton(options);
        services.AddSingleton<IRateLimitStorage, InMemoryRateLimitStorage>();
        services.AddSingleton<IRateLimitService, RateLimitService>();
        services.AddSingleton<IRateLimitMonitoringService, RateLimitMonitoringService>();

        return services;
    }

    /// <summary>
    /// Adds rate limiting middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseRateLimit(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RateLimitMiddleware>();
    }
}
