using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace dotnet_rest_api.Middleware;

/// <summary>
/// HTTP caching middleware for ETag and Cache-Control headers
/// </summary>
public class HttpCacheMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<HttpCacheMiddleware> _logger;

    public HttpCacheMiddleware(RequestDelegate next, ILogger<HttpCacheMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only apply caching to GET requests
        if (context.Request.Method != HttpMethods.Get)
        {
            await _next(context);
            return;
        }

        // Skip caching for certain paths
        if (ShouldSkipCaching(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Create a custom response stream to capture the response
        var originalBodyStream = context.Response.Body;
        using var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        await _next(context);

        // Only apply caching to successful responses
        if (context.Response.StatusCode == 200)
        {
            await ApplyHttpCaching(context, responseBodyStream);
        }

        // Copy the response back to the original stream
        responseBodyStream.Position = 0;
        await responseBodyStream.CopyToAsync(originalBodyStream);
        context.Response.Body = originalBodyStream;
    }

    private async Task ApplyHttpCaching(HttpContext context, MemoryStream responseBodyStream)
    {
        try
        {
            var responseBody = responseBodyStream.ToArray();
            var etag = GenerateETag(responseBody);
            var cacheControl = GetCacheControlHeader(context.Request.Path);

            // Set ETag header
            context.Response.Headers.ETag = $"\"{etag}\"";
            
            // Set Cache-Control header
            context.Response.Headers.CacheControl = cacheControl;
            
            // Set Last-Modified header (current time)
            context.Response.Headers.LastModified = DateTime.UtcNow.ToString("R");

            // Check If-None-Match header (ETag validation)
            var ifNoneMatch = context.Request.Headers.IfNoneMatch.FirstOrDefault();
            if (!string.IsNullOrEmpty(ifNoneMatch) && ifNoneMatch.Trim('"') == etag)
            {
                _logger.LogDebug("ETag match found, returning 304 Not Modified for path: {Path}", context.Request.Path);
                context.Response.StatusCode = 304;
                context.Response.Body = Stream.Null;
                return;
            }

            // Check If-Modified-Since header
            var ifModifiedSince = context.Request.Headers.IfModifiedSince.FirstOrDefault();
            if (!string.IsNullOrEmpty(ifModifiedSince) && 
                DateTime.TryParse(ifModifiedSince, out var modifiedSince))
            {
                var cacheMaxAge = GetCacheMaxAge(context.Request.Path);
                if (DateTime.UtcNow - modifiedSince < cacheMaxAge)
                {
                    _logger.LogDebug("Content not modified, returning 304 for path: {Path}", context.Request.Path);
                    context.Response.StatusCode = 304;
                    context.Response.Body = Stream.Null;
                    return;
                }
            }

            _logger.LogDebug("Applied HTTP caching headers for path: {Path}, ETag: {ETag}", 
                context.Request.Path, etag);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying HTTP caching for path: {Path}", context.Request.Path);
        }
    }

    private string GenerateETag(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(content);
        return Convert.ToBase64String(hash)[..16]; // Use first 16 characters for shorter ETag
    }

    private string GetCacheControlHeader(PathString path)
    {
        return path.Value switch
        {
            // User roles - cache for 24 hours (rarely change)
            var p when p.Contains("/api/v1/users") && p.Contains("/roles") => "public, max-age=86400, s-maxage=86400",
            
            // Project statuses and metadata - cache for 1 hour
            var p when p.Contains("/api/v1/projects") && (p.Contains("/statuses") || p.Contains("/metadata")) => "public, max-age=3600, s-maxage=3600",
            
            // User profiles - cache for 30 minutes
            var p when p.Contains("/api/v1/users/") && !p.Contains("/tasks") && !p.Contains("/projects") => "private, max-age=1800",
            
            // Project lists (with pagination) - cache for 10 minutes
            var p when p.Contains("/api/v1/projects") && !p.Contains("/tasks") => "public, max-age=600, s-maxage=600",
            
            // Task lists - cache for 5 minutes (more dynamic)
            var p when p.Contains("/api/v1/tasks") => "public, max-age=300, s-maxage=300",
            
            // Image metadata - cache for 2 hours
            var p when p.Contains("/api/v1/images") && p.Contains("/metadata") => "public, max-age=7200, s-maxage=7200",
            
            // Health checks - no cache
            var p when p.Contains("/health") => "no-cache, no-store, must-revalidate",
            
            // Authentication endpoints - no cache
            var p when p.Contains("/auth") => "no-cache, no-store, must-revalidate",
            
            // Rate limiting admin - no cache
            var p when p.Contains("/rate-limit") => "no-cache, no-store, must-revalidate",
            
            // Default caching for other endpoints
            _ => "public, max-age=300, s-maxage=300"
        };
    }

    private TimeSpan GetCacheMaxAge(PathString path)
    {
        return path.Value switch
        {
            var p when p.Contains("/users") && p.Contains("/roles") => TimeSpan.FromHours(24),
            var p when p.Contains("/projects") && p.Contains("/metadata") => TimeSpan.FromHours(1),
            var p when p.Contains("/users/") => TimeSpan.FromMinutes(30),
            var p when p.Contains("/projects") => TimeSpan.FromMinutes(10),
            var p when p.Contains("/tasks") => TimeSpan.FromMinutes(5),
            var p when p.Contains("/images") => TimeSpan.FromHours(2),
            _ => TimeSpan.FromMinutes(5)
        };
    }

    private bool ShouldSkipCaching(PathString path)
    {
        var pathValue = path.Value?.ToLowerInvariant() ?? "";
        
        return pathValue.Contains("/health") ||
               pathValue.Contains("/auth/") ||
               pathValue.Contains("/swagger") ||
               pathValue.Contains("/rate-limit/admin") ||
               pathValue.Contains("/debug");
    }
}

/// <summary>
/// Action filter for custom cache control on specific endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class CacheAttribute : ActionFilterAttribute
{
    private readonly int _maxAgeSeconds;
    private readonly bool _isPrivate;
    private readonly bool _noCache;
    private readonly bool _mustRevalidate;

    public CacheAttribute(int maxAgeSeconds, bool isPrivate = false, bool noCache = false, bool mustRevalidate = false)
    {
        _maxAgeSeconds = maxAgeSeconds;
        _isPrivate = isPrivate;
        _noCache = noCache;
        _mustRevalidate = mustRevalidate;
    }

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.StatusCode == 200)
        {
            var cacheControl = BuildCacheControlHeader();
            context.HttpContext.Response.Headers.CacheControl = cacheControl;
            
            // Generate ETag for the response
            if (objectResult.Value != null)
            {
                var etag = GenerateETag(objectResult.Value);
                context.HttpContext.Response.Headers.ETag = $"\"{etag}\"";
            }
        }

        base.OnActionExecuted(context);
    }

    private string BuildCacheControlHeader()
    {
        if (_noCache)
        {
            return "no-cache, no-store, must-revalidate";
        }

        var parts = new List<string>();
        
        parts.Add(_isPrivate ? "private" : "public");
        parts.Add($"max-age={_maxAgeSeconds}");
        
        if (!_isPrivate)
        {
            parts.Add($"s-maxage={_maxAgeSeconds}");
        }
        
        if (_mustRevalidate)
        {
            parts.Add("must-revalidate");
        }

        return string.Join(", ", parts);
    }

    private string GenerateETag(object value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash)[..16];
        }
        catch
        {
            return Guid.NewGuid().ToString("N")[..16];
        }
    }
}

