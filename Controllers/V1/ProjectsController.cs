using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IQueryService _queryService;

    public ProjectsController(IProjectService projectService, IQueryService queryService)
    {
        _projectService = projectService;
        _queryService = queryService;
    }

    /// <summary>
    /// Get all projects with advanced filtering, sorting, and field selection
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<ProjectDto>>>> GetProjects([FromQuery] ProjectQueryParameters parameters)
    {
        // Parse dynamic filters from query string if provided
        var filterString = Request.Query["filter"].FirstOrDefault();
        if (!string.IsNullOrEmpty(filterString))
        {
            parameters.Filters.AddRange(parameters.ParseFilters(filterString));
        }

        var result = await _projectService.GetProjectsAsync(parameters);
        return Ok(result);
    }

    /// <summary>
    /// Get all projects with pagination (legacy endpoint for backward compatibility)
    /// </summary>
    [HttpGet("legacy")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetProjectsLegacy(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? managerId = null)
    {
        var result = await _projectService.GetProjectsLegacyAsync(pageNumber, pageSize, managerId);
        return Ok(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> GetProject(Guid id)
    {
        var result = await _projectService.GetProjectByIdAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "Invalid request data",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var result = await _projectService.CreateProjectAsync(request);
        
        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetProject), new { id = result.Data!.ProjectId }, result);
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    public async Task<ActionResult<ApiResponse<ProjectDto>>> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse<ProjectDto>
            {
                Success = false,
                Message = "Invalid request data",
                Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        var result = await _projectService.UpdateProjectAsync(id, request);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a project
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteProject(Guid id)
    {
        var result = await _projectService.DeleteProjectAsync(id);
        
        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get projects for the current user
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<PagedResult<ProjectDto>>>> GetMyProjects(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new ApiResponse<PagedResult<ProjectDto>>
            {
                Success = false,
                Message = "Invalid user ID in token"
            });
        }

        var result = await _projectService.GetUserProjectsAsync(userId, pageNumber, pageSize);
        return Ok(result);
    }

    /// <summary>
    /// Get all projects with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("rich")]
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
            // Validate pagination parameters
            if (page < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

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
            if (!serviceResult.Success)
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
            return StatusCode(500, new ApiResponseWithPagination<ProjectDto>
            {
                Success = false,
                Message = "An error occurred while retrieving projects",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}
