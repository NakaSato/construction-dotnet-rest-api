using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProjectsController : BaseApiController
{
    private readonly IProjectService _projectService;
    private readonly IQueryService _queryService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, IQueryService queryService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Get all projects with advanced filtering, sorting, and field selection
    /// Available to: All authenticated users (view projects)
    /// </summary>
    [HttpGet]
    [MediumCache] // 15 minute cache for project lists
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<ProjectDto>>>> GetProjects([FromQuery] ProjectQueryParameters parameters)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "GetProjects", parameters);

        // Parse dynamic filters from query string using the base controller method
        var filterString = Request.Query["filter"].FirstOrDefault();
        ApplyFiltersFromQuery(parameters, filterString);

        var result = await _projectService.GetProjectsAsync(parameters);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Get all projects with pagination (legacy endpoint for backward compatibility)
    /// </summary>
    [HttpGet("legacy")]
    [MediumCache] // 15 minute cache for legacy project lists
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjectsLegacy(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? managerId = null)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "GetProjectsLegacy", new { pageNumber, pageSize, managerId });

        var result = await _projectService.GetProjectsLegacyAsync(pageNumber, pageSize, managerId);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [LongCache] // 1 hour cache for individual project details
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "GetProject", new { id });

        var result = await _projectService.GetProjectByIdAsync(id);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Create a new project
    /// Available to: Administrator, ProjectManager (can manage projects)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "CreateProject", request);

        if (!ModelState.IsValid)
        {
            return CreateErrorResponse("Invalid input data", 400);
        }

        var result = await _projectService.CreateProjectAsync(request);
        
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.ProjectId }, 
                ToApiResponse(result).Value);
        }

        return ToApiResponse(result);
    }

    /// <summary>
    /// Update an existing project
    /// Available to: Administrator, ProjectManager (can manage projects)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "UpdateProject", new { id, request });

        if (!ModelState.IsValid)
        {
            return CreateErrorResponse("Invalid input data", 400);
        }

        var result = await _projectService.UpdateProjectAsync(id, request);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Partially update a project
    /// Available to: Administrator, ProjectManager (can manage projects)
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> PatchProject(Guid id, [FromBody] PatchProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "PatchProject", new { id, request });

        if (!ModelState.IsValid)
        {
            return CreateErrorResponse("Invalid input data", 400);
        }

        var result = await _projectService.PatchProjectAsync(id, request);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Delete a project
    /// Available to: Administrator only (full system access)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(Guid id)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "DeleteProject", new { id });

        var result = await _projectService.DeleteProjectAsync(id);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Get projects for the current user
    /// </summary>
    [HttpGet("me")]
    [ShortCache] // 5 minute cache for user-specific projects
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetMyProjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "GetMyProjects", new { pageNumber, pageSize });

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return CreateErrorResponse("Invalid user ID in token", 401);
        }

        var result = await _projectService.GetUserProjectsAsync(userId, pageNumber, pageSize);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Get all projects with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("rich")]
    [MediumCache] // 15 minute cache for rich project pagination
    public async Task<ActionResult<ApiResponseWithPagination<ProjectDto>>> GetProjectsWithRichPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? managerId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetProjectsWithRichPagination", new { page, pageSize, managerId, status, sortBy, sortOrder });

            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(page, pageSize);
            if (validationResult != null)
                return validationResult;

            // Create query parameters
            var parameters = new ProjectQueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                ManagerId = managerId,
                Status = status,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Get data using existing service
            var serviceResult = await _projectService.GetProjectsAsync(parameters);
            if (!serviceResult.IsSuccess)
                return BadRequest(serviceResult.Message);

            // Build base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (managerId.HasValue) queryParams.Add("managerId", managerId.Value.ToString());
            if (!string.IsNullOrEmpty(status)) queryParams.Add("status", status);
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add("sortBy", sortBy);
            if (!string.IsNullOrEmpty(sortOrder)) queryParams.Add("sortOrder", sortOrder);

            // Create rich paginated response using QueryService
            var response = _queryService.CreateRichPaginatedResponse(
                serviceResult.Data!.Items,
                serviceResult.Data.TotalCount,
                page,
                pageSize,
                baseUrl,
                queryParams,
                "Projects retrieved successfully"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving projects with rich pagination");
        }
    }
}
