using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Api.IntegrationTests;

/// <summary>
/// Guards the Calendar layering refactor: CalendarService now returns Result&lt;T&gt;
/// and CalendarController maps it via BaseApiController (ToApiResponse/ToCreatedResponse).
/// Verifies the ApiResponse envelope shape, 201-on-create and 404-on-missing survive.
/// </summary>
public class CalendarTests : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory;
    private const string AdminEmail = "admin@example.com";

    public CalendarTests(ApiFactory factory) => _factory = factory;

    private async Task<HttpClient> AdminClientAsync()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", await _factory.GetTokenAsync(AdminEmail));
        return client;
    }

    [Fact]
    public async Task Create_Then_Get_Then_Delete_RoundTrip()
    {
        var client = await AdminClientAsync();

        var create = await client.PostAsJsonAsync("/api/v1/calendar", new
        {
            title = "Refactor smoke event",
            startDateTime = "2026-08-01T09:00:00Z",
            endDateTime = "2026-08-01T10:00:00Z",
            eventType = 1,
            status = 1,
            priority = 1
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        using var createdDoc = JsonDocument.Parse(await create.Content.ReadAsStringAsync());
        Assert.True(createdDoc.RootElement.GetProperty("success").GetBoolean());
        var id = createdDoc.RootElement.GetProperty("data").GetProperty("id").GetGuid();

        var get = await client.GetAsync($"/api/v1/calendar/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);

        var del = await client.DeleteAsync($"/api/v1/calendar/{id}");
        Assert.Equal(HttpStatusCode.OK, del.StatusCode);
    }

    [Fact]
    public async Task Get_Missing_Event_Returns_404_Envelope()
    {
        var client = await AdminClientAsync();

        var res = await client.GetAsync($"/api/v1/calendar/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
        Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
    }
}
