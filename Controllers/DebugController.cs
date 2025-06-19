using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Data;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly ApplicationDbContext _context;

    public DebugController(IConfiguration configuration, ICacheService cacheService, ApplicationDbContext context)
    {
        _configuration = configuration;
        _cacheService = cacheService;
        _context = context;
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
    public ActionResult GetCacheStats()
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

    [HttpGet("database")]
    [NoCache] // No caching for debug endpoints
    public async Task<ActionResult> GetDatabaseStatus()
    {
        try
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            // Test database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            
            // Get database info
            var dbInfo = new
            {
                CanConnect = canConnect,
                DatabaseProvider = _context.Database.ProviderName,
                ConnectionString = connectionString?.Substring(0, Math.Min(50, connectionString.Length)) + "...",
                Environment = environment
            };

            if (canConnect)
            {
                // Test basic operations
                var projectCount = await _context.Projects.CountAsync();
                var userCount = await _context.Users.CountAsync();
                
                return Ok(new
                {
                    Status = "Connected",
                    DatabaseInfo = dbInfo,
                    TableCounts = new
                    {
                        Projects = projectCount,
                        Users = userCount
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return Ok(new
                {
                    Status = "Cannot Connect",
                    DatabaseInfo = dbInfo,
                    Timestamp = DateTime.UtcNow
                });
            }
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                Status = "Error",
                Error = ex.Message,
                InnerError = ex.InnerException?.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
