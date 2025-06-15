using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.Services;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
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
public class WorkRequestsController : BaseApiController
{
    private readonly IWorkRequestService _workRequestService;
    private readonly ILogger<WorkRequestsController> _logger;
    private readonly IWorkRequestApprovalService _approvalService;

    public WorkRequestsController(
        IWorkRequestService workRequestService,
        ILogger<WorkRequestsController> logger,
        IWorkRequestApprovalService approvalService)
    {
        _workRequestService = workRequestService;
        _logger = logger;
        _approvalService = approvalService;
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
            // Log controller action for debugging
            LogControllerAction(_logger, "GetWorkRequests", parameters);

            // Apply dynamic filters from query string using base controller method
            var filterString = Request.Query["filter"].FirstOrDefault();
            ApplyFiltersFromQuery(parameters, filterString);

            var result = await _workRequestService.GetWorkRequestsAsync(parameters);

            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return CreateSuccessResponse(result.Data!, "Work requests retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<WorkRequestDto>>(_logger, ex, "retrieving work requests");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "GetWorkRequest", new { id });

            var result = await _workRequestService.GetWorkRequestByIdAsync(id);

            if (!result.Success)
            {
                return CreateNotFoundResponse(result.Message);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Work request retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"retrieving work request {id}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "GetProjectWorkRequests", new { projectId, pageNumber, pageSize });

            var result = await _workRequestService.GetProjectWorkRequestsAsync(projectId, pageNumber, pageSize);

            if (!result.Success)
            {
                return CreateNotFoundResponse(result.Message);
            }

            // Add HATEOAS links
            if (result.Data?.Items != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return CreateSuccessResponse(result.Data!, "Project work requests retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<PagedResult<WorkRequestDto>>(_logger, ex, $"retrieving work requests for project {projectId}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "GetAssignedWorkRequests", new { userId, pageNumber, pageSize });

            var result = await _workRequestService.GetAssignedWorkRequestsAsync(userId, pageNumber, pageSize);

            if (!result.Success)
            {
                return CreateNotFoundResponse(result.Message);
            }

            // Add HATEOAS links
            if (result.Data?.Items != null)
            {
                foreach (var workRequest in result.Data.Items)
                {
                    AddHateoasLinks(workRequest);
                }
            }

            return CreateSuccessResponse(result.Data!, "Assigned work requests retrieved successfully");
        }
        catch (Exception ex)
        {
            return HandleException<PagedResult<WorkRequestDto>>(_logger, ex, $"retrieving work requests assigned to user {userId}");
        }
    }

    /// <summary>
    /// Create a new work request
    /// Available to: Administrator, ProjectManager, Planner (planning responsibilities)
    /// </summary>
    /// <param name="request">Work request creation request</param>
    /// <returns>Created work request</returns>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager,Planner")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> CreateWorkRequest([FromBody] CreateWorkRequestRequest request)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "CreateWorkRequest", new { request.Title, request.ProjectId });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = errors
                };
                return BadRequest(response);
            }

            var userId = GetCurrentUserId();
            var result = await _workRequestService.CreateWorkRequestAsync(request, userId);

