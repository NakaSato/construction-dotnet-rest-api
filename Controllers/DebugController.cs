using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;

namespace dotnet_rest_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;

    public DebugController(IConfiguration configuration, ICacheService cacheService)
    {
        _configuration = configuration;
        _cacheService = cacheService;
    }

    [HttpGet("config")]
    [NoCache] // No caching for debug endpoints
    public ActionResult GetConfig()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        
        return Ok(new
        {
            Environment = environment,
            ConnectionString = connectionString,
            AllConnectionStrings = _configuration.GetSection("ConnectionStrings").GetChildren()
                .ToDictionary(x => x.Key, x => x.Value)
        });
    }

    [HttpGet("cache-stats")]
    [NoCache] // No caching for debug endpoints
    public async Task<ActionResult> GetCacheStats()
    {
        try
        {
            var cacheConfig = _configuration.GetSection("Caching");
            var isDistributedCacheEnabled = cacheConfig.GetValue<bool>("EnableDistributedCache");
            var redisConnection = _configuration.GetConnectionString("Redis");

            return Ok(new
            {
                CacheConfiguration = new
                {
                    DefaultMemoryCacheDuration = cacheConfig.GetValue<int>("DefaultMemoryCacheDurationMinutes"),
                    DefaultDistributedCacheDuration = cacheConfig.GetValue<int>("DefaultDistributedCacheDurationMinutes"),
                    IsDistributedCacheEnabled = isDistributedCacheEnabled,
                    HasRedisConnection = !string.IsNullOrEmpty(redisConnection),
                    CacheKeyPrefix = cacheConfig.GetValue<string>("CacheKeyPrefix")
                },
                CacheProfiles = cacheConfig.GetSection("Profiles").GetChildren()
                    .ToDictionary(x => x.Key, x => new
                    {
                        DurationMinutes = x.GetValue<int>("DurationMinutes"),
                        SlidingExpiration = x.GetValue<bool>("SlidingExpiration")
                    }),
                SystemInfo = new
                {
                    Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"),
                    MachineName = Environment.MachineName,
                    ProcessorCount = Environment.ProcessorCount,
                    WorkingSet = Environment.WorkingSet,
                    Timestamp = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message });
        }
    }
}
