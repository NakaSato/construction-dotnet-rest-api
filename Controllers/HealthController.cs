using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Data;

namespace dotnet_rest_api.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var healthCheck = new
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        };

        _logger.LogInformation("Health check requested at {Timestamp}", healthCheck.Timestamp);
        
        return Ok(healthCheck);
    }

    [HttpGet("detailed")]
    public async Task<IActionResult> GetDetailed([FromServices] ApplicationDbContext context)
    {
        try
        {
            // Test database connectivity
            var canConnect = await context.Database.CanConnectAsync();
            
            var detailedHealth = new
            {
                Status = canConnect ? "Healthy" : "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                Database = new
                {
                    Status = canConnect ? "Connected" : "Disconnected",
                    Provider = context.Database.ProviderName
                },
                Memory = new
                {
                    WorkingSet = GC.GetTotalMemory(false),
                    Gen0Collections = GC.CollectionCount(0),
                    Gen1Collections = GC.CollectionCount(1),
                    Gen2Collections = GC.CollectionCount(2)
                }
            };

            return canConnect ? Ok(detailedHealth) : StatusCode(503, detailedHealth);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            
            var errorHealth = new
            {
                Status = "Unhealthy",
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            };
            
            return StatusCode(503, errorHealth);
        }
    }
}
