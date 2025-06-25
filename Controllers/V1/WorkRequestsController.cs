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
    /// Get all work requests with advanced filtering
    /// </summary>
    [HttpGet]
    [MediumCache]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<WorkRequestDto>>>> GetWorkRequests([FromQuery] WorkRequestQueryParameters parameters)
    {
        try
        {
            LogControllerAction(_logger, "GetWorkRequests", parameters);
            var result = await _workRequestService.GetWorkRequestsAsync(parameters);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<EnhancedPagedResult<WorkRequestDto>>(_logger, ex, "retrieving work requests");
        }
    }

    /// <summary>
    /// Get work request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [LongCache]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> GetWorkRequest(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetWorkRequest", new { id });
            var result = await _workRequestService.GetWorkRequestByIdAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"retrieving work request {id}");
        }
    }

    /// <summary>
    /// Create a new work request
    /// </summary>
    [HttpPost]
    [NoCache]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> CreateWorkRequest([FromBody] CreateWorkRequestRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateWorkRequest", request);

            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Invalid input data"));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(CreateErrorResponse("Invalid user ID in token"));
            }

            var result = await _workRequestService.CreateWorkRequestAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, "creating work request");
        }
    }

    /// <summary>
    /// Update an existing work request
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> UpdateWorkRequest(Guid id, [FromBody] UpdateWorkRequestRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdateWorkRequest", new { id, request });

            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Invalid input data"));
            }

            var result = await _workRequestService.UpdateWorkRequestAsync(id, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"updating work request {id}");
        }
    }

    /// <summary>
    /// Delete a work request
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [CriticalDeleteRateLimit]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteWorkRequest(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "DeleteWorkRequest", new { id });
            var result = await _workRequestService.DeleteWorkRequestAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"deleting work request {id}");
        }
    }

    /// <summary>
    /// Assign work request to user
    /// </summary>
    [HttpPost("{id:guid}/assign/{userId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> AssignWorkRequest(Guid id, Guid userId)
    {
        try
        {
            LogControllerAction(_logger, "AssignWorkRequest", new { id, userId });
            var result = await _workRequestService.AssignWorkRequestAsync(id, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"assigning work request {id}");
        }
    }

    /// <summary>
    /// Complete a work request
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<WorkRequestDto>>> CompleteWorkRequest(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "CompleteWorkRequest", new { id });
            var result = await _workRequestService.CompleteWorkRequestAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<WorkRequestDto>(_logger, ex, $"completing work request {id}");
        }
    }

    /// <summary>
    /// Submit work request for approval
    /// </summary>
    [HttpPost("{id:guid}/submit-for-approval")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> SubmitForApproval(Guid id, [FromBody] SubmitForApprovalRequest request)
    {
        try
        {
            LogControllerAction(_logger, "SubmitForApproval", new { id, request });

            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Invalid input data"));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(CreateErrorResponse("Invalid user ID in token"));
            }

            var result = await _approvalService.SubmitForApprovalAsync(id, request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"submitting work request {id} for approval");
        }
    }

    /// <summary>
    /// Process approval/rejection for work request
    /// </summary>
    [HttpPost("{id:guid}/process-approval")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> ProcessApproval(Guid id, [FromBody] ApprovalRequest request)
    {
        try
        {
            LogControllerAction(_logger, "ProcessApproval", new { id, request });

            if (!ModelState.IsValid)
            {
                return BadRequest(CreateErrorResponse("Invalid input data"));
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(CreateErrorResponse("Invalid user ID in token"));
            }

            var result = await _approvalService.ProcessApprovalAsync(id, request, userId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, $"processing approval for work request {id}");
        }
    }

    /// <summary>
    /// Get approval status for work request
    /// </summary>
    [HttpGet("{id:guid}/approval-status")]
    [ShortCache]
    public async Task<ActionResult<ApiResponse<ApprovalWorkflowStatusDto>>> GetApprovalStatus(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetApprovalStatus", new { id });
            var result = await _approvalService.GetApprovalStatusAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ApprovalWorkflowStatusDto>(_logger, ex, $"retrieving approval status for work request {id}");
        }
    }
}
