using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Controller for managing Work Breakdown Structure (WBS) tasks for solar PV installation projects
/// </summary>
[Route("api/v1/wbs")]
[ApiController]
[Authorize]
public class WbsController : ControllerBase
{
    private readonly IWbsService _wbsService;
    private readonly ILogger<WbsController> _logger;

    public WbsController(IWbsService wbsService, ILogger<WbsController> logger)
    {
        _wbsService = wbsService;
        _logger = logger;
    }

    /// <summary>
    /// Get all WBS tasks with optional filtering
    /// </summary>
    /// <param name="projectId">Filter by project ID</param>
    /// <param name="installationArea">Filter by installation area</param>
    /// <param name="status">Filter by task status</param>
    /// <returns>List of WBS tasks</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<WbsTaskDto>>>> GetAllTasks(
        [FromQuery] Guid projectId,
        [FromQuery] string? installationArea = null,
        [FromQuery] WbsTaskStatus? status = null)
    {
        try
        {
            var tasks = await _wbsService.GetAllTasksAsync(projectId, installationArea, status);
            
            return Ok(new ApiResponse<IEnumerable<WbsTaskDto>>
            {
                Success = true,
                Data = tasks,
                Message = "WBS tasks retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WBS tasks");
            return StatusCode(500, new ApiResponse<IEnumerable<WbsTaskDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving WBS tasks"
            });
        }
    }

    /// <summary>
    /// Get a specific WBS task by its WBS ID
    /// </summary>
    /// <param name="wbsId">The WBS ID of the task</param>
    /// <returns>WBS task details</returns>
    [HttpGet("{wbsId}")]
    public async Task<ActionResult<ApiResponse<WbsTaskDto>>> GetTask(string wbsId)
    {
        try
        {
            var task = await _wbsService.GetTaskByIdAsync(wbsId);
            
            if (task == null)
            {
                return NotFound(new ApiResponse<WbsTaskDto>
                {
                    Success = false,
                    Message = $"WBS task with ID '{wbsId}' not found"
                });
            }

            return Ok(new ApiResponse<WbsTaskDto>
            {
                Success = true,
                Data = task,
                Message = "WBS task retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WBS task {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = "An error occurred while retrieving the WBS task"
            });
        }
    }

    /// <summary>
    /// Get hierarchical tree structure of WBS tasks for a project
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Hierarchical tree of WBS tasks</returns>
    [HttpGet("hierarchy/{projectId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WbsTaskHierarchyDto>>>> GetTaskHierarchy(Guid projectId)
    {
        try
        {
            var hierarchy = await _wbsService.GetTaskHierarchyAsync(projectId);
            
            return Ok(new ApiResponse<IEnumerable<WbsTaskHierarchyDto>>
            {
                Success = true,
                Data = hierarchy,
                Message = "WBS task hierarchy retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WBS task hierarchy for project {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<IEnumerable<WbsTaskHierarchyDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving the WBS task hierarchy"
            });
        }
    }

    /// <summary>
    /// Create a new WBS task
    /// </summary>
    /// <param name="createDto">WBS task creation data</param>
    /// <returns>Created WBS task</returns>
    [HttpPost]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WbsTaskDto>>> CreateTask([FromBody] CreateWbsTaskDto createDto)
    {
        try
        {
            var task = await _wbsService.CreateTaskAsync(createDto);
            
            return CreatedAtAction(nameof(GetTask), new { wbsId = task.WbsId }, 
                new ApiResponse<WbsTaskDto>
                {
                    Success = true,
                    Data = task,
                    Message = "WBS task created successfully"
                });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WBS task");
            return StatusCode(500, new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = "An error occurred while creating the WBS task"
            });
        }
    }

    /// <summary>
    /// Update an existing WBS task
    /// </summary>
    /// <param name="wbsId">WBS ID of the task to update</param>
    /// <param name="updateDto">Updated task data</param>
    /// <returns>Updated WBS task</returns>
    [HttpPut("{wbsId}")]
    [Authorize(Roles = "Admin,ProjectManager")]
    public async Task<ActionResult<ApiResponse<WbsTaskDto>>> UpdateTask(string wbsId, [FromBody] UpdateWbsTaskDto updateDto)
    {
        try
        {
            var task = await _wbsService.UpdateTaskAsync(wbsId, updateDto);
            
            return Ok(new ApiResponse<WbsTaskDto>
            {
                Success = true,
                Data = task,
                Message = "WBS task updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WBS task {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = "An error occurred while updating the WBS task"
            });
        }
    }

    /// <summary>
    /// Delete a WBS task
    /// </summary>
    /// <param name="wbsId">WBS ID of the task to delete</param>
    /// <returns>Confirmation of deletion</returns>
    [HttpDelete("{wbsId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> DeleteTask(string wbsId)
    {
        try
        {
            var deleted = await _wbsService.DeleteTaskAsync(wbsId);
            
            if (!deleted)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = $"WBS task with ID '{wbsId}' not found"
                });
            }

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "WBS task deleted successfully"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ApiResponse<object>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting WBS task {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the WBS task"
            });
        }
    }

    /// <summary>
    /// Update the status of a WBS task
    /// </summary>
    /// <param name="wbsId">WBS ID of the task</param>
    /// <param name="status">New status</param>
    /// <returns>Updated WBS task</returns>
    [HttpPatch("{wbsId}/status")]
    public async Task<ActionResult<ApiResponse<WbsTaskDto>>> UpdateTaskStatus(string wbsId, [FromBody] WbsTaskStatus status)
    {
        try
        {
            var task = await _wbsService.UpdateTaskStatusAsync(wbsId, status);
            
            return Ok(new ApiResponse<WbsTaskDto>
            {
                Success = true,
                Data = task,
                Message = "WBS task status updated successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WBS task status {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<WbsTaskDto>
            {
                Success = false,
                Message = "An error occurred while updating the WBS task status"
            });
        }
    }

    /// <summary>
    /// Add evidence to a WBS task
    /// </summary>
    /// <param name="wbsId">WBS ID of the task</param>
    /// <param name="evidenceDto">Evidence data</param>
    /// <returns>Created evidence</returns>
    [HttpPost("{wbsId}/evidence")]
    public async Task<ActionResult<ApiResponse<WbsTaskEvidenceDto>>> AddEvidence(string wbsId, [FromBody] CreateWbsTaskEvidenceDto evidenceDto)
    {
        try
        {
            var evidence = await _wbsService.AddEvidenceAsync(wbsId, evidenceDto);
            
            return Ok(new ApiResponse<WbsTaskEvidenceDto>
            {
                Success = true,
                Data = evidence,
                Message = "Evidence added successfully"
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ApiResponse<WbsTaskEvidenceDto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding evidence to WBS task {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<WbsTaskEvidenceDto>
            {
                Success = false,
                Message = "An error occurred while adding evidence"
            });
        }
    }

    /// <summary>
    /// Get all evidence for a WBS task
    /// </summary>
    /// <param name="wbsId">WBS ID of the task</param>
    /// <returns>List of evidence</returns>
    [HttpGet("{wbsId}/evidence")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WbsTaskEvidenceDto>>>> GetTaskEvidence(string wbsId)
    {
        try
        {
            var evidence = await _wbsService.GetTaskEvidenceAsync(wbsId);
            
            return Ok(new ApiResponse<IEnumerable<WbsTaskEvidenceDto>>
            {
                Success = true,
                Data = evidence,
                Message = "Evidence retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving evidence for WBS task {WbsId}", wbsId);
            return StatusCode(500, new ApiResponse<IEnumerable<WbsTaskEvidenceDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving evidence"
            });
        }
    }

    /// <summary>
    /// Calculate project progress based on WBS task completion
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Project progress information</returns>
    [HttpGet("progress/{projectId}")]
    public async Task<ActionResult<ApiResponse<WbsProjectProgressDto>>> GetProjectProgress(Guid projectId)
    {
        try
        {
            var progress = await _wbsService.CalculateProjectProgressAsync(projectId);
            
            return Ok(new ApiResponse<WbsProjectProgressDto>
            {
                Success = true,
                Data = progress,
                Message = "Project progress calculated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating project progress for {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<WbsProjectProgressDto>
            {
                Success = false,
                Message = "An error occurred while calculating project progress"
            });
        }
    }

    /// <summary>
    /// Get tasks that are ready to start (dependencies completed)
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>List of ready-to-start tasks</returns>
    [HttpGet("ready-to-start/{projectId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WbsTaskDto>>>> GetReadyToStartTasks(Guid projectId)
    {
        try
        {
            var tasks = await _wbsService.GetReadyToStartTasksAsync(projectId);
            
            return Ok(new ApiResponse<IEnumerable<WbsTaskDto>>
            {
                Success = true,
                Data = tasks,
                Message = "Ready-to-start tasks retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving ready-to-start tasks for project {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<IEnumerable<WbsTaskDto>>
            {
                Success = false,
                Message = "An error occurred while retrieving ready-to-start tasks"
            });
        }
    }

    /// <summary>
    /// Seed sample WBS data for a project (development/testing)
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <returns>Confirmation message</returns>
    [HttpPost("seed-data/{projectId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<object>>> SeedSampleData(Guid projectId)
    {
        try
        {
            await _wbsService.SeedSampleDataAsync(projectId);
            
            return Ok(new ApiResponse<object>
            {
                Success = true,
                Message = "Sample WBS data seeded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding sample data for project {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while seeding sample data"
            });
        }
    }
}
