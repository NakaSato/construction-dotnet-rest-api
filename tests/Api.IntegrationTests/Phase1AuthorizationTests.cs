using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// Regression coverage for the Phase 1 security &amp; authorization fixes
/// (docs/API_DESIGN_REVIEW.md): DebugController removal, CalendarController auth,
/// and the role-string collapse to the seeded Admin/Manager/User/Viewer set.
/// </summary>
public class Phase1AuthorizationTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Seeded users (ApplicationDbContext.HasData), all password "Admin123!".
    private const string AdminEmail = "admin@solarprojects.com";     // RoleId 1 = Admin
    private const string UserEmail = "engineer@solarprojects.com";   // RoleId 3 = User
    private const string Password = "Admin123!";

    public Phase1AuthorizationTests(ApiFactory factory) => _factory = factory;

    // ---- P0-1: DebugController deleted -------------------------------------

    [Theory]
    [InlineData("/api/debug/config")]
    [InlineData("/api/debug/database")]
    [InlineData("/api/debug/database-info")]
    [InlineData("/api/debug/cache-stats")]
    public async Task Debug_Get_Endpoints_Are_Gone(string path)
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task Debug_MigrateDatabase_Is_Gone()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsync("/api/debug/migrate-database", null);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ---- P1-1: CalendarController no longer anonymous ----------------------

    [Fact]
    public async Task Calendar_Read_Requires_Authentication()
    {
        var client = _factory.CreateClient();
        var res = await client.GetAsync("/api/v1/calendar");
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task Calendar_Write_Anonymous_Is_Unauthorized()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsync("/api/v1/calendar",
            JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ---- P0-2: role gates now match the seeded role set --------------------

    [Fact]
    public async Task Admin_Login_Returns_Token()
    {
        var token = await LoginAsync(AdminEmail, Password);
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    // Endpoints whose [Authorize(Roles=...)] previously referenced non-existent
    // roles (Administrator/ProjectManager/...) and returned 403 for everyone.
    // After the fix an Admin must get PAST authorization (anything but 403/401).
    [Theory]
    [InlineData("/api/v1/master-plans")]
    [InlineData("/api/v1/documents")]
    [InlineData("/api/v1/resources")]
    [InlineData("/api/v1/calendar")]
    [InlineData("/api/v1/weekly-reports")]
    public async Task Admin_Passes_Authorization_On_Previously_Broken_Gates(string path)
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(AdminEmail, Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PostAsync(path, JsonContent.Create(new { }));

        Assert.NotEqual(HttpStatusCode.Forbidden, res.StatusCode);
        Assert.NotEqual(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task PlainUser_Is_Forbidden_On_Manager_Gated_Write()
    {
        var client = _factory.CreateClient();
        var token = await LoginAsync(UserEmail, Password);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var res = await client.PostAsync("/api/v1/calendar", JsonContent.Create(new { }));

        Assert.Equal(HttpStatusCode.Forbidden, res.StatusCode);
    }

    [Fact]
    public async Task Unauthenticated_Admin_Write_Is_Unauthorized()
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsync("/api/v1/users", JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    // ---- helper ------------------------------------------------------------

    private async Task<string> LoginAsync(string username, string password)
    {
        var client = _factory.CreateClient();
        var res = await client.PostAsJsonAsync("/api/v1/auth/login",
            new { username, password });
        var body = await res.Content.ReadAsStringAsync();
        if (!res.IsSuccessStatusCode)
            throw new Xunit.Sdk.XunitException($"login {username} -> {(int)res.StatusCode}: {body}");

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("data").GetProperty("token").GetString() ?? "";
    }
}
