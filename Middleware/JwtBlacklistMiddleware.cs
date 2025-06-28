using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace dotnet_rest_api.Middleware;

public class JwtBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IMemoryCache _cache;
    private readonly ILogger<JwtBlacklistMiddleware> _logger;

    public JwtBlacklistMiddleware(RequestDelegate next, IMemoryCache cache, ILogger<JwtBlacklistMiddleware> logger)
    {
        _next = next;
        _cache = cache;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only check JWT tokens for authenticated endpoints
        var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        
        if (authHeader != null && authHeader.StartsWith("Bearer "))
        {
            var token = authHeader.Substring("Bearer ".Length).Trim();
            
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                if (tokenHandler.CanReadToken(token))
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    var tokenId = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value ?? token;
                    var cacheKey = $"blacklisted_token_{tokenId}";
                    
                    // Check if token is blacklisted
                    if (_cache.TryGetValue(cacheKey, out _))
                    {
                        _logger.LogInformation("Blocked blacklisted token: {TokenId}", tokenId);
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("Token has been invalidated");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error checking token blacklist");
                // Continue processing - let the JWT middleware handle invalid tokens
            }
        }

        await _next(context);
    }
}
