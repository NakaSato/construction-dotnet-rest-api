using Microsoft.AspNetCore.Mvc.Filters;

namespace dotnet_rest_api.Attributes;

/// <summary>
/// Attribute to specify custom rate limiting rules for controller actions
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RateLimitAttribute : Attribute, IActionFilter
{
    public string Rule { get; }
    public int? Limit { get; set; }
    public TimeSpan? Period { get; set; }

    public RateLimitAttribute(string rule)
    {
        Rule = rule;
    }

    public RateLimitAttribute(int limit, int periodMinutes)
    {
        Rule = "custom";
        Limit = limit;
        Period = TimeSpan.FromMinutes(periodMinutes);
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Add rate limit rule to request context
        context.HttpContext.Items["RateLimit.Rule"] = Rule;
        
        if (Limit.HasValue)
            context.HttpContext.Items["RateLimit.Limit"] = Limit.Value;
            
        if (Period.HasValue)
            context.HttpContext.Items["RateLimit.Period"] = Period.Value;
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}

/// <summary>
/// Attribute for restrictive rate limiting on delete operations
/// </summary>
public class DeleteRateLimitAttribute : RateLimitAttribute
{
    public DeleteRateLimitAttribute() : base("delete-operations")
    {
    }
}

/// <summary>
/// Attribute for very restrictive rate limiting on critical delete operations
/// </summary>
public class CriticalDeleteRateLimitAttribute : RateLimitAttribute
{
    public CriticalDeleteRateLimitAttribute() : base("critical-delete")
    {
    }
}
