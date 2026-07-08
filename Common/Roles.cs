namespace dotnet_rest_api.Common;

/// <summary>
/// Canonical role names. These MUST match the roles seeded in
/// <see cref="dotnet_rest_api.Data.ApplicationDbContext"/> (RoleId 1-4) and the
/// role claim emitted by AuthService. Use these constants in [Authorize(Roles = ...)]
/// instead of raw strings so authorization gates cannot drift from the seeded set.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Manager = "Manager";
    public const string User = "User";
    public const string Viewer = "Viewer";

    /// <summary>Admin or Manager — the common "can manage" gate for write/workflow actions.</summary>
    public const string AdminOrManager = Admin + "," + Manager;
}
