using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services;
using dotnet_rest_api.Services.Commands;
using dotnet_rest_api.Services.Interfaces;
using dotnet_rest_api.Attributes;
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
        ILogger<MasterPlansController> logger,
        IUserContextService userContextService,
        IResponseBuilderService responseBuilderService,
        IValidationHelperService validationHelperService)
        : base(logger, userContextService, responseBuilderService)
    {
        _masterPlanService = masterPlanService;
        _logger = logger;
    }

    /// <summary>
    /// Get all master plans with pagination
    /// </summary>
    [HttpGet]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<List<MasterPlanDto>>>> GetAllMasterPlans([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 50)
    {
        LogControllerAction(_logger, "GetAllMasterPlans", new { pageNumber, pageSize });

        var query = new GetAllMasterPlansQuery { PageNumber = pageNumber, PageSize = pageSize };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetAllMasterPlansQuery, List<MasterPlanDto>>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<List<MasterPlanDto>> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<List<MasterPlanDto>> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "CreateMasterPlan", request);

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<MasterPlanDto>();

        var userId = GetCurrentUserId();
        if (userId == null)
            return CreateErrorResponse<MasterPlanDto>("User not authenticated", 401);

        var command = new CreateMasterPlanCommand
        {
            Request = request,
            CreatedById = userId.Value
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<CreateMasterPlanCommand, MasterPlanDto>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            CreateSuccessResponse(result.Data!, result.Message) : 
            CreateErrorResponse<MasterPlanDto>(result.Message ?? "Operation failed", 400);
    }

    /// <summary>
    /// Get master plan by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> GetMasterPlan(Guid id)
    {
        LogControllerAction(_logger, "GetMasterPlan", new { id });

        var query = new GetMasterPlanQuery { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetMasterPlanQuery, MasterPlanDto>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<MasterPlanDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<MasterPlanDto> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Get master plan by project ID
    /// </summary>
    [HttpGet("project/{projectId:guid}")]
    [LongCache] // 1 hour cache
    public async Task<ActionResult<ApiResponse<MasterPlanDto>>> GetMasterPlanByProject(Guid projectId)
    {
        LogControllerAction(_logger, "GetMasterPlanByProject", new { projectId });

        var query = new GetMasterPlanByProjectQuery { ProjectId = projectId };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetMasterPlanByProjectQuery, MasterPlanDto>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<MasterPlanDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<MasterPlanDto> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "UpdateMasterPlan", new { id, request });

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<MasterPlanDto>();

        var command = new UpdateMasterPlanCommand
        {
            MasterPlanId = id,
            Request = request
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<UpdateMasterPlanCommand, MasterPlanDto>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            CreateSuccessResponse(result.Data!, result.Message) : 
            CreateErrorResponse<MasterPlanDto>(result.Message ?? "Operation failed", 400);
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
        LogControllerAction(_logger, "ApproveMasterPlan", new { id, notes });

        var userId = GetCurrentUserId();
        if (userId == null)
            return CreateErrorResponse<bool>("User not authenticated", 401);

        var command = new ApproveMasterPlanCommand
        {
            MasterPlanId = id,
            ApprovedById = userId.Value,
            Notes = notes
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<ApproveMasterPlanCommand, bool>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "ActivateMasterPlan", new { id });

        var command = new ActivateMasterPlanCommand { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<ActivateMasterPlanCommand, bool>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Get overall progress summary for a master plan
    /// </summary>
    [HttpGet("{id:guid}/progress")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<ProgressSummaryDto>>> GetProgressSummary(Guid id)
    {
        LogControllerAction(_logger, "GetProgressSummary", new { id });

        var query = new GetProgressSummaryQuery { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetProgressSummaryQuery, ProgressSummaryDto>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<ProgressSummaryDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<ProgressSummaryDto> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Calculate overall completion percentage
    /// </summary>
    [HttpGet("{id:guid}/completion")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<decimal>>> GetOverallProgress(Guid id)
    {
        LogControllerAction(_logger, "GetOverallProgress", new { id });

        var query = new GetOverallProgressQuery { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetOverallProgressQuery, decimal>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<decimal> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<decimal> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Get all phases in a master plan
    /// </summary>
    [HttpGet("{id:guid}/phases")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectPhaseDto>>>> GetPhases(Guid id)
    {
        LogControllerAction(_logger, "GetPhases", new { id });

        var query = new GetPhasesQuery { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetPhasesQuery, List<ProjectPhaseDto>>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<List<ProjectPhaseDto>> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<List<ProjectPhaseDto>> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "AddPhase", new { id, request });

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<ProjectPhaseDto>();

        var command = new AddPhaseCommand
        {
            MasterPlanId = id,
            Request = request
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<AddPhaseCommand, ProjectPhaseDto>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<ProjectPhaseDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<ProjectPhaseDto> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "UpdatePhaseProgress", new { masterPlanId, phaseId, request });

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<bool>();

        var command = new UpdatePhaseProgressCommand
        {
            PhaseId = phaseId,
            CompletionPercentage = request.CompletionPercentage,
            Status = request.Status
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<UpdatePhaseProgressCommand, bool>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Get all milestones in a master plan
    /// </summary>
    [HttpGet("{id:guid}/milestones")]
    [MediumCache] // 15 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectMilestoneDto>>>> GetMilestones(Guid id)
    {
        LogControllerAction(_logger, "GetMilestones", new { id });

        var query = new GetMilestonesQuery { MasterPlanId = id };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetMilestonesQuery, List<ProjectMilestoneDto>>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<List<ProjectMilestoneDto>> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<List<ProjectMilestoneDto>> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "AddMilestone", new { id, request });

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<ProjectMilestoneDto>();

        var command = new AddMilestoneCommand
        {
            MasterPlanId = id,
            Request = request
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<AddMilestoneCommand, ProjectMilestoneDto>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<ProjectMilestoneDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<ProjectMilestoneDto> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "CompleteMilestone", new { masterPlanId, milestoneId, evidence });

        var userId = GetCurrentUserId();
        if (userId == null)
            return CreateErrorResponse<bool>("User not authenticated", 401);

        var command = new CompleteMilestoneCommand
        {
            MilestoneId = milestoneId,
            CompletedById = userId.Value,
            Evidence = evidence
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<CompleteMilestoneCommand, bool>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<bool> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<bool> { Success = false, Message = result.Message });
    }

    /// <summary>
    /// Get upcoming milestones (next 30 days by default)
    /// </summary>
    [HttpGet("{id:guid}/milestones/upcoming")]
    [ShortCache] // 5 minute cache
    public async Task<ActionResult<ApiResponse<List<ProjectMilestoneDto>>>> GetUpcomingMilestones(Guid id, [FromQuery] int days = 30)
    {
        LogControllerAction(_logger, "GetUpcomingMilestones", new { id, days });

        var query = new GetUpcomingMilestonesQuery { MasterPlanId = id, Days = days };
        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetUpcomingMilestonesQuery, List<ProjectMilestoneDto>>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<List<ProjectMilestoneDto>> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<List<ProjectMilestoneDto>> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "CreateProgressReport", new { id, request });

        if (!ModelState.IsValid)
            return CreateValidationErrorResponse<ProgressReportDto>();

        var userId = GetCurrentUserId();
        if (userId == null)
            return CreateErrorResponse<ProgressReportDto>("User not authenticated", 401);

        var command = new CreateProgressReportCommand
        {
            MasterPlanId = id,
            Request = request,
            CreatedById = userId.Value
        };

        var handler = HttpContext.RequestServices.GetRequiredService<ICommandHandler<CreateProgressReportCommand, ProgressReportDto>>();
        var result = await handler.HandleAsync(command);

        return result.IsSuccess ? 
            Ok(new ApiResponse<ProgressReportDto> { Success = true, Data = result.Data, Message = result.Message }) : 
            BadRequest(new ApiResponse<ProgressReportDto> { Success = false, Message = result.Message });
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
        LogControllerAction(_logger, "GetProgressReports", new { id, pageNumber, pageSize });

        var validationResult = ValidatePaginationParameters(pageNumber, pageSize);
        if (validationResult != null)
            return BadRequest(CreateErrorResponse(validationResult));

        var query = new GetProgressReportsQuery 
        { 
            MasterPlanId = id, 
            PageNumber = pageNumber, 
            PageSize = pageSize 
        };

        var handler = HttpContext.RequestServices.GetRequiredService<IQueryHandler<GetProgressReportsQuery, List<ProgressReportDto>>>();
        var result = await handler.HandleAsync(query);

        return result.IsSuccess ? 
            Ok(new ApiResponse<List<ProgressReportDto>> { Success = true, Data = result.Data }) : 
            BadRequest(new ApiResponse<List<ProgressReportDto>> { Success = false, Message = result.Message });
    }
}
