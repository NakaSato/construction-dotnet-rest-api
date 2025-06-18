using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing tasks in solar projects
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tasks")]
[Authorize]
public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;
    private readonly IQueryService _queryService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, IQueryService queryService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all tasks with optional pagination and filtering
    /// Available to: All authenticated users (view assigned tasks)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="assigneeId">Filter by assignee ID</param>
    /// <returns>Paginated list of tasks</returns>
    [HttpGet]
    [ShortCache] // 5 minute cache for task lists
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? assigneeId = null)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetTasks", new { pageNumber, pageSize, projectId, assigneeId });

            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return validationResult;

            var result = await _taskService.GetTasksAsync(pageNumber, pageSize, projectId, assigneeId);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving tasks");
        }
    }

    /// <summary>
    /// Gets a specific task by ID
    /// Available to: All authenticated users (view task details)
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    [MediumCache] // 15 minute cache for individual task details
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetTask", new { id });

            var result = await _taskService.GetTaskByIdAsync(id);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data!, "Task retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving task");
        }
    }

    /// <summary>
    /// Gets all tasks for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>List of project tasks</returns>
    [HttpGet("project/{projectId:guid}")]
    [MediumCache] // 15 minute cache for project tasks
    public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>> GetProjectTasks(Guid projectId)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetProjectTasks", new { projectId });

            var result = await _taskService.GetProjectTasksAsync(projectId);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data!, "Project tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving project tasks");
        }
    }

    /// <summary>
    /// Creates a new task
    /// Available to: Administrator, ProjectManager (can manage tasks)
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="createTaskRequest">Task creation data</param>
    /// <returns>Created task</returns>
    [HttpPost("project/{projectId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(Guid projectId, [FromBody] CreateTaskRequest createTaskRequest)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "CreateTask", new { projectId, createTaskRequest });

            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _taskService.CreateTaskAsync(projectId, createTaskRequest);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return StatusCode(201, new ApiResponse<TaskDto>
            {
                Success = true,
                Message = "Task created successfully",
                Data = result.Data
            });
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "creating task");
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateTaskRequest">Task update data</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id:guid}")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest updateTaskRequest)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateTask", new { id, updateTaskRequest });

            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _taskService.UpdateTaskAsync(id, updateTaskRequest);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data!, "Task updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "updating task");
        }
    }

    /// <summary>
    /// Partially updates an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="patchTaskRequest">Task partial update data</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<TaskDto>>> PatchTask(Guid id, [FromBody] PatchTaskRequest patchTaskRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _taskService.PatchTaskAsync(id, patchTaskRequest);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data!, "Task updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "updating task");
        }
    }

    /// <summary>
    /// Updates only the status of a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="status">New task status</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}/status")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<bool>>> UpdateTaskStatus(Guid id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return CreateErrorResponse("Status cannot be empty", 400);

            // Parse string status to enum
            if (!Enum.TryParse<dotnet_rest_api.Models.TaskStatus>(status, true, out var taskStatus))
            {
                return CreateErrorResponse($"Invalid task status: {status}", 400);
            }

            var result = await _taskService.UpdateTaskStatusAsync(id, taskStatus);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data, "Task status updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "updating task status");
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteTask", new { id });

            var result = await _taskService.DeleteTaskAsync(id);
            if (!result.Success)
                return CreateNotFoundResponse(result.Message);
            
            return CreateSuccessResponse(result.Data, "Task deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "deleting task");
        }
    }

    /// <summary>
    /// Gets all tasks with advanced querying capabilities including filtering, sorting, and field selection
    /// </summary>
    /// <param name="parameters">Advanced query parameters</param>
    /// <returns>Enhanced paginated list of tasks with metadata</returns>
    [HttpGet("advanced")]
    [ShortCache] // 5 minute cache for advanced task queries
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<TaskDto>>>> GetTasksAdvanced([FromQuery] TaskQueryParameters parameters)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetTasksAdvanced", parameters);

            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(parameters.PageNumber, parameters.PageSize);
            if (validationResult != null)
                return validationResult;

            // Parse dynamic filters from query string using the base controller method
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _taskService.GetTasksAsync(parameters);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Tasks retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving tasks with advanced query");
        }
    }

    /// <summary>
    /// Gets all tasks with rich HATEOAS pagination and enhanced metadata
    /// </summary>
    [HttpGet("rich")]
    [ShortCache] // 5 minute cache for rich task pagination
    public async Task<ActionResult<ApiResponseWithPagination<TaskDto>>> GetTasksWithRichPagination(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? assigneeId = null,
        [FromQuery] string? status = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = "asc")
    {
        try
        {
            // Validate pagination parameters using base controller method
            var validationResult = ValidatePaginationParameters(page, pageSize);
            if (validationResult != null)
                return validationResult;

            // Create query parameters
            var parameters = new TaskQueryParameters
            {
                PageNumber = page,
                PageSize = pageSize,
                ProjectId = projectId,
                AssigneeId = assigneeId,
                Status = status,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            // Get data using existing service
            var serviceResult = await _taskService.GetTasksAsync(parameters);
            if (!serviceResult.Success)
                return CreateErrorResponse(serviceResult.Message, 400);

            // Build base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (projectId.HasValue) queryParams.Add("projectId", projectId.Value.ToString());
            if (assigneeId.HasValue) queryParams.Add("assigneeId", assigneeId.Value.ToString());
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
                "Tasks retrieved successfully"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, "retrieving tasks with rich pagination");
        }
    }

    /// <summary>
    /// Get progress reports for a specific task (limited nesting)
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet("{taskId:guid}/progress-reports")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<PagedResult<TaskProgressReportDto>>>> GetTaskProgressReports(
        Guid taskId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            LogControllerAction(_logger, "GetTaskProgressReports", new { taskId, pageNumber, pageSize });

            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return validationResult;

            var result = await _taskService.GetTaskProgressReportsAsync(taskId, pageNumber, pageSize);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Task progress reports retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"retrieving progress reports for task {taskId}");
        }
    }

    /// <summary>
    /// Create a progress report for a specific task
    /// Available to: Administrator, ProjectManager, Technician
    /// </summary>
    [HttpPost("{taskId:guid}/progress-reports")]
    [Authorize(Roles = "Administrator,ProjectManager,Technician")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<TaskProgressReportDto>>> CreateTaskProgressReport(
        Guid taskId, 
        [FromBody] CreateTaskProgressReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateTaskProgressReport", new { taskId, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse("Invalid input data", 400);

            var result = await _taskService.CreateTaskProgressReportAsync(taskId, request);
            if (!result.Success)
                return CreateErrorResponse(result.Message, 400);

            return CreateSuccessResponse(result.Data!, "Task progress report created successfully");
        }
        catch (Exception ex)
        {
            return HandleException(_logger, ex, $"creating progress report for task {taskId}");
        }
    }
}
