using Microsoft.AspNetCore.Mvc;

namespace dotnet_rest_api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DebugController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public DebugController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("config")]
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
}
