using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Models;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/weekly-requests")]
[Authorize]
public class WeeklyWorkRequestsController : BaseApiController
{
    private readonly IWeeklyWorkRequestService _weeklyWorkRequestService;
    private readonly ILogger<WeeklyWorkRequestsController> _logger;

    public WeeklyWorkRequestsController(
        IWeeklyWorkRequestService weeklyWorkRequestService, 
        ILogger<WeeklyWorkRequestsController> logger)
    {
        _weeklyWorkRequestService = weeklyWorkRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get weekly work request by ID
    /// Available to: All authenticated users (view assigned work)
    /// </summary>
    [HttpGet("{requestId:guid}")]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> GetWeeklyWorkRequest(Guid requestId)
    {
        LogControllerAction(_logger, "GetWeeklyWorkRequest", requestId);

        var result = await _weeklyWorkRequestService.GetWeeklyWorkRequestByIdAsync(requestId);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Get all weekly work requests with advanced filtering and pagination
    /// Available to: All authenticated users (view work requests)
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>>> GetWeeklyWorkRequests(
        [FromQuery] WeeklyWorkRequestQueryParameters parameters)
    {
        LogControllerAction(_logger, "GetWeeklyWorkRequests", parameters);

        // Parse dynamic filters from query string using the base controller method
        var filterString = Request.Query["filter"].FirstOrDefault();
        ApplyFiltersFromQuery(parameters, filterString);

        var result = await _weeklyWorkRequestService.GetWeeklyWorkRequestsAsync(parameters);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Create a new weekly work request
    /// Available to: Administrator, ProjectManager, Planner (planning responsibilities)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Planner")]
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> CreateWeeklyWorkRequest(
        [FromBody] CreateWeeklyWorkRequestDto request)
    {
        LogControllerAction(_logger, "CreateWeeklyWorkRequest", request);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _weeklyWorkRequestService.CreateWeeklyWorkRequestAsync(request);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Update an existing weekly work request
    /// Available to: Administrator, ProjectManager, Planner (can manage work requests)
    /// </summary>
    [HttpPut("{requestId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager,Planner")]
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> UpdateWeeklyWorkRequest(
        Guid requestId, [FromBody] UpdateWeeklyWorkRequestDto request)
    {
        LogControllerAction(_logger, "UpdateWeeklyWorkRequest", new { requestId, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _weeklyWorkRequestService.UpdateWeeklyWorkRequestAsync(requestId, request);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Submit weekly work request for approval
    /// Available to: Administrator, ProjectManager, Planner (can submit requests)
    /// </summary>
    [HttpPost("{requestId:guid}/submit")]
    [Authorize(Roles = "Administrator,ProjectManager,Planner")]
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> SubmitWeeklyWorkRequest(Guid requestId)
    {
        LogControllerAction(_logger, "SubmitWeeklyWorkRequest", requestId);

        var result = await _weeklyWorkRequestService.UpdateWeeklyWorkRequestStatusAsync(
            requestId, WeeklyRequestStatus.Submitted);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Approve weekly work request
    /// Available to: Administrator, ProjectManager (approval authority)
    /// </summary>
    [HttpPost("{requestId:guid}/approve")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> ApproveWeeklyWorkRequest(Guid requestId)
    {
        LogControllerAction(_logger, "ApproveWeeklyWorkRequest", requestId);

        var result = await _weeklyWorkRequestService.UpdateWeeklyWorkRequestStatusAsync(
            requestId, WeeklyRequestStatus.Approved);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Delete weekly work request
    /// Available to: Administrator only (full system access)
    /// </summary>
    [HttpDelete("{requestId:guid}")]
    [Authorize(Roles = "Administrator")]
    [CriticalDeleteRateLimit]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWeeklyWorkRequest(Guid requestId)
    {
        LogControllerAction(_logger, "DeleteWeeklyWorkRequest", requestId);

        var result = await _weeklyWorkRequestService.DeleteWeeklyWorkRequestAsync(requestId);
        return ToApiResponse(result);
    }
}

/// <summary>
/// Project-specific weekly work requests endpoints
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/projects/{projectId:guid}/weekly-requests")]
[Authorize]
public class ProjectWeeklyWorkRequestsController : BaseApiController
{
    private readonly IWeeklyWorkRequestService _weeklyWorkRequestService;
    private readonly ILogger<ProjectWeeklyWorkRequestsController> _logger;

    public ProjectWeeklyWorkRequestsController(
        IWeeklyWorkRequestService weeklyWorkRequestService, 
        ILogger<ProjectWeeklyWorkRequestsController> logger)
    {
        _weeklyWorkRequestService = weeklyWorkRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get weekly work requests for a specific project
    /// Available to: All authenticated users (view project work)
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache
    [Authorize] // All authenticated users can view
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WeeklyWorkRequestDto>>>> GetProjectWeeklyWorkRequests(
        Guid projectId, [FromQuery] WeeklyWorkRequestQueryParameters parameters)
    {
        LogControllerAction(_logger, "GetProjectWeeklyWorkRequests", new { projectId, parameters });

        // Parse dynamic filters from query string using the base controller method
        var filterString = Request.Query["filter"].FirstOrDefault();
        ApplyFiltersFromQuery(parameters, filterString);

        var result = await _weeklyWorkRequestService.GetProjectWeeklyWorkRequestsAsync(projectId, parameters);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Create a new weekly work request for a specific project
    /// Available to: Administrator, ProjectManager, Planner (planning responsibilities)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Planner")]
    public async Task<ActionResult<ApiResponse<WeeklyWorkRequestDto>>> CreateProjectWeeklyWorkRequest(
        Guid projectId, [FromBody] CreateWeeklyWorkRequestDto request)
    {
        LogControllerAction(_logger, "CreateProjectWeeklyWorkRequest", new { projectId, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Ensure the project ID matches the route parameter
        request.ProjectId = projectId;

        var result = await _weeklyWorkRequestService.CreateWeeklyWorkRequestAsync(request);
        return ToApiResponse(result);
    }
}
