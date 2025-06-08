using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
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
public class RateLimitAdminController : ControllerBase
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

            return Ok(ApiResponse<RateLimitStatistics>.SuccessResponse(
                statistics, 
                $"Rate limit statistics for the past {hours} hours retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rate limit statistics");
            return StatusCode(500, ApiResponse<RateLimitStatistics>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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

            return Ok(ApiResponse<List<ClientRateLimitInfo>>.SuccessResponse(
                topClients, 
                $"Top {count} clients retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving top clients");
            return StatusCode(500, ApiResponse<List<ClientRateLimitInfo>>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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

            return Ok(ApiResponse<List<RateLimitViolation>>.SuccessResponse(
                violations, 
                $"Rate limit violations for the past {hours} hours retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rate limit violations");
            return StatusCode(500, ApiResponse<List<RateLimitViolation>>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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

            return Ok(ApiResponse<Dictionary<string, RateLimitRule>>.SuccessResponse(
                rules, 
                "Active rate limiting rules retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving rate limiting rules");
            return StatusCode(500, ApiResponse<Dictionary<string, RateLimitRule>>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "clientId",
                            Code = "REQUIRED",
                            Message = "Client ID is required"
                        }
                    },
                    HttpContext.Request.Path, HttpContext.TraceIdentifier));
            }

            await _monitoringService.ClearClientLimits(clientId);

            _logger.LogInformation("Rate limits cleared for client {ClientId} by user {UserId}", 
                clientId, User.Identity?.Name ?? "Unknown");

            return Ok(ApiResponse<object>.SuccessResponse(
                null, 
                $"Rate limits cleared for client {clientId}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing rate limits for client {ClientId}", clientId);
            return StatusCode(500, ApiResponse<object>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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

            return Ok(ApiResponse<object>.SuccessResponse(
                null, 
                "All rate limits cleared successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all rate limits");
            return StatusCode(500, ApiResponse<object>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "ruleName",
                            Code = "REQUIRED",
                            Message = "Rule name is required"
                        }
                    },
                    HttpContext.Request.Path, HttpContext.TraceIdentifier));
            }

            if (rule.Limit <= 0)
            {
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "limit",
                            Code = "INVALID_VALUE",
                            Message = "Limit must be greater than 0"
                        }
                    },
                    HttpContext.Request.Path, HttpContext.TraceIdentifier));
            }

            if (rule.Period <= TimeSpan.Zero)
            {
                return BadRequest(ApiResponse<object>.ValidationErrorResponse(
                    new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Field = "period",
                            Code = "INVALID_VALUE",
                            Message = "Period must be greater than 0"
                        }
                    },
                    HttpContext.Request.Path, HttpContext.TraceIdentifier));
            }

            await _monitoringService.UpdateRuleConfiguration(ruleName, rule);

            _logger.LogInformation("Rate limiting rule {RuleName} updated by user {UserId}: {Limit} requests per {Period}", 
                ruleName, User.Identity?.Name ?? "Unknown", rule.Limit, rule.Period);

            return Ok(ApiResponse<object>.SuccessResponse(
                null, 
                $"Rate limiting rule '{ruleName}' updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating rate limiting rule {RuleName}", ruleName);
            return StatusCode(500, ApiResponse<object>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
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
            return Ok(ApiResponse<object>.SuccessResponse(
                new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    service = "rate-limiting"
                },
                "Rate limiting service is healthy"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rate limiting health check failed");
            return StatusCode(500, ApiResponse<object>.ServerErrorResponse(
                HttpContext.Request.Path, HttpContext.TraceIdentifier));
        }
    }
}