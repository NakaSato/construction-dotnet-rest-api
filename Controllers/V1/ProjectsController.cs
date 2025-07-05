using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] // Enable authorization for all endpoints by default
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
    [Authorize(Roles = "Admin,Manager")] // Enable role-based authorization for creating projects
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "CreateProject", request);

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProjectDto> { Success = false, Message = "Invalid input data" });
        }

        // Get user information for real-time notifications
        var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

        var result = await _projectService.CreateProjectAsync(request, userName);
        
        if (result.IsSuccess)
        {
            return StatusCode(201, CreateSuccessResponse(result.Data!, "Project created successfully"));
        }

        return ToApiResponse(result);
    }

    /// <summary>
    /// Update an existing project
    /// Available to: Administrator, ProjectManager (can manage projects)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "UpdateProject", new { id, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProjectDto> { Success = false, Message = "Invalid input data" });
        }

        // Get user information for real-time notifications
        var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

        var result = await _projectService.UpdateProjectAsync(id, request, userName);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Partially update a project
    /// Available to: Administrator, ProjectManager (can manage projects)
    /// </summary>
    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<ProjectDto>>> PatchProject(Guid id, [FromBody] PatchProjectRequest request)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "PatchProject", new { id, request });

        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProjectDto> { Success = false, Message = "Invalid input data" });
        }

        // Get user information for real-time notifications
        var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

        var result = await _projectService.PatchProjectAsync(id, request, userName);
        return ToApiResponse(result);
    }

    /// <summary>
    /// Delete a project
    /// Available to: Administrator only (full system access)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")] // Enable admin-only authorization for deleting projects
    [CriticalDeleteRateLimit]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(Guid id)
    {
        // Log controller action for debugging
        LogControllerAction(_logger, "DeleteProject", new { id });

        // Get user information for real-time notifications
        var userName = User.Identity?.Name ?? User.FindFirst(ClaimTypes.Name)?.Value ?? "Unknown User";

        var result = await _projectService.DeleteProjectAsync(id, userName);
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
            return BadRequest(new ApiResponse<PagedResult<ProjectDto>> { Success = false, Message = "Invalid user ID in token" });
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
                return BadRequest(CreateErrorResponse(validationResult));
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (managerId.HasValue) queryParams.Add("managerId", managerId.Value.ToString());
            if (!string.IsNullOrEmpty(status)) queryParams.Add("status", status);
            if (!string.IsNullOrEmpty(sortBy)) queryParams.Add("sortBy", sortBy);
            if (!string.IsNullOrEmpty(sortOrder)) queryParams.Add("sortOrder", sortOrder);

            // Get projects from service
            var parameters = new ProjectQueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                ManagerId = managerId,
                Status = status,
                SortBy = sortBy,
                SortOrder = sortOrder
            };
            var serviceResult = await _projectService.GetProjectsAsync(parameters);
            if (!serviceResult.Success)
                return BadRequest(new ApiResponseWithPagination<ProjectDto> { Success = false, Message = serviceResult.Message });

            // Create base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";

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
            _logger.LogError(ex, "Error retrieving projects with rich pagination");
            return StatusCode(500, new ApiResponseWithPagination<ProjectDto> 
            { 
                Success = false, 
                Message = "An internal error occurred",
                Data = new ApiDataWithPagination<ProjectDto> 
                { 
                    Items = new List<ProjectDto>(),
                    Pagination = new PaginationInfo()
                }
            });
        }
    }

    /// <summary>
    /// Get real-time status for a specific project
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet("{id:guid}/status")]
    [ShortCache] // 5 minute cache for real-time status
    public async Task<ActionResult<ApiResponse<ProjectStatusDto>>> GetProjectStatus(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetProjectStatus", new { id });

            // Get project basic info
            var projectResult = await _projectService.GetProjectByIdAsync(id);
            if (!projectResult.IsSuccess)
                return BadRequest(new ApiResponse<ProjectStatusDto> { Success = false, Message = projectResult.Message });

            // Calculate project status from master plan progress
            var statusDto = new ProjectStatusDto
            {
                ProjectId = id,
                ProjectName = projectResult.Data!.ProjectName,
                Status = projectResult.Data.Status,
                PlannedStartDate = projectResult.Data.StartDate,
                PlannedEndDate = projectResult.Data.EstimatedEndDate,
                ActualStartDate = projectResult.Data.StartDate,
                OverallCompletionPercentage = 0,
                IsOnSchedule = true,
                IsOnBudget = true,
                ActiveTasks = 0,
                CompletedTasks = 0,
                TotalTasks = 0,
                LastUpdated = projectResult.Data.UpdatedAt ?? projectResult.Data.CreatedAt
            };

            // Add HATEOAS links for related resources
            statusDto.Links = new List<LinkDto>
            {
                new LinkDto { Href = Url.Action(nameof(GetProject), new { id }), Rel = "project", Method = "GET" },
                new LinkDto { Href = $"/api/v1/master-plans?projectId={id}", Rel = "master-plans", Method = "GET" },
                new LinkDto { Href = $"/api/v1/tasks?projectId={id}", Rel = "tasks", Method = "GET" },
                new LinkDto { Href = $"/api/v1/documents?projectId={id}", Rel = "documents", Method = "GET" }
            };

            return CreateSuccessResponse(statusDto, "Project status retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<ProjectStatusDto>(_logger, ex, $"retrieving project status for {id}");
        }
    }

    /// <summary>
    /// Test endpoint for API health check - no authentication required
    /// </summary>
    /// <returns>Test data to verify the Projects API is working</returns>
    [HttpGet("test")]
    [AllowAnonymous]
    public ActionResult<object> GetTestProjects()
    {
        var testData = new
        {
            message = "Projects API is working",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            apiVersion = "v1.0",
            sampleProjects = new[]
            {
                new { id = 1, name = "Solar Farm Project A", status = "Active", location = "California" },
                new { id = 2, name = "Solar Installation B", status = "Planning", location = "Texas" },
                new { id = 3, name = "Residential Solar C", status = "Completed", location = "Florida" }
            }
        };

        _logger.LogInformation("Test endpoint accessed at {Timestamp}", testData.timestamp);
        
        return Ok(testData);
    }
}
