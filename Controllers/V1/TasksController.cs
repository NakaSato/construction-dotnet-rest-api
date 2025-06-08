using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing tasks in solar projects
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all tasks with optional pagination and filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="assigneeId">Filter by assignee ID</param>
    /// <returns>Paginated list of tasks</returns>
    [HttpGet]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? projectId = null,
        [FromQuery] Guid? assigneeId = null)
    {
        try
        {
            // Validate pagination parameters
            if (pageNumber < 1)
                return BadRequest("Page number must be greater than 0.");

            if (pageSize < 1 || pageSize > 100)
                return BadRequest("Page size must be between 1 and 100.");

            var result = await _taskService.GetTasksAsync(pageNumber, pageSize, projectId, assigneeId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, "An error occurred while retrieving tasks.");
        }
    }

    /// <summary>
    /// Gets a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Task details</returns>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetTask(Guid id)
    {
        try
        {
            var result = await _taskService.GetTaskByIdAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {TaskId}", id);
            return StatusCode(500, "An error occurred while retrieving the task.");
        }
    }

    /// <summary>
    /// Gets all tasks for a specific project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>List of project tasks</returns>
    [HttpGet("project/{projectId:guid}")]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetProjectTasks(Guid projectId)
    {
        try
        {
            var result = await _taskService.GetProjectTasksAsync(projectId);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks for project {ProjectId}", projectId);
            return StatusCode(500, "An error occurred while retrieving project tasks.");
        }
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="createTaskRequest">Task creation data</param>
    /// <returns>Created task</returns>
    [HttpPost("project/{projectId:guid}")]
    public async Task<ActionResult<TaskDto>> CreateTask(Guid projectId, [FromBody] CreateTaskRequest createTaskRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.CreateTaskAsync(projectId, createTaskRequest);
            if (!result.Success)
                return BadRequest(result.Message);

            return CreatedAtAction(nameof(GetTask), new { id = result.Data!.TaskId }, result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, "An error occurred while creating the task.");
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="updateTaskRequest">Task update data</param>
    /// <returns>Updated task</returns>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> UpdateTask(Guid id, [FromBody] UpdateTaskRequest updateTaskRequest)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _taskService.UpdateTaskAsync(id, updateTaskRequest);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task.");
        }
    }

    /// <summary>
    /// Updates only the status of a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="status">New task status</param>
    /// <returns>Updated task</returns>
    [HttpPatch("{id:guid}/status")]
    public async Task<ActionResult<bool>> UpdateTaskStatus(Guid id, [FromBody] string status)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(status))
                return BadRequest("Status cannot be empty.");

            // Parse string status to enum
            if (!Enum.TryParse<dotnet_rest_api.Models.TaskStatus>(status, true, out var taskStatus))
            {
                return BadRequest($"Invalid task status: {status}");
            }

            var result = await _taskService.UpdateTaskStatusAsync(id, taskStatus);
            if (!result.Success)
                return NotFound(result.Message);
            
            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status for task {TaskId}", id);
            return StatusCode(500, "An error occurred while updating the task status.");
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>No content if successful</returns>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            var result = await _taskService.DeleteTaskAsync(id);
            if (!result.Success)
                return NotFound(result.Message);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", id);
            return StatusCode(500, "An error occurred while deleting the task.");
        }
    }
}
