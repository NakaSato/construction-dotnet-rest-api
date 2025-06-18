// Example of applying rate limiting to DELETE endpoints

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[Route("api/v1/[controller]")]
public class ExampleController : BaseApiController
{
    /// <summary>
    /// Standard DELETE operation with moderate rate limiting
    /// </summary>
    [HttpDelete("{id:guid}")]
    [DeleteRateLimit] // 10 requests per minute
    public async Task<ActionResult<ApiResponse<bool>>> DeleteResource(Guid id)
    {
        // Implementation here
        return CreateSuccessResponse(true, "Resource deleted successfully");
    }

    /// <summary>
    /// Critical DELETE operation with strict rate limiting
    /// Examples: Deleting projects, users, or other critical data
    /// </summary>
    [HttpDelete("critical/{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [CriticalDeleteRateLimit] // 3 requests per 5 minutes
    public async Task<ActionResult<ApiResponse<bool>>> DeleteCriticalResource(Guid id)
    {
        // Implementation here
        return CreateSuccessResponse(true, "Critical resource deleted successfully");
    }

    /// <summary>
    /// Custom rate limiting for specific DELETE operations
    /// </summary>
    [HttpDelete("bulk")]
    [RateLimit(2, 10)] // 2 requests per 10 minutes
    public async Task<ActionResult<ApiResponse<bool>>> BulkDelete([FromBody] List<Guid> ids)
    {
        // Implementation here
        return CreateSuccessResponse(true, "Bulk delete completed successfully");
    }
}
