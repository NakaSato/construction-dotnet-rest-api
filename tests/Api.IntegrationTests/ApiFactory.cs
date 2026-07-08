using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace Api.IntegrationTests;

/// <summary>
/// Hosts the API with the in-memory database and a deterministic JWT key.
///
/// Program.cs reads USE_IN_MEMORY_DB and JWT_KEY via raw
/// Environment.GetEnvironmentVariable at the very top of its top-level statements,
/// which executes before ConfigureWebHost callbacks. So these must be set in the
/// factory constructor — before the first CreateClient() triggers host startup.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private const string TestJwtKey = "IntegrationTestSecretKeyNotForProduction1234567890";

    public ApiFactory()
    {
        Environment.SetEnvironmentVariable("USE_IN_MEMORY_DB", "true");
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        // JWT_KEY drives Program's token validation; Jwt__Key (=> config "Jwt:Key")
        // drives AuthService token generation. Both must be the same non-empty value —
        // appsettings.json ships "Jwt:Key": "" and only Development overrides it.
        Environment.SetEnvironmentVariable("JWT_KEY", TestJwtKey);
        Environment.SetEnvironmentVariable("Jwt__Key", TestJwtKey);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
    }

    /// <summary>Logs in and returns a bearer token. Seeded users all use password "Admin123!".</summary>
    public async Task<string> GetTokenAsync(string username, string password = "Admin123!")
    {
        var client = CreateClient();
        var res = await client.PostAsJsonAsync("/api/v1/auth/login", new { username, password });
        res.EnsureSuccessStatusCode();
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        return doc.RootElement.GetProperty("data").GetProperty("token").GetString() ?? "";
    }
}