/// <summary>
/// Extension methods for HTTP caching middleware
/// </summary>
public static class HttpCacheExtensions
{
    public static IApplicationBuilder UseHttpCaching(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HttpCacheMiddleware>();
    }
}

/// <summary>
/// Predefined cache attribute configurations for common scenarios
/// </summary>
public static class CacheProfiles
{
    /// <summary>
    /// Cache for user roles and permissions (24 hours)
    /// </summary>
    public class RolesCache : CacheAttribute
    {
        public RolesCache() : base(86400, false, false, false) { }
    }

    /// <summary>
    /// Cache for user profiles (30 minutes, private)
    /// </summary>
    public class UserProfileCache : CacheAttribute
    {
        public UserProfileCache() : base(1800, true, false, false) { }
    }

    /// <summary>
    /// Cache for project metadata (1 hour)
    /// </summary>
    public class ProjectMetadataCache : CacheAttribute
    {
        public ProjectMetadataCache() : base(3600, false, false, false) { }
    }

    /// <summary>
    /// Cache for project lists (10 minutes)
    /// </summary>
    public class ProjectListCache : CacheAttribute
    {
        public ProjectListCache() : base(600, false, false, false) { }
    }

    /// <summary>
    /// Cache for task lists (5 minutes)
    /// </summary>
    public class TaskListCache : CacheAttribute
    {
        public TaskListCache() : base(300, false, false, false) { }
    }

    /// <summary>
    /// No cache for dynamic/sensitive data
    /// </summary>
    public class NoCache : CacheAttribute
    {
        public NoCache() : base(0, false, true, true) { }
    }
}
