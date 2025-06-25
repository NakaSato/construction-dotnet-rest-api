using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Attributes;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Top-level phases controller following canonical URI structure
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/phases")]
[Authorize]
public class PhasesController : BaseApiController
{
    private readonly IMasterPlanService _masterPlanService;
    private readonly ITaskService _taskService;
    private readonly ILogger<PhasesController> _logger;

    public PhasesController(
        IMasterPlanService masterPlanService,
        ITaskService taskService,
        ILogger<PhasesController> logger)
    {
        _masterPlanService = masterPlanService;
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// Get a specific phase by ID (canonical top-level resource)
    /// </summary>
    [HttpGet("{phaseId:guid}")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<ProjectPhaseDto>>> GetPhase(Guid phaseId)
    {
        try
        {
            LogControllerAction(_logger, "GetPhase", new { phaseId });

            var result = await _masterPlanService.GetPhaseByIdAsync(phaseId);
            return result.IsSuccess ? 
                Ok(new ApiResponse<ProjectPhaseDto> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<ProjectPhaseDto> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<ProjectPhaseDto>(_logger, ex, "retrieving phase");
        }
    }

    /// <summary>
    /// Update a phase (canonical top-level resource)
    /// Available to: Project Managers, Administrators
    /// </summary>
    [HttpPatch("{phaseId:guid}")]
    [Authorize(Roles = "Administrator,ProjectManager")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<ProjectPhaseDto>>> UpdatePhase(Guid phaseId, [FromBody] UpdateProjectPhaseRequest request)
    {
        try
        {
            LogControllerAction(_logger, "UpdatePhase", new { phaseId, request });

            if (!ModelState.IsValid)
                return BadRequest(new ApiResponse<TaskDto> { Success = false, Message = "Invalid input data" });

            var result = await _masterPlanService.UpdatePhaseAsync(phaseId, request);
            return result.IsSuccess ? 
                Ok(new ApiResponse<ProjectPhaseDto> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<ProjectPhaseDto> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<ProjectPhaseDto>(_logger, ex, "updating phase");
        }
    }

    /// <summary>
    /// Delete a phase (canonical top-level resource)
    /// Available to: Administrators
    /// </summary>
    [HttpDelete("{phaseId:guid}")]
    [Authorize(Roles = "Administrator")]
    [NoCache]
    public async Task<ActionResult<ApiResponse<bool>>> DeletePhase(Guid phaseId)
    {
        try
        {
            LogControllerAction(_logger, "DeletePhase", new { phaseId });

            var result = await _masterPlanService.DeletePhaseAsync(phaseId);
            return result.IsSuccess ? 
                Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
                BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
        }
        catch (Exception ex)
        {
            return HandleException<bool>(_logger, ex, "deleting phase");
        }
    }

    /// <summary>
    /// Get tasks for a specific phase (limited nesting: one level)
    /// </summary>
    [HttpGet("{phaseId:guid}/tasks")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<PagedResult<TaskDto>>>> GetPhaseTasks(
        Guid phaseId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            LogControllerAction(_logger, "GetPhaseTasks", new { phaseId, pageNumber, pageSize });

            var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
            if (validationResult != null)
                return BadRequest(CreateErrorResponse("Error"));

            var result = await _taskService.GetPhaseTasksAsync(phaseId, pageNumber, pageSize);
            return ToApiResponse<PagedResult<TaskDto>>(result);
        }
        catch (Exception ex)
        {
            return HandleException<PagedResult<TaskDto>>(_logger, ex, "retrieving phase tasks");
        }
    }
}
