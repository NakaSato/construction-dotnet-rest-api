using System.Security.Claims;

namespace dotnet_rest_api.Services;

/// <summary>
/// Service for extracting and validating user context from claims
/// </summary>
public interface IUserContextService
{
    /// <summary>
    /// Gets the current user's ID from claims
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>User ID if valid, null otherwise</returns>
    Guid? GetCurrentUserId(ClaimsPrincipal user);

    /// <summary>
    /// Gets the current user's role from claims
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>User role name</returns>
    string? GetCurrentUserRole(ClaimsPrincipal user);

    /// <summary>
    /// Gets the current user's context (ID and role)
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <returns>User context or null if invalid</returns>
    UserContext? GetCurrentUserContext(ClaimsPrincipal user);

    /// <summary>
    /// Validates if the user has the required role
    /// </summary>
    /// <param name="user">The claims principal</param>
    /// <param name="requiredRoles">Required roles (any of them)</param>
    /// <returns>True if user has any of the required roles</returns>
    bool HasRole(ClaimsPrincipal user, params string[] requiredRoles);
}

/// <summary>
/// User context containing ID and role information
/// </summary>
public class UserContext
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Implementation of user context service
/// </summary>
public class UserContextService : IUserContextService
{
    public Guid? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public string? GetCurrentUserRole(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value;
    }

    public UserContext? GetCurrentUserContext(ClaimsPrincipal user)
    {
        var userId = GetCurrentUserId(user);
        if (!userId.HasValue)
            return null;

        return new UserContext
        {
            UserId = userId.Value,
            Role = GetCurrentUserRole(user) ?? string.Empty,
            Username = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty
        };
    }

    public bool HasRole(ClaimsPrincipal user, params string[] requiredRoles)
    {
        var userRole = GetCurrentUserRole(user);
        return userRole != null && requiredRoles.Contains(userRole, StringComparer.OrdinalIgnoreCase);
    }
}
