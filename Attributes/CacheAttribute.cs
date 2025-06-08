
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace dotnet_rest_api.Attributes;

/// <summary>
/// Attribute to enable HTTP caching with Cache-Control headers
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class CacheAttribute : Attribute, IActionFilter
{
    private readonly int _durationInSeconds;
    private readonly bool _isPublic;
    private readonly bool _noStore;
    private readonly bool _mustRevalidate;
    private readonly string? _vary;

    /// <summary>
    /// Initializes caching with specified duration
    /// </summary>
    /// <param name="durationInSeconds">Cache duration in seconds</param>
    /// <param name="isPublic">Whether cache can be shared (public) or private</param>
    /// <param name="noStore">Whether to prevent caching entirely</param>
    /// <param name="mustRevalidate">Whether to require revalidation</param>
    /// <param name="vary">Vary header value for conditional caching</param>
    public CacheAttribute(
        int durationInSeconds = 300, 
        bool isPublic = false, 
        bool noStore = false,
        bool mustRevalidate = false,
        string? vary = null)
    {
        _durationInSeconds = durationInSeconds;
        _isPublic = isPublic;
        _noStore = noStore;
        _mustRevalidate = mustRevalidate;
        _vary = vary;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // No action needed before execution
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult result && result.StatusCode == 200)
        {
            var response = context.HttpContext.Response;
            
            if (_noStore)
            {
                response.Headers["Cache-Control"] = "no-store, no-cache";
                response.Headers["Pragma"] = "no-cache";
                return;
            }

            var cacheControlParts = new List<string>();
            
            if (_isPublic)
                cacheControlParts.Add("public");
            else
                cacheControlParts.Add("private");
                
            cacheControlParts.Add($"max-age={_durationInSeconds}");
            
            if (_mustRevalidate)
                cacheControlParts.Add("must-revalidate");

            response.Headers["Cache-Control"] = string.Join(", ", cacheControlParts);
            
            if (!string.IsNullOrEmpty(_vary))
            {
                response.Headers["Vary"] = _vary;
            }
        }
    }
}

/// <summary>
/// Attribute for no-cache responses
/// </summary>
public class NoCacheAttribute : CacheAttribute
{
    public NoCacheAttribute() : base(noStore: true)
    {
    }
}

/// <summary>
/// Attribute for short-term caching (5 minutes)
/// </summary>
public class ShortCacheAttribute : CacheAttribute
{
    public ShortCacheAttribute() : base(300, isPublic: false, vary: "Authorization")
    {
    }
}

/// <summary>
/// Attribute for medium-term caching (15 minutes)
/// </summary>
public class MediumCacheAttribute : CacheAttribute
{
    public MediumCacheAttribute() : base(900, isPublic: false, vary: "Authorization")
    {
    }
}

/// <summary>
/// Attribute for long-term caching (1 hour)
/// </summary>
public class LongCacheAttribute : CacheAttribute
{
    public LongCacheAttribute() : base(3600, isPublic: false, vary: "Authorization")
    {
    }
}

/// <summary>
/// Attribute for public cacheable resources (30 minutes)
/// </summary>
public class PublicCacheAttribute : CacheAttribute
{
    public PublicCacheAttribute() : base(1800, isPublic: true)
    {
    }
}
