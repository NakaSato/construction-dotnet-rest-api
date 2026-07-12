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
/// Covers <see cref="AuthService"/> login brute-force lockout, registration
/// duplicate guards, and JWT revoke/blacklist round-trip. Each test uses an
/// isolated in-memory database and cache.
/// </summary>
public class AuthServiceTests
{
    private const string Password = "Secret123!";

    private static AuthService NewService(out ApplicationDbContext ctx)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"auth-{Guid.NewGuid()}")
            .Options;
        ctx = new ApplicationDbContext(options);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "unit-test-signing-key-that-is-at-least-32-bytes-long!!",
                ["Jwt:Issuer"] = "SolarProjectsAPI",
                ["Jwt:Audience"] = "SolarProjectsClient",
            })
            .Build();

        return new AuthService(ctx, config, new MemoryCache(new MemoryCacheOptions()),
            NullLogger<AuthService>.Instance);
    }

    // User -> Role is required and AuthService Include()s it on login; under the EF
    // in-memory provider the user vanishes from the query if the Role row is absent.
    private static void SeedUser(ApplicationDbContext ctx, string username = "alice")
    {
        if (!ctx.Roles.Any(r => r.RoleId == 3))
            ctx.Roles.Add(new Role { RoleId = 3, RoleName = "User" });

        ctx.Users.Add(new User
        {
            UserId = Guid.NewGuid(),
            Username = username,
            Email = $"{username}@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password),
            FullName = username,
            RoleId = 3,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        });
        ctx.SaveChanges();
    }

    private static LoginRequest Login(string user, string pass) => new() { Username = user, Password = pass };

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalid()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);

        var result = await svc.LoginAsync(Login("alice", "wrong"));

        Assert.False(result.IsSuccess);
        Assert.Contains("Invalid username or password", result.Errors);
    }

    [Fact]
    public async Task Login_CorrectPassword_Succeeds_AndIssuesTokens()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);

        var result = await svc.LoginAsync(Login("alice", Password));

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.Token));
        Assert.False(string.IsNullOrWhiteSpace(result.Data!.RefreshToken));
    }

    [Fact]
    public async Task Login_LockoutAfter5Failures_BlocksEvenCorrectPassword()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);

        // 5 failed attempts arm the lockout (MaxFailedLoginAttempts = 5).
        for (var i = 0; i < 5; i++)
            Assert.False((await svc.LoginAsync(Login("alice", "wrong"))).IsSuccess);

        // 6th attempt is blocked even with the correct password.
        var blocked = await svc.LoginAsync(Login("alice", Password));

        Assert.False(blocked.IsSuccess);
        Assert.Contains(blocked.Errors, e => e.Contains("locked"));
    }

    [Fact]
    public async Task Login_SuccessBeforeThreshold_StillWorks()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);

        // 4 failures stay below the threshold, so the correct password still logs in.
        for (var i = 0; i < 4; i++)
            await svc.LoginAsync(Login("alice", "wrong"));

        var ok = await svc.LoginAsync(Login("alice", Password));

        Assert.True(ok.IsSuccess);
    }

    [Fact]
    public async Task Register_NewUser_Succeeds()
    {
        var svc = NewService(out _);

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            Username = "bob",
            Email = "bob@example.com",
            Password = Password,
            FullName = "Bob",
            RoleId = 0, // defaults to User (3)
        });

        Assert.True(result.IsSuccess);
        Assert.Equal("bob", result.Data!.Username);
    }

    [Fact]
    public async Task Register_DuplicateUsername_Fails()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx, "alice");

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            Username = "alice",              // taken
            Email = "different@example.com",
            Password = Password,
            FullName = "Alice II",
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Username or email already exists", result.Errors);
    }

    [Fact]
    public async Task Register_DuplicateEmail_Fails()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx, "alice"); // email alice@example.com

        var result = await svc.RegisterAsync(new RegisterRequest
        {
            Username = "someoneelse",
            Email = "alice@example.com",     // taken
            Password = Password,
            FullName = "Someone",
        });

        Assert.False(result.IsSuccess);
        Assert.Contains("Username or email already exists", result.Errors);
    }

    [Fact]
    public async Task Revoke_ThenBlacklistCheck_ReturnsTrue()
    {
        var svc = NewService(out var ctx);
        SeedUser(ctx);
        var jwt = (await svc.LoginAsync(Login("alice", Password))).Data!.Token!;

        Assert.False((await svc.IsTokenBlacklistedAsync(jwt)).Data); // not blacklisted yet

        var revoke = await svc.RevokeTokenAsync(jwt);
        Assert.True(revoke.IsSuccess);

        var check = await svc.IsTokenBlacklistedAsync(jwt);
        Assert.True(check.Data);
    }

    [Fact]
    public async Task Revoke_MalformedToken_ReturnsError()
    {
        var svc = NewService(out _);

        var result = await svc.RevokeTokenAsync("not-a-jwt");

        Assert.False(result.IsSuccess);
    }
}
