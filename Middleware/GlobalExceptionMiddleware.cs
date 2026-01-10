using System.Diagnostics;
using System.Text.Json;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next, 
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        
        // Log the exception with structured data
        _logger.LogError(
            exception, 
            "Unhandled exception occurred. TraceId: {TraceId}, Path: {Path}, Method: {Method}",
            traceId,
            context.Request.Path,
            context.Request.Method);

        context.Response.ContentType = "application/json";
        
        // Determine status code based on exception type
        context.Response.StatusCode = exception switch
        {
            KeyNotFoundException => StatusCodes.Status404NotFound,
            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
            ArgumentException => StatusCodes.Status400BadRequest,
            InvalidOperationException => StatusCodes.Status400BadRequest,
            TimeoutException => StatusCodes.Status504GatewayTimeout,
            OperationCanceledException => StatusCodes.Status499ClientClosedRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        // Build error response
        var response = new ApiResponse<object>
        {
            Success = false,
            Message = GetUserFriendlyMessage(exception, context.Response.StatusCode),
            Errors = new List<string> { exception.Message },
            Error = new 
            { 
                TraceId = traceId,
                Type = exception.GetType().Name,
                // Only include stack trace in development
                StackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
            }
        };

        var jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
        
        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }

    private static string GetUserFriendlyMessage(Exception exception, int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status404NotFound => "The requested resource was not found.",
            StatusCodes.Status401Unauthorized => "You are not authorized to access this resource.",
            StatusCodes.Status400BadRequest => exception.Message,
            StatusCodes.Status504GatewayTimeout => "The request timed out. Please try again.",
            StatusCodes.Status499ClientClosedRequest => "The request was cancelled.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }
}

/// <summary>
/// Extension method for registering the middleware
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
