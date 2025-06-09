using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Controllers;
using dotnet_rest_api.Middleware;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Controller for rate limit administration and monitoring
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/rate-limit")]
[Authorize] // Require authentication for admin operations
public class RateLimitAdminController : BaseApiController
{
    private readonly IRateLimitMonitoringService _monitoringService;
    private readonly ILogger<RateLimitAdminController> _logger;

    public RateLimitAdminController(
        IRateLimitMonitoringService monitoringService,
        ILogger<RateLimitAdminController> logger)
    {
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Get rate limiting statistics
    /// </summary>
    /// <param name="hours">Number of hours to look back (default: 1)</param>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<RateLimitStatistics>>> GetStatistics([FromQuery] int hours = 1)
    {
        try
        {
            var period = TimeSpan.FromHours(Math.Max(1, Math.Min(24, hours))); // Limit between 1-24 hours
            var statistics = await _monitoringService.GetStatistics(period);

            return CreateSuccessResponse(statistics, $"Rate limit statistics for the past {hours} hours retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<RateLimitStatistics>(_logger, ex, "retrieving rate limit statistics");
        }
    }

    /// <summary>
    /// Get top clients by request count
    /// </summary>
    /// <param name="count">Number of top clients to return (default: 10)</param>
    [HttpGet("clients/top")]
    public async Task<ActionResult<ApiResponse<List<ClientRateLimitInfo>>>> GetTopClients([FromQuery] int count = 10)
    {
        try
        {
            var topClients = await _monitoringService.GetTopClients(Math.Max(1, Math.Min(100, count)));

            return CreateSuccessResponse(topClients, $"Top {count} clients retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<List<ClientRateLimitInfo>>(_logger, ex, "retrieving top clients");
        }
    }

    /// <summary>
    /// Get recent rate limit violations
    /// </summary>
    /// <param name="hours">Number of hours to look back (default: 1)</param>
    [HttpGet("violations")]
    public async Task<ActionResult<ApiResponse<List<RateLimitViolation>>>> GetViolations([FromQuery] int hours = 1)
    {
        try
        {
            var period = TimeSpan.FromHours(Math.Max(1, Math.Min(24, hours)));
            var violations = await _monitoringService.GetRecentViolations(period);

            return CreateSuccessResponse(violations, $"Rate limit violations for the past {hours} hours retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<List<RateLimitViolation>>(_logger, ex, "retrieving rate limit violations");
        }
    }

    /// <summary>
    /// Get active rate limiting rules
    /// </summary>
    [HttpGet("rules")]
    public async Task<ActionResult<ApiResponse<Dictionary<string, RateLimitRule>>>> GetRules()
    {
        try
        {
            var rules = await _monitoringService.GetActiveRules();

            return CreateSuccessResponse(rules, "Active rate limiting rules retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<Dictionary<string, RateLimitRule>>(_logger, ex, "retrieving rate limiting rules");
        }
    }

    /// <summary>
    /// Clear rate limits for a specific client
    /// </summary>
    /// <param name="clientId">The client identifier to clear limits for</param>
    [HttpDelete("clients/{clientId}")]
    public async Task<ActionResult<ApiResponse<object>>> ClearClientLimits(string clientId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                return CreateErrorResponse("Client ID is required", 400);
            }

            await _monitoringService.ClearClientLimits(clientId);

            _logger.LogInformation("Rate limits cleared for client {ClientId} by user {UserId}", 
                clientId, User.Identity?.Name ?? "Unknown");

            return CreateSuccessResponse<object>(null, $"Rate limits cleared for client {clientId}");
        }
        catch (Exception ex)
        {
            return HandleException<object>(_logger, ex, $"clearing rate limits for client {clientId}");
        }
    }

    /// <summary>
    /// Clear all rate limits (use with caution)
    /// </summary>
    [HttpDelete("all")]
    public async Task<ActionResult<ApiResponse<object>>> ClearAllLimits()
    {
        try
        {
            await _monitoringService.ClearAllLimits();

            _logger.LogWarning("All rate limits cleared by user {UserId}", 
                User.Identity?.Name ?? "Unknown");

            return CreateSuccessResponse<object>(null, "All rate limits cleared successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "clearing all rate limits");
        }
    }

    /// <summary>
    /// Update a rate limiting rule (runtime configuration)
    /// </summary>
    /// <param name="ruleName">Name of the rule to update</param>
    /// <param name="rule">New rule configuration</param>
    [HttpPut("rules/{ruleName}")]
    public async Task<ActionResult<ApiResponse<object>>> UpdateRule(string ruleName, [FromBody] RateLimitRule rule)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(ruleName))
            {
                return CreateErrorResponse("Rule name is required", 400);
            }

            if (rule.Limit <= 0)
            {
                return CreateErrorResponse("Limit must be greater than 0", 400);
            }

            if (rule.Period <= TimeSpan.Zero)
            {
                return CreateErrorResponse("Period must be greater than 0", 400);
            }

            await _monitoringService.UpdateRuleConfiguration(ruleName, rule);

            _logger.LogInformation("Rate limiting rule {RuleName} updated by user {UserId}: {Limit} requests per {Period}", 
                ruleName, User.Identity?.Name ?? "Unknown", rule.Limit, rule.Period);

            return CreateSuccessResponse<object>(null, $"Rate limiting rule '{ruleName}' updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"updating rate limiting rule {ruleName}");
        }
    }

    /// <summary>
    /// Health check endpoint for rate limiting system
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous] // Allow anonymous access for health checks
    public ActionResult<ApiResponse<object>> HealthCheck()
    {
        try
        {
            var healthData = new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                service = "rate-limiting"
            };

            return CreateSuccessResponse<object>(healthData, "Rate limiting service is healthy");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "rate limiting health check");
        }
    }
}