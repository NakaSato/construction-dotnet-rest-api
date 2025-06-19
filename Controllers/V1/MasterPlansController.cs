using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using dotnet_rest_api.Controllers;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing master plans and overall project progress
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/master-plans")]
[Authorize]
public class MasterPlansController : BaseApiController
{
    private readonly IMasterPlanService _masterPlanService;
    private readonly ILogger<MasterPlansController> _logger;

    public MasterPlansController(
        IMasterPlanService masterPlanService,
        ILogger<MasterPlansController> logger)
    {
        _masterPlanService = masterPlanService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new master plan for a project
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> CreateMasterPlan([FromBody] CreateMasterPlanRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateMasterPlan", request);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<MasterPlanDto>("Invalid input data", 400, errors);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<MasterPlanDto>("Invalid user ID in token", 401);

            var result = await _masterPlanService.CreateMasterPlanAsync(request, userId);
            return ToApiResponse(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while creating master plan");
            return CreateErrorResponse<MasterPlanDto>($"Invalid input: {ex.Message}", 400);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access while creating master plan");
            return CreateErrorResponse<MasterPlanDto>("Access denied", 403);
        }
        catch (Exception ex)
        {
            return HandleException<MasterPlanDto>(_logger, ex, "creating master plan");
        }
    }

    /// <summary>
    /// Get master plan by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> GetMasterPlan(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetMasterPlan", new { id });

            var result = await _masterPlanService.GetMasterPlanDtoByIdAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<MasterPlanDto>(_logger, ex, "retrieving master plan");
        }
    }

    /// <summary>
    /// Get master plan by project ID
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> GetMasterPlanByProject(Guid projectId)
    {
        try
        {
            LogControllerAction(_logger, "GetMasterPlanByProject", new { projectId });

            var result = await _masterPlanService.GetMasterPlanByProjectIdAsync(projectId);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<MasterPlanDto>(_logger, ex, "retrieving master plan by project");
        }
    }

    /// <summary>
    /// Update master plan details
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> UpdateMasterPlan(Guid id, [FromBody] UpdateMasterPlanRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdateMasterPlan", new { id, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse<MasterPlanDto>("Invalid input data", 400);

            var result = await _masterPlanService.UpdateMasterPlanAsync(id, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<MasterPlanDto>(_logger, ex, "updating master plan");
        }
    }

    /// <summary>
    /// Approve master plan
    /// Available to: Administrators
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = "Administrator")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> ApproveMasterPlan(Guid id, [FromBody] string? notes = null)
    {
        try
        {
            LogControllerAction(_logger, "ApproveMasterPlan", new { id, notes });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<bool>("Invalid user ID in token", 401);

            var result = await _masterPlanService.ApproveMasterPlanAsync(id, userId, notes);
            return result.IsSuccess ? 
                Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "approving master plan");
        }
    }

    /// <summary>
    /// Activate master plan (start project execution)
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> ActivateMasterPlan(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "ActivateMasterPlan", new { id });

            var result = await _masterPlanService.ActivateMasterPlanAsync(id);
            return result.IsSuccess ? 
                Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "activating master plan");
        }
    }

    /// <summary>
    /// Get overall progress summary for a master plan
    /// </summary>
    [HttpGet("{id:guid}/progress")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<ProgressSummaryDto>>> GetProgressSummary(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetProgressSummary", new { id });

            var result = await _masterPlanService.GetProgressSummaryAsync(id);
            return result.IsSuccess ? 
                Ok(new ApiResponse<ProgressSummaryDto> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<ProgressSummaryDto> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<ProgressSummaryDto>(_logger, ex, "retrieving progress summary");
        }
    }

    /// <summary>
    /// Calculate overall completion percentage
    /// </summary>
    [HttpGet("{id:guid}/completion")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<decimal>>> GetOverallProgress(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetOverallProgress", new { id });

            var result = await _masterPlanService.CalculateOverallProgressAsync(id);
            return result.IsSuccess ? 
                Ok(new ApiResponse<decimal> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<decimal> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<decimal>(_logger, ex, "calculating overall progress");
        }
    }

    /// <summary>
    /// Get all phases in a master plan
    /// </summary>
    [HttpGet("{id:guid}/phases")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectPhaseDto>>>> GetPhases(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetPhases", new { id });

            var result = await _masterPlanService.GetPhasesByMasterPlanAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<List<ProjectPhaseDto>>(_logger, ex, "retrieving phases");
        }
    }

    /// <summary>
    /// Add a new phase to master plan
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPost("{id:guid}/phases")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ProjectPhaseDto>>> AddPhase(Guid id, [FromBody] CreateProjectPhaseRequest request)
    {
        try
        {
            LogControllerAction(_logger, "AddPhase", new { id, request });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<ProjectPhaseDto>("Invalid input data", 400, errors);
            }

            var result = await _masterPlanService.AddPhaseToMasterPlanAsync(id, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ProjectPhaseDto>(_logger, ex, "adding phase");
        }
    }

    /// <summary>
    /// Update phase progress
    /// Available to: Project Managers, Site Supervisors
    /// </summary>
    [HttpPatch("{masterPlanId:guid}/phases/{phaseId:guid}/progress")]
    [Authorize(Roles = "Administrator,ProjectManager,SiteSupervisor")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePhaseProgress(
        Guid masterPlanId, 
        Guid phaseId, 
        [FromBody] UpdatePhaseProgressRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdatePhaseProgress", new { masterPlanId, phaseId, request });

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return CreateErrorResponse<bool>("Invalid input data", 400, errors);
            }

            // Validate percentage range
            if (request.CompletionPercentage < 0 || request.CompletionPercentage > 100)
            {
                return CreateErrorResponse<bool>("Completion percentage must be between 0 and 100", 400);
            }

            // Validate status
            var validStatuses = new[] { "NotStarted", "InProgress", "Completed", "OnHold", "Cancelled" };
            if (!validStatuses.Contains(request.Status))
            {
                return CreateErrorResponse<bool>($"Invalid status. Valid statuses are: {string.Join(", ", validStatuses)}", 400);
            }

            var result = await _masterPlanService.UpdatePhaseProgressAsync(phaseId, request.CompletionPercentage, request.Status);
            return ToApiResponse(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument while updating phase progress");
            return CreateErrorResponse<bool>($"Invalid input: {ex.Message}", 400);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Phase not found while updating progress");
            return CreateErrorResponse<bool>($"Phase with ID {phaseId} not found", 404);
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "updating phase progress");
        }
    }

    /// <summary>
    /// Get all milestones in a master plan
    /// </summary>
    [HttpGet("{id:guid}/milestones")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectMilestoneDto>>>> GetMilestones(Guid id)
    {
        try
        {
            LogControllerAction(_logger, "GetMilestones", new { id });

            var result = await _masterPlanService.GetMilestonesByMasterPlanAsync(id);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<List<ProjectMilestoneDto>>(_logger, ex, "retrieving milestones");
        }
    }

    /// <summary>
    /// Add a new milestone to master plan
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPost("{id:guid}/milestones")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ProjectMilestoneDto>>> AddMilestone(Guid id, [FromBody] CreateProjectMilestoneRequest request)
    {
        try
        {
            LogControllerAction(_logger, "AddMilestone", new { id, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse<ProjectMilestoneDto>("Invalid input data", 400);

            var result = await _masterPlanService.AddMilestoneToMasterPlanAsync(id, request);
            return ToApiResponse(result);
        }
        catch (Exception ex)
        {
            return HandleException<ProjectMilestoneDto>(_logger, ex, "adding milestone");
        }
    }

    /// <summary>
    /// Complete a milestone
    /// Available to: Project Managers, Site Supervisors
    /// </summary>
    [HttpPost("{masterPlanId:guid}/milestones/{milestoneId:guid}/complete")]
    [Authorize(Roles = "Administrator,ProjectManager,SiteSupervisor")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> CompleteMilestone(
        Guid masterPlanId, 
        Guid milestoneId, 
        [FromBody] string? evidence = null)
    {
        try
        {
            LogControllerAction(_logger, "CompleteMilestone", new { masterPlanId, milestoneId, evidence });

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<bool>("Invalid user ID in token", 401);

            var result = await _masterPlanService.CompleteMilestoneAsync(milestoneId, userId, evidence);
            return result.IsSuccess ? 
                Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "completing milestone");
        }
    }

    /// <summary>
    /// Get upcoming milestones (next 30 days by default)
    /// </summary>
    [HttpGet("{id:guid}/milestones/upcoming")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectMilestoneDto>>>> GetUpcomingMilestones(Guid id, [FromQuery] int days = 30)
    {
        try
        {
            LogControllerAction(_logger, "GetUpcomingMilestones", new { id, days });

            var result = await _masterPlanService.GetUpcomingMilestonesAsync(id, days);
            return result.IsSuccess ? 
                Ok(new ApiResponse<List<ProjectMilestoneDto>> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<List<ProjectMilestoneDto>> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<List<ProjectMilestoneDto>>(_logger, ex, "retrieving upcoming milestones");
        }
    }

    /// <summary>
    /// Create a progress report
    /// Available to: Project Managers, Site Supervisors
    /// </summary>
    [HttpPost("{id:guid}/progress-reports")]
    [Authorize(Roles = "Administrator,ProjectManager,SiteSupervisor")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ProgressReportDto>>> CreateProgressReport(Guid id, [FromBody] CreateProgressReportRequest request)
    {
        try
        {
            LogControllerAction(_logger, "CreateProgressReport", new { id, request });

            if (!ModelState.IsValid)
                return CreateErrorResponse<ProgressReportDto>("Invalid input data", 400);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<ProgressReportDto>("Invalid user ID in token", 401);

            var result = await _masterPlanService.CreateProgressReportAsync(id, request, userId);
            return result.IsSuccess ? 
                Ok(new ApiResponse<ProgressReportDto> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<ProgressReportDto> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<ProgressReportDto>(_logger, ex, "creating progress report");
        }
    }

    /// <summary>
    /// Get progress reports with pagination
    /// </summary>
    [HttpGet("{id:guid}/progress-reports")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<List<ProgressReportDto>>>> GetProgressReports(
        Guid id, 
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            LogControllerAction(_logger, "GetProgressReports", new { id, pageNumber, pageSize });

            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return BadRequest(CreateErrorResponse(validationResult));

            var result = await _masterPlanService.GetProgressReportsAsync(id, pageNumber, pageSize);
            return result.IsSuccess ? 
                Ok(new ApiResponse<List<ProgressReportDto>> { Success = true, Data = result.Data }) : 
                BadRequest(new ApiResponse<List<ProgressReportDto>> { Success = false, Message = "Failed to retrieve progress reports" });
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Master plan not found while retrieving progress reports");
            return NotFound(new ApiResponse<List<ProgressReportDto>> { Success = false, Message = $"Master plan with ID {id} not found" });
        }
        catch (Exception ex)
        {
            return HandleException<List<ProgressReportDto>>(_logger, ex, "retrieving progress reports");
        }
    }
}
