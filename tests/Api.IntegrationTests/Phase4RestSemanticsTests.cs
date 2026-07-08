using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// Coverage for the Phase 4 REST cleanups (docs/API_DESIGN_REVIEW.md):
/// test endpoints removed, workflow actions moved to POST (with PATCH alias),
/// and creates returning 201.
/// </summary>
public class Phase4RestSemanticsTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private const string AdminEmail = "admin@solarprojects.com";

    public Phase4RestSemanticsTests(ApiFactory factory) => _factory = factory;

    // ---- item 5: test endpoints removed from the v1 surface ----------------

    [Theory]
    [InlineData("/api/v1/projects/test")]          // was [AllowAnonymous] GET returning sample data
    public async Task Removed_Get_Endpoints_Return_404(string path)
    {
        var res = await _factory.CreateClient().GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Theory]
    [InlineData("/api/v1/notifications/test")]
    [InlineData("/api/v1/notifications/test-signalr")]
    public async Task Removed_Post_Endpoints_Return_404(string path)
    {
        var res = await _factory.CreateClient().PostAsync(path, JsonContent.Create(new { }));
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    // ---- item 3: activate/deactivate are POST, with PATCH kept as alias ----
    // A matched-but-unauthenticated endpoint returns 401; a missing route returns 404.
    // So 401 here proves the route exists for that verb.

    [Theory]
    [InlineData("POST", "activate")]
    [InlineData("POST", "deactivate")]
    [InlineData("PATCH", "activate")]   // deprecated alias still routes
    [InlineData("PATCH", "deactivate")]
    public async Task User_Workflow_Routes_Exist_For_Verb(string method, string action)
    {
        var id = Guid.Empty;
        var req = new HttpRequestMessage(new HttpMethod(method), $"/api/v1/users/{id}/{action}");
        var res = await _factory.CreateClient().SendAsync(req);
        Assert.Equal(HttpStatusCode.Unauthorized, res.StatusCode);
    }

    [Fact]
    public async Task User_Activate_Does_Not_Accept_Get()
    {
        // Route exists for POST/PATCH only, so GET is rejected as 405 (not 404).
        var res = await _factory.CreateClient().GetAsync($"/api/v1/users/{Guid.Empty}/activate");
        Assert.Equal(HttpStatusCode.MethodNotAllowed, res.StatusCode);
    }

    // ---- item 1: creates return 201 --------------------------------------

    [Fact]
    public async Task Create_Project_Returns_201()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await _factory.GetTokenAsync(AdminEmail));

        var res = await client.PostAsJsonAsync("/api/v1/projects", new
        {
            projectName = "Integration Test Project",
            address = "123 Integration Test Street, Test City",
            startDate = "2026-01-01T00:00:00Z"
        });

        Assert.Equal(HttpStatusCode.Created, res.StatusCode);
    }
}
