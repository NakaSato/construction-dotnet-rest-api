using System.Security.Cryptography;
using System.Text;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using dotnet_rest_api.Services.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

// Models namespace defines a `Task` entity that collides with System.Threading.Tasks.Task.
using Task = System.Threading.Tasks.Task;

namespace UnitTests;

/// <summary>
/// Pins the security-critical refresh-token flow in <see cref="AuthService"/>:
/// single-use rotation, replay/reuse detection (which revokes the whole family),
/// expiry, unknown-token rejection, and the inactive-user guard.
/// Each test gets an isolated in-memory database.
/// </summary>
public class RefreshTokenRotationTests
{
    private static AuthService NewService(out ApplicationDbContext ctx)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"refresh-{Guid.NewGuid()}")
            .Options;
        ctx = new ApplicationDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                // HMAC-SHA256 needs >= 256 bits of key material.
                ["Jwt:Key"] = "unit-test-signing-key-that-is-at-least-32-bytes-long!!",
                ["Jwt:Issuer"] = "SolarProjectsAPI",
                ["Jwt:Audience"] = "SolarProjectsClient",
            })
            .Build();

        return new AuthService(ctx, config, new MemoryCache(new MemoryCacheOptions()),
            NullLogger<AuthService>.Instance);
    }

    private static User SeedUser(ApplicationDbContext ctx, bool active = true)
    {
        // User -> Role is a required relationship; AuthService Include()s Role on both
        // login and refresh. Under the EF in-memory provider an Include over a required
        // nav filters the principal out when the FK target is absent, so the Role row
        // must exist or the user/refresh-token lookups silently return null.
        if (!ctx.Roles.Any(r => r.RoleId == 3))
        {
            ctx.Roles.Add(new Role { RoleId = 3, RoleName = "User" });
            ctx.SaveChanges();
        }

        var user = new User
        {
            UserId = Guid.NewGuid(),
            Username = "alice",
            Email = "alice@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Secret123!"),
            FullName = "Alice",
            RoleId = 3,
            IsActive = active,
            CreatedAt = DateTime.UtcNow,
        };
        ctx.Users.Add(user);
        ctx.SaveChanges();
        return user;
    }

    // Mirrors AuthService.HashRefreshToken so tests can seed a row for a known raw token.
    private static string Hash(string raw)
        => Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));

    private static async Task<string> LoginAndGetRefreshToken(AuthService svc)
    {
        var login = await svc.LoginAsync(new LoginRequest { Username = "alice", Password = "Secret123!" });
        Assert.True(login.IsSuccess);
        var token = login.Data!.RefreshToken;
        Assert.False(string.IsNullOrWhiteSpace(token));
        return token!;
    }

    [Fact]
    public async Task Refresh_RotatesToken_IssuesNewAndRevokesOld()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);
        var original = await LoginAndGetRefreshToken(svc);

        var result = await svc.RefreshTokenAsync(original);

        Assert.True(result.IsSuccess);
        var rotated = result.Data!.RefreshToken!;
        Assert.NotEqual(original, rotated);

        // Consumed row is revoked and linked to its successor.
        var oldRow = await ctx.RefreshTokens.SingleAsync(rt => rt.TokenHash == Hash(original));
        Assert.NotNull(oldRow.RevokedAt);
        Assert.Equal(Hash(rotated), oldRow.ReplacedByTokenHash);

        // Successor is active.
        var newRow = await ctx.RefreshTokens.SingleAsync(rt => rt.TokenHash == Hash(rotated));
        Assert.Null(newRow.RevokedAt);
        Assert.True(newRow.IsActive);
    }

    [Fact]
    public async Task Refresh_ReuseOfRotatedToken_IsRejected_AndRevokesWholeFamily()
    {
        var svc = NewService(out var ctx);
        var user = SeedUser(ctx);
        var original = await LoginAndGetRefreshToken(svc);

        // First refresh succeeds and rotates.
        var first = await svc.RefreshTokenAsync(original);
        Assert.True(first.IsSuccess);

        // Replaying the already-rotated token is reuse: reject...
        var replay = await svc.RefreshTokenAsync(original);
        Assert.False(replay.IsSuccess);
        Assert.Contains("Invalid refresh token", replay.Errors);

        // ...and revoke the entire family (defensive) — no active tokens remain.
        var anyActive = await ctx.RefreshTokens
            .AnyAsync(rt => rt.UserId == user.UserId && rt.RevokedAt == null);
        Assert.False(anyActive);
    }

    [Fact]
    public async Task Refresh_UnknownToken_ReturnsInvalid()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);

        var result = await svc.RefreshTokenAsync("this-token-was-never-issued");

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid refresh token", result.Errors);
    }

    [Fact]
    public async Task Refresh_EmptyToken_ReturnsRequired()
    {
        var svc = NewService(out _);

        var result = await svc.RefreshTokenAsync("");

        Assert.False(result.IsSuccess);
        Assert.Contains("Refresh token is required", result.Errors);
    }

    [Fact]
    public async Task Refresh_ExpiredToken_ReturnsExpired()
    {
        var svc = NewService(out var ctx);
        var user = SeedUser(ctx);

        const string raw = "expired-raw-token";
        ctx.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.UserId,
            TokenHash = Hash(raw),
            CreatedAt = DateTime.UtcNow.AddDays(-8),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1), // in the past, not revoked
        });
        await ctx.SaveChangesAsync();

        var result = await svc.RefreshTokenAsync(raw);

        Assert.False(result.IsSuccess);
        Assert.Contains("Refresh token has expired", result.Errors);
    }

    [Fact]
    public async Task Refresh_InactiveUser_IsRejected()
    {
        var svc = NewService(out var ctx);
        var user = SeedUser(ctx, active: true);
        var original = await LoginAndGetRefreshToken(svc);

        // Deactivate after issuing the token.
        user.IsActive = false;
        ctx.Users.Update(user);
        await ctx.SaveChangesAsync();

        var result = await svc.RefreshTokenAsync(original);

        Assert.False(result.IsSuccess);
        Assert.Contains("User is no longer active", result.Errors);
    }
}
