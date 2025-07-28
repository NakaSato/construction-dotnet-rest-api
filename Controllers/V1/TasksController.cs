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
    public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>> GetTasks(
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
            var validationError = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationError != null)
                return CreateErrorResponse<PagedResult<TaskDto>>(validationError, 400);

            var result = await _taskService.GetTasksAsync(pageNumber, pageSize, projectId, assigneeId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<PagedResult<TaskDto>>(_logger, ex, "retrieving tasks");
        }
    }

    /// <summary>
    /// Gets a specific task by ID
    /// Available to: All authenticated users
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    [ShortCache] // 5 minute cache for individual tasks
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetTask(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "GetTask", new { id });

            var result = await _taskService.GetTaskByIdAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<TaskDto>(_logger, ex, "retrieving task");
        }
    }

    /// <summary>
    /// Creates a new task
    /// Available to: Admin, Manager, Supervisor roles
    /// </summary>
    /// <param name="projectId">Project ID to create the task for</param>
    /// <param name="request">Task creation data</param>
    /// <returns>Created task</returns>
    [HttpPost]
    [NoCache] // No caching for write operations
    [Authorize(Roles = "Admin,Manager,Supervisor")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> CreateTask(
        [FromQuery] Guid projectId,
        [FromBody] CreateTaskRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "CreateTask", new { projectId, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse<TaskDto>("Invalid input data", 400);

            var result = await _taskService.CreateTaskAsync(projectId, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<TaskDto>(_logger, ex, "creating task");
        }
    }

    /// <summary>
    /// Updates an existing task
    /// Available to: Admin, Manager, Supervisor roles
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateTaskRequest">Task update data</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id:guid}")]
    [NoCache] // No caching for write operations
    [Authorize(Roles = "Admin,Manager,Supervisor")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest updateTaskRequest)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateTask", new { id, updateTaskRequest });

            if (!ModelState.IsValid)
                return CreateErrorResponse<TaskDto>("Invalid input data", 400);

            var result = await _taskService.UpdateTaskAsync(id, updateTaskRequest);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<TaskDto>(_logger, ex, "updating task");
        }
    }

    /// <summary>
    /// Partially updates an existing task
    /// Available to: Admin, Manager, Supervisor roles
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="patchTaskRequest">Task partial update data</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}")]
    [NoCache] // No caching for write operations
    [Authorize(Roles = "Admin,Manager,Supervisor")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> PatchTask(Guid id, [FromBody] PatchTaskRequest patchTaskRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return CreateErrorResponse<TaskDto>("Invalid input data", 400);

            var result = await _taskService.PatchTaskAsync(id, patchTaskRequest);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<TaskDto>(_logger, ex, "updating task");
        }
    }

    /// <summary>
    /// Updates only the status of a task
    /// Available to: All authenticated users (own tasks), Admin, Manager, Supervisor (all tasks)
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="status">New task status</param>
    /// <returns>Success result</returns>
    [HttpPatch("{id:guid}/status")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<bool>>> UpdateTaskStatus(Guid id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return CreateErrorResponse<bool>("Status cannot be empty", 400);

            // Parse string status to enum
            if (!Enum.TryParse<dotnet_rest_api.Models.TaskStatus>(status, true, out var taskStatus))
            {
                return CreateErrorResponse<bool>($"Invalid task status: {status}", 400);
            }

            var result = await _taskService.UpdateTaskStatusAsync(id, taskStatus);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "updating task status");
        }
    }

    /// <summary>
    /// Deletes a task
    /// Available to: Admin, Manager roles only
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Success result</returns>
    [HttpDelete("{id:guid}")]
    [NoCache] // No caching for write operations
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTask(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteTask", new { id });

            var result = await _taskService.DeleteTaskAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "deleting task");
        }
    }

    /// <summary>
    /// Gets all tasks with advanced querying capabilities including filtering, sorting, and field selection
    /// Available to: All authenticated users
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
            var validationError = ValidatePaginationParameters(parameters.PageNumber, parameters.PageSize);
            if (validationError != null)
                return CreateErrorResponse<EnhancedPagedResult<TaskDto>>(validationError, 400);

            // Get data using existing service
            var serviceResult = await _taskService.GetTasksAsync(parameters);
            if (!serviceResult.Success)
                return CreateErrorResponse<EnhancedPagedResult<TaskDto>>(serviceResult.Message ?? "Operation failed", 400);

            // Build base URL for HATEOAS links
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.Path}";
            
            // Build query parameters for HATEOAS links
            var queryParams = new Dictionary<string, string>();
            if (parameters.ProjectId.HasValue) queryParams.Add("projectId", parameters.ProjectId.Value.ToString());
            if (parameters.AssigneeId.HasValue) queryParams.Add("assigneeId", parameters.AssigneeId.Value.ToString());
            if (!string.IsNullOrEmpty(parameters.Status)) queryParams.Add("status", parameters.Status);
            if (!string.IsNullOrEmpty(parameters.SortBy)) queryParams.Add("sortBy", parameters.SortBy);
            if (!string.IsNullOrEmpty(parameters.SortOrder)) queryParams.Add("sortOrder", parameters.SortOrder);

            // Create rich paginated response using QueryService
            var response = _queryService.CreateRichPaginatedResponse(
                serviceResult.Data!.Items,
                serviceResult.Data.TotalCount,
                parameters.PageNumber,
                parameters.PageSize,
                baseUrl,
                queryParams,
                "Tasks retrieved successfully"
            );

            return Ok(response);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<TaskDto>>(_logger, ex, "retrieving tasks with rich pagination");
        }
    }

    /// <summary>
    /// Get progress reports for a specific task
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

            var validationError = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationError != null)
                return CreateErrorResponse<PagedResult<TaskProgressReportDto>>(validationError, 400);

            var result = await _taskService.GetTaskProgressReportsAsync(taskId, pageNumber, pageSize);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<PagedResult<TaskProgressReportDto>>(_logger, ex, $"retrieving progress reports for task {taskId}");
        }
    }

    /// <summary>
    /// Create a progress report for a specific task
    /// Available to: All authenticated users (task assignees), Admin, Manager, Supervisor
    /// </summary>
    [HttpPost("{taskId:guid}/progress-reports")]
    [NoCache] // No caching for write operations
    public async Task<ActionResult<ApiResponse<TaskProgressReportDto>>> CreateTaskProgressReport(
        Guid taskId,
        [FromBody] CreateTaskProgressReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateTaskProgressReport", new { taskId, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse<TaskProgressReportDto>("Invalid input data", 400);

            var result = await _taskService.CreateTaskProgressReportAsync(taskId, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<TaskProgressReportDto>(_logger, ex, $"creating progress report for task {taskId}");
        }
    }
}