            if (!result.Success)
            {
                return CreateErrorResponse(result.Message, 400);
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
            return HandleException<WorkRequestDto>(_logger, ex, "creating work request");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateWorkRequest", new { id, request.Title });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = errors
                };
                return BadRequest(response);
            }

            var result = await _workRequestService.UpdateWorkRequestAsync(id, request);

            if (!result.Success)
            {
                return result.Message == "Work request not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Work request updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"updating work request {id}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteWorkRequest", new { id });

            var result = await _workRequestService.DeleteWorkRequestAsync(id);

            if (!result.Success)
            {
                return result.Message == "Work request not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Work request deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"deleting work request {id}");
        }
    }

    /// <summary>
    /// Assign a work request to a user
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <param name="userId">The user ID to assign to</param>
    /// <returns>Success status</returns>
    [HttpPost("{id:guid}/assign/{userId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> AssignWorkRequest(Guid id, Guid userId)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "AssignWorkRequest", new { id, userId });

            var result = await _workRequestService.AssignWorkRequestAsync(id, userId);

            if (!result.Success)
            {
                return result.Message == "Work request not found" || result.Message == "User not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Work request assigned successfully");
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"assigning work request {id} to user {userId}");
        }
    }

    /// <summary>
    /// Complete a work request
    /// </summary>
    /// <param name="id">The work request ID</param>
    /// <returns>Completed work request</returns>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(ApiResponse<WorkRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> CompleteWorkRequest(Guid id)
    {
        try
        {
            // Log controller action for debugging
            LogControllerAction(_logger, "CompleteWorkRequest", new { id });

            var result = await _workRequestService.CompleteWorkRequestAsync(id);

            if (!result.Success)
            {
                return result.Message == "Work request not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            // Add HATEOAS links
            if (result.Data != null)
            {
                AddHateoasLinks(result.Data);
            }

            return CreateSuccessResponse(result.Data!, "Work request completed successfully");
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"completing work request {id}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "AddWorkRequestTask", new { workRequestId, request.Title });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = errors
                };
                return BadRequest(response);
            }

            var result = await _workRequestService.AddWorkRequestTaskAsync(workRequestId, request);

            if (!result.Success)
            {
                return result.Message == "Work request not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreatedAtAction(
                nameof(GetWorkRequest),
                new { id = workRequestId },
                result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestTaskDto>(_logger, ex, $"adding task to work request {workRequestId}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "UpdateWorkRequestTask", new { workRequestId, taskId });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = errors
                };
                return BadRequest(response);
            }

            var result = await _workRequestService.UpdateWorkRequestTaskAsync(taskId, request);

            if (!result.Success)
            {
                return result.Message == "Work request task not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data!, "Work request task updated successfully");
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestTaskDto>(_logger, ex, $"updating work request task {taskId}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteWorkRequestTask", new { workRequestId, taskId });

            var result = await _workRequestService.DeleteWorkRequestTaskAsync(taskId);

            if (!result.Success)
            {
                return result.Message == "Work request task not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Work request task deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"deleting work request task {taskId}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "AddWorkRequestComment", new { workRequestId });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = "Invalid request data",
                    Errors = errors
                };
                return BadRequest(response);
            }

            var userId = GetCurrentUserId();
            var result = await _workRequestService.AddWorkRequestCommentAsync(workRequestId, request, userId);

            if (!result.Success)
            {
                return result.Message == "Work request not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreatedAtAction(
                nameof(GetWorkRequest),
                new { id = workRequestId },
                result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestCommentDto>(_logger, ex, $"adding comment to work request {workRequestId}");
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
            // Log controller action for debugging
            LogControllerAction(_logger, "DeleteWorkRequestComment", new { workRequestId, commentId });

            var result = await _workRequestService.DeleteWorkRequestCommentAsync(commentId);

            if (!result.Success)
            {
                return result.Message == "Work request comment not found" 
                    ? CreateNotFoundResponse(result.Message) 
                    : CreateErrorResponse(result.Message, 400);
            }

            return CreateSuccessResponse(result.Data, "Work request comment deleted successfully");
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"deleting work request comment {commentId}");
        }
    }

    #endregion

    #region Approval Workflow

    /// <summary>
    /// Submit a work request for approval
    /// </summary>
    /// <param name="id">Work request ID</param>
    /// <param name="request">Approval submission request</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/submit-approval")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitForApproval(Guid id, [FromBody] SubmitForApprovalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!IsValidUserId(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _approvalService.SubmitForApprovalAsync(id, request, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting work request for approval");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while submitting for approval"
            });
        }
    }

    /// <summary>
    /// Process approval for a work request (approve/reject/escalate)
    /// </summary>
    /// <param name="id">Work request ID</param>
    /// <param name="request">Approval request</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/process-approval")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> ProcessApproval(Guid id, [FromBody] ApprovalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!IsValidUserId(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _approvalService.ProcessApprovalAsync(id, request, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing approval");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while processing approval"
            });
        }
    }

    /// <summary>
    /// Get approval workflow status for a work request
    /// </summary>
    /// <param name="id">Work request ID</param>
    /// <returns>Approval workflow status</returns>
    [HttpGet("{id}/approval-status")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalWorkflowStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowStatusDto>>> GetApprovalStatus(Guid id)
    {
        try
        {
            var result = await _approvalService.GetApprovalStatusAsync(id);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval status");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while getting approval status"
            });
        }
    }

    /// <summary>
    /// Get approval history for a work request
    /// </summary>
    /// <param name="id">Work request ID</param>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated approval history</returns>
    [HttpGet("{id}/approval-history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkRequestApprovalDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkRequestApprovalDto>>>> GetApprovalHistory(
        Guid id, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var result = await _approvalService.GetApprovalHistoryAsync(id, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval history");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while getting approval history"
            });
        }
    }

    /// <summary>
    /// Escalate approval to another user
    /// </summary>
    /// <param name="id">Work request ID</param>
    /// <param name="escalateToUserId">User to escalate to</param>
    /// <param name="reason">Escalation reason</param>
    /// <returns>Success response</returns>
    [HttpPost("{id}/escalate")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> EscalateApproval(
        Guid id, 
        [FromQuery] Guid escalateToUserId, 
        [FromQuery] string reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!IsValidUserId(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _approvalService.EscalateApprovalAsync(id, escalateToUserId, reason, userId);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating approval");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while escalating approval"
            });
        }
    }

    /// <summary>
    /// Get pending approvals for the current user
    /// </summary>
    /// <param name="pageNumber">Page number</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>Paginated pending approvals</returns>
    [HttpGet("pending-approvals")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<WorkRequestDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<PagedResult<WorkRequestDto>>>> GetPendingApprovals(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!IsValidUserId(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _approvalService.GetPendingApprovalsAsync(userId, pageNumber, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while getting pending approvals"
            });
        }
    }

    /// <summary>
    /// Get approval statistics
    /// </summary>
    /// <returns>Approval statistics</returns>
    [HttpGet("approval-statistics")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<ApprovalStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<ApprovalStatisticsDto>>> GetApprovalStatistics()
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.GetApprovalStatisticsAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting approval statistics");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while getting approval statistics"
            });
        }
    }

    /// <summary>
    /// Process bulk approvals
    /// </summary>
    /// <param name="request">Bulk approval request</param>
    /// <returns>Success response</returns>
    [HttpPost("bulk-approval")]
    [Authorize(Roles = "Manager,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> BulkApproval([FromBody] BulkApprovalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!IsValidUserId(userId))
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    Message = "User not authenticated"
                });
            }

            var result = await _approvalService.BulkApprovalAsync(request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing bulk approval");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while processing bulk approval"
            });
        }
    }

    /// <summary>
    /// Send approval reminders for overdue requests
    /// </summary>
    /// <returns>Success response</returns>
    [HttpPost("send-approval-reminders")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<bool>>> SendApprovalReminders()
    {
        try
        {
            var result = await _approvalService.SendApprovalRemindersAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending approval reminders");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "An error occurred while sending approval reminders"
            });
        }
    }

    #endregion

    #region Helper Methods

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
    
    /// <summary>
    /// Validates if user ID is valid (not empty)
    /// </summary>
    /// <param name="userId">User ID to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private bool IsValidUserId(Guid userId)
    {
        return userId != Guid.Empty;
    }

    private void AddHateoasLinks(WorkRequestDto workRequest)
    {
        workRequest.Links = new List<LinkDto>
        {
            new LinkDto
            {
                Href = Url.Action(nameof(GetWorkRequest), new { id = workRequest.WorkRequestId }) ?? "",
                Rel = "self",
                Method = "GET"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(UpdateWorkRequest), new { id = workRequest.WorkRequestId }) ?? "",
                Rel = "update",
                Method = "PUT"
            },
            new LinkDto
            {
                Href = Url.Action(nameof(DeleteWorkRequest), new { id = workRequest.WorkRequestId }) ?? "",
                Rel = "delete",
                Method = "DELETE"
            }
        };

        // Add status-specific actions
        if (workRequest.Status == "Open" || workRequest.Status == "InProgress")
        {
            workRequest.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(CompleteWorkRequest), new { id = workRequest.WorkRequestId }) ?? "",
                Rel = "complete",
                Method = "POST"
            });
        }

        if (workRequest.AssignedToId == null)
        {
            workRequest.Links.Add(new LinkDto
            {
                Href = Url.Action(nameof(AssignWorkRequest), new { id = workRequest.WorkRequestId, userId = "{userId}" }) ?? "",
                Rel = "assign",
                Method = "POST"
            });
        }
    }

    #endregion
}
