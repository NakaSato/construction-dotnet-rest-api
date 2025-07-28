using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Services.Users;
using dotnet_rest_api.Services.Tasks;
using dotnet_rest_api.Services.Projects;
using dotnet_rest_api.Services.MasterPlans;
using dotnet_rest_api.Services.WBS;
using dotnet_rest_api.Services.Infrastructure;
using dotnet_rest_api.Attributes;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Top-level resources controller for construction resources (personnel, equipment)
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resources")]
[Authorize]
public class ResourcesController : BaseApiController
{
    private readonly IResourceService _resourceService;
    private readonly IQueryService _queryService;
    private readonly ILogger<ResourcesController> _logger;

    public ResourcesController(
        IResourceService resourceService,
        IQueryService queryService,
        ILogger<ResourcesController> logger)
    {
        _resourceService = resourceService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all construction resources with filtering (top-level collection)
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<ResourceDto>>>> GetResources([FromQuery] ResourceQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetResources", parameters);

            // Apply dynamic filters from query string
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _resourceService.GetResourcesAsync(parameters);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<ResourceDto>>(_logger, ex, "retrieving resources");
        }
    }

    /// <summary>
    /// Get a specific resource by ID (canonical resource)
    /// </summary>
    [HttpGet("{resourceId:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<ResourceDto>>> GetResource(Guid resourceId)
    {
        try
        {
            LogControllerAction(_logger, "GetResource", new { resourceId });

            var result = await _resourceService.GetResourceByIdAsync(resourceId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ResourceDto>(_logger, ex, "retrieving resource");
        }
    }

    /// <summary>
    /// Create a new construction resource
    /// Available to: Administrator, ProjectManager
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ResourceDto>>> CreateResource([FromBody] CreateResourceRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateResource", request);

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ResourceDto> { Success = false, Message = "Invalid input data" });

            var result = await _resourceService.CreateResourceAsync(request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ResourceDto>(_logger, ex, "creating resource");
        }
    }

    /// <summary>
    /// Update a resource (canonical resource)
    /// Available to: Administrator, ProjectManager
    /// </summary>
    [HttpPatch("{resourceId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ResourceDto>>> UpdateResource(Guid resourceId, [FromBody] UpdateResourceRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdateResource", new { resourceId, request });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<ResourceDto> { Success = false, Message = "Invalid input data" });

            var result = await _resourceService.UpdateResourceAsync(resourceId, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ResourceDto>(_logger, ex, "updating resource");
        }
    }

    /// <summary>
    /// Delete a resource (canonical resource)
    /// Available to: Administrator
    /// </summary>
    [HttpDelete("{resourceId:guid}")]
    [Authorize(Roles = "Administrator")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteResource(Guid resourceId)
    {
        try
        {
            LogControllerAction(_logger, "DeleteResource", new { resourceId });

            var result = await _resourceService.DeleteResourceAsync(resourceId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "deleting resource");
        }
    }
}
