using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// Happy-path smoke coverage: every V1 controller with a no-parameter collection
/// GET returns 2xx and the standard ApiResponse envelope (success=true) for an
/// authenticated Admin. Guards the controller -> service -> DbContext wiring and
/// the response-envelope contract against regressions during refactors.
/// </summary>
public class ControllerSmokeTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private const string AdminEmail = "admin@example.com";

    public ControllerSmokeTests(ApiFactory factory) => _factory = factory;

    // Collection GET routes that require no path parameters.
    [Theory]
    [InlineData("/api/v1/calendar")]
    [InlineData("/api/v1/daily-reports")]
    [InlineData("/api/v1/dashboard/overview")]
    // NOTE: /api/v1/master-plans has no controller (MasterPlansController is retired);
    // the MasterPlan feature is exposed only via PhasesController today.
    [InlineData("/api/v1/notifications")]
    [InlineData("/api/v1/projects")]
    [InlineData("/api/v1/tasks")]
    [InlineData("/api/v1/users")]
    [InlineData("/api/v1/wbs")]
    [InlineData("/api/v1/weekly-reports")]
    [InlineData("/api/v1/weekly-requests")]
    [InlineData("/api/v1/work-requests")]
    public async Task Collection_Get_Returns_Success_Envelope_For_Admin(string path)
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await _factory.GetTokenAsync(AdminEmail));

        var res = await client.GetAsync(path);

        Assert.True(res.IsSuccessStatusCode,
            $"GET {path} -> {(int)res.StatusCode}: {await res.Content.ReadAsStringAsync()}");

        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        Assert.True(doc.RootElement.TryGetProperty("success", out var success),
            $"GET {path} response has no 'success' property");
        Assert.True(success.GetBoolean(), $"GET {path} returned success=false");
    }

    [Fact]
    public async Task Health_Endpoint_Is_Healthy()
    {
        var res = await _factory.CreateClient().GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }
}
