using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Attributes;
using System.Security.Claims;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Controller for managing work requests
/// </summary>
[Route("api/v{version:apiVersion}/work-requests")]
[ApiController]
[ApiVersion("1.0")]
[Authorize]
public class WorkRequestsController : ControllerBase
{
    private readonly IWorkRequestService _workRequestService;
    private readonly ILogger<WorkRequestsController> _logger;

    public WorkRequestsController(
        IWorkRequestService workRequestService,
        ILogger<WorkRequestsController> logger)
    {
        _workRequestService = workRequestService;
        _logger = logger;
    }

    /// <summary>
    /// Get all work requests with filtering and pagination
    /// </summary>
    /// <param name="parameters">Query parameters for filtering and pagination</param>
    /// <returns>Paginated list of work requests</returns>
    [HttpGet]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<EnhancedPagedResult<WorkRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WorkRequestDto>>>> GetWorkRequests([FromQuery] WorkRequestQueryParameters parameters)
    {
        try
        {
            var result = await _workRequestService.GetWorkRequestsAsync(parameters);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving work requests"
            });
        }
    }

    /// <summary>
    /// Get a specific work request by ID
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <returns>Work request details</returns>
    [HttpGet("{id:guid}")]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<WorkRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> GetWorkRequest(Guid id)
    {
        try
        {
            var result = await _workRequestService.GetWorkRequestByIdAsync(id);

            if (!result.Success)
            {
                return NotFound(result);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work request {WorkRequestId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving the work request"
            });
        }
    }

    /// <summary>
    /// Get work requests for a specific project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of work requests for the project</returns>
    [HttpGet("project/{projectId:guid}")]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkRequestDto>>>> GetProjectWorkRequests(
        Guid projectId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _workRequestService.GetProjectWorkRequestsAsync(projectId, pageNumber, pageSize);

            if (!result.Success)
            {
                return NotFound(result);
            }

            // Add HATEOAS links
            if (result.Data?.Items != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests for project {ProjectId}", projectId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving project work requests"
            });
        }
    }

    /// <summary>
    /// Get work requests assigned to a specific user
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <returns>Paginated list of work requests assigned to the user</returns>
    [HttpGet("assigned/{userId:guid}")]
    [Cache(300)] // Cache for 5 minutes
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkRequestDto>>>> GetAssignedWorkRequests(
        Guid userId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var result = await _workRequestService.GetAssignedWorkRequestsAsync(userId, pageNumber, pageSize);

            if (!result.Success)
            {
                return NotFound(result);
            }

            // Add HATEOAS links
            if (result.Data?.Items != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving work requests assigned to user {UserId}", userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while retrieving assigned work requests"
            });
        }
    }

    /// <summary>
    /// Create a new work request
    /// </summary>
    /// <param name="request">Work request creation request</param>
    /// <returns>Created work request</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> CreateWorkRequest([FromBody] CreateWorkRequestRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetCurrentUserId();
            var result = await _workRequestService.CreateWorkRequestAsync(request, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreatedAtAction(
                nameof(GetWorkRequest),
                new { id = result.Data!.WorkRequestId },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating work request");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while creating the work request"
            });
        }
    }

    /// <summary>
    /// Update an existing work request
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <param name="request">Work request update request</param>
    /// <returns>Updated work request</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> UpdateWorkRequest(Guid id, [FromBody] UpdateWorkRequestRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _workRequestService.UpdateWorkRequestAsync(id, request);

            if (!result.Success)
            {
                return result.Message == "Work request not found" ? NotFound(result) : BadRequest(result);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work request {WorkRequestId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the work request"
            });
        }
    }

    /// <summary>
    /// Delete a work request
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkRequest(Guid id)
    {
        try
        {
            var result = await _workRequestService.DeleteWorkRequestAsync(id);

            if (!result.Success)
            {
                return result.Message == "Work request not found" ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work request {WorkRequestId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the work request"
            });
        }
    }

    /// <summary>
    /// Assign a work request to a user
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <param name="userId">The user ID to assign to</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/assign/{userId:guid}")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignWorkRequest(Guid id, Guid userId)
    {
        try
        {
            var result = await _workRequestService.AssignWorkRequestAsync(id, userId);

            if (!result.Success)
            {
                return result.Message == "Work request not found" || result.Message == "User not found" 
                    ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning work request {WorkRequestId} to user {UserId}", id, userId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while assigning the work request"
            });
        }
    }

    /// <summary>
    /// Complete a work request
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteWorkRequest(Guid id)
    {
        try
        {
            var result = await _workRequestService.CompleteWorkRequestAsync(id);

            if (!result.Success)
            {
                return result.Message == "Work request not found" ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing work request {WorkRequestId}", id);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while completing the work request"
            });
        }
    }

    #region Work Request Tasks

    /// <summary>
    /// Add a task to a work request
    /// </summary>
    /// <param name="workRequestId">The work request ID</param>
    /// <param name="request">Work request task creation request</param>
    /// <returns>Created work request task</returns>
    [HttpPost("{workRequestId:guid}/tasks")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestTaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestTaskDto>>> AddWorkRequestTask(Guid workRequestId, [FromBody] CreateWorkRequestTaskRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _workRequestService.AddWorkRequestTaskAsync(workRequestId, request);

            if (!result.Success)
            {
                return result.Message == "Work request not found" ? NotFound(result) : BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetWorkRequest),
                new { id = workRequestId },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding task to work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while adding the work request task"
            });
        }
    }

    /// <summary>
    /// Update a work request task
    /// </summary>
    /// <param name="workRequestId">The work request ID</param>
    /// <param name="taskId">The work request task ID</param>
    /// <param name="request">Work request task update request</param>
    /// <returns>Updated work request task</returns>
    [HttpPut("{workRequestId:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestTaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestTaskDto>>> UpdateWorkRequestTask(Guid workRequestId, Guid taskId, [FromBody] UpdateWorkRequestTaskRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var result = await _workRequestService.UpdateWorkRequestTaskAsync(taskId, request);

            if (!result.Success)
            {
                return result.Message == "Work request task not found" ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating work request task {TaskId}", taskId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while updating the work request task"
            });
        }
    }

    /// <summary>
    /// Delete a work request task
    /// </summary>
    /// <param name="workRequestId">The work request ID</param>
    /// <param name="taskId">The work request task ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{workRequestId:guid}/tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkRequestTask(Guid workRequestId, Guid taskId)
    {
        try
        {
            var result = await _workRequestService.DeleteWorkRequestTaskAsync(taskId);

            if (!result.Success)
            {
                return result.Message == "Work request task not found" ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work request task {TaskId}", taskId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the work request task"
            });
        }
    }

    #endregion

    #region Work Request Comments

    /// <summary>
    /// Add a comment to a work request
    /// </summary>
    /// <param name="workRequestId">The work request ID</param>
    /// <param name="request">Work request comment creation request</param>
    /// <returns>Created work request comment</returns>
    [HttpPost("{workRequestId:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestCommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestCommentDto>>> AddWorkRequestComment(Guid workRequestId, [FromBody] CreateWorkRequestCommentRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
                });
            }

            var userId = GetCurrentUserId();
            var result = await _workRequestService.AddWorkRequestCommentAsync(workRequestId, request, userId);

            if (!result.Success)
            {
                return result.Message == "Work request not found" ? NotFound(result) : BadRequest(result);
            }

            return CreatedAtAction(
                nameof(GetWorkRequest),
                new { id = workRequestId },
                result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to work request {WorkRequestId}", workRequestId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while adding the work request comment"
            });
        }
    }

    /// <summary>
    /// Delete a work request comment
    /// </summary>
    /// <param name="workRequestId">The work request ID</param>
    /// <param name="commentId">The work request comment ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{workRequestId:guid}/comments/{commentId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkRequestComment(Guid workRequestId, Guid commentId)
    {
        try
        {
            var result = await _workRequestService.DeleteWorkRequestCommentAsync(commentId);

            if (!result.Success)
            {
                return result.Message == "Work request comment not found" ? NotFound(result) : BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting work request comment {CommentId}", commentId);
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while deleting the work request comment"
            });
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private void AddHateoasLinks(WorkRequestDto workRequest)
    {
        workRequest.Links = new List<LinkDto>
        {
            new LinkDto
            {
                Href = Url.Action(nameof(GetWorkRequest), new { id = workRequest.WorkRequestId }),
                Rel = "self",
                Method = "GET"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(UpdateWorkRequest), new { id = workRequest.WorkRequestId }),
                Rel = "update",
                Method = "PUT"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(DeleteWorkRequest), new { id = workRequest.WorkRequestId }),
                Rel = "delete",
                Method = "DELETE"
            }
        };

        // Add status-specific actions
        if (workRequest.Status == "Open" || workRequest.Status == "InProgress")
        {
            workRequest.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(CompleteWorkRequest), new { id = workRequest.WorkRequestId }),
                Rel = "complete",
                Method = "POST"
            });
        }

        if (workRequest.AssignedToId == null)
        {
            workRequest.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(AssignWorkRequest), new { id = workRequest.WorkRequestId, userId = "{userId}" }),
                Rel = "assign",
                Method = "POST"
            });
        }
    }

    #endregion
}
