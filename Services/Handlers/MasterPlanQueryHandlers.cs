using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Commands;
using dotnet_rest_api.Services.Interfaces;

namespace dotnet_rest_api.Services.Handlers;

/// <summary>
/// Query handlers for master plan read operations
/// Separates read and write operations following CQRS pattern
/// </summary>

#region Master Plan Query Handlers

public class GetMasterPlanQueryHandler : IQueryHandler<GetMasterPlanQuery, MasterPlanDto>
{
    private readonly IMasterPlanCrudService _crudService;
    private readonly ILogger<GetMasterPlanQueryHandler> _logger;

    public GetMasterPlanQueryHandler(IMasterPlanCrudService crudService, ILogger<GetMasterPlanQueryHandler> logger)
    {
        _crudService = crudService;
        _logger = logger;
    }

    public async Task<Result<MasterPlanDto>> HandleAsync(GetMasterPlanQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving master plan {MasterPlanId}", query.MasterPlanId);
            return await _crudService.GetByIdAsync(query.MasterPlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetMasterPlanQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<MasterPlanDto>.Failure($"Error retrieving master plan: {ex.Message}");
        }
    }
}

public class GetMasterPlanByProjectQueryHandler : IQueryHandler<GetMasterPlanByProjectQuery, MasterPlanDto>
{
    private readonly IMasterPlanCrudService _crudService;
    private readonly ILogger<GetMasterPlanByProjectQueryHandler> _logger;

    public GetMasterPlanByProjectQueryHandler(IMasterPlanCrudService crudService, ILogger<GetMasterPlanByProjectQueryHandler> logger)
    {
        _crudService = crudService;
        _logger = logger;
    }

    public async Task<Result<MasterPlanDto>> HandleAsync(GetMasterPlanByProjectQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving master plan for project {ProjectId}", query.ProjectId);
            return await _crudService.GetByProjectIdAsync(query.ProjectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetMasterPlanByProjectQuery for project {ProjectId}", query.ProjectId);
            return Result<MasterPlanDto>.Failure($"Error retrieving master plan by project: {ex.Message}");
        }
    }
}

public class GetProgressSummaryQueryHandler : IQueryHandler<GetProgressSummaryQuery, ProgressSummaryDto>
{
    private readonly IMasterPlanAnalyticsService _analyticsService;
    private readonly ILogger<GetProgressSummaryQueryHandler> _logger;

    public GetProgressSummaryQueryHandler(IMasterPlanAnalyticsService analyticsService, ILogger<GetProgressSummaryQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<Result<ProgressSummaryDto>> HandleAsync(GetProgressSummaryQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving progress summary for master plan {MasterPlanId}", query.MasterPlanId);
            return await _analyticsService.GetProgressSummaryAsync(query.MasterPlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetProgressSummaryQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<ProgressSummaryDto>.Failure($"Error retrieving progress summary: {ex.Message}");
        }
    }
}

public class GetOverallProgressQueryHandler : IQueryHandler<GetOverallProgressQuery, decimal>
{
    private readonly IMasterPlanAnalyticsService _analyticsService;
    private readonly ILogger<GetOverallProgressQueryHandler> _logger;

    public GetOverallProgressQueryHandler(IMasterPlanAnalyticsService analyticsService, ILogger<GetOverallProgressQueryHandler> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    public async Task<Result<decimal>> HandleAsync(GetOverallProgressQuery query)
    {
        try
        {
            _logger.LogInformation("Calculating overall progress for master plan {MasterPlanId}", query.MasterPlanId);
            return await _analyticsService.CalculateOverallProgressAsync(query.MasterPlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetOverallProgressQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<decimal>.Failure($"Error calculating overall progress: {ex.Message}");
        }
    }
}

public class GetPhasesQueryHandler : IQueryHandler<GetPhasesQuery, List<ProjectPhaseDto>>
{
    private readonly IPhaseManagementService _phaseService;
    private readonly ILogger<GetPhasesQueryHandler> _logger;

    public GetPhasesQueryHandler(IPhaseManagementService phaseService, ILogger<GetPhasesQueryHandler> logger)
    {
        _phaseService = phaseService;
        _logger = logger;
    }

    public async Task<Result<List<ProjectPhaseDto>>> HandleAsync(GetPhasesQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving phases for master plan {MasterPlanId}", query.MasterPlanId);
            return await _phaseService.GetPhasesByMasterPlanAsync(query.MasterPlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetPhasesQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<List<ProjectPhaseDto>>.Failure($"Error retrieving phases: {ex.Message}");
        }
    }
}

public class GetMilestonesQueryHandler : IQueryHandler<GetMilestonesQuery, List<ProjectMilestoneDto>>
{
    private readonly IMilestoneService _milestoneService;
    private readonly ILogger<GetMilestonesQueryHandler> _logger;

    public GetMilestonesQueryHandler(IMilestoneService milestoneService, ILogger<GetMilestonesQueryHandler> logger)
    {
        _milestoneService = milestoneService;
        _logger = logger;
    }

    public async Task<Result<List<ProjectMilestoneDto>>> HandleAsync(GetMilestonesQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving milestones for master plan {MasterPlanId}", query.MasterPlanId);
            return await _milestoneService.GetMilestonesByMasterPlanAsync(query.MasterPlanId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetMilestonesQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving milestones: {ex.Message}");
        }
    }
}

public class GetUpcomingMilestonesQueryHandler : IQueryHandler<GetUpcomingMilestonesQuery, List<ProjectMilestoneDto>>
{
    private readonly IMilestoneService _milestoneService;
    private readonly ILogger<GetUpcomingMilestonesQueryHandler> _logger;

    public GetUpcomingMilestonesQueryHandler(IMilestoneService milestoneService, ILogger<GetUpcomingMilestonesQueryHandler> logger)
    {
        _milestoneService = milestoneService;
        _logger = logger;
    }

    public async Task<Result<List<ProjectMilestoneDto>>> HandleAsync(GetUpcomingMilestonesQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving upcoming milestones for master plan {MasterPlanId}, days: {Days}", 
                query.MasterPlanId, query.Days);
            return await _milestoneService.GetUpcomingMilestonesAsync(query.MasterPlanId, query.Days);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetUpcomingMilestonesQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<List<ProjectMilestoneDto>>.Failure($"Error retrieving upcoming milestones: {ex.Message}");
        }
    }
}

public class GetProgressReportsQueryHandler : IQueryHandler<GetProgressReportsQuery, List<ProgressReportDto>>
{
    private readonly IMasterPlanReportingService _reportingService;
    private readonly ILogger<GetProgressReportsQueryHandler> _logger;

    public GetProgressReportsQueryHandler(IMasterPlanReportingService reportingService, ILogger<GetProgressReportsQueryHandler> logger)
    {
        _reportingService = reportingService;
        _logger = logger;
    }

    public async Task<Result<List<ProgressReportDto>>> HandleAsync(GetProgressReportsQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving progress reports for master plan {MasterPlanId}, page: {PageNumber}, size: {PageSize}", 
                query.MasterPlanId, query.PageNumber, query.PageSize);
            return await _reportingService.GetProgressReportsAsync(query.MasterPlanId, query.PageNumber, query.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetProgressReportsQuery for {MasterPlanId}", query.MasterPlanId);
            return Result<List<ProgressReportDto>>.Failure($"Error retrieving progress reports: {ex.Message}");
        }
    }
}

public class GetAllMasterPlansQueryHandler : IQueryHandler<GetAllMasterPlansQuery, List<MasterPlanDto>>
{
    private readonly IMasterPlanCrudService _crudService;
    private readonly ILogger<GetAllMasterPlansQueryHandler> _logger;

    public GetAllMasterPlansQueryHandler(IMasterPlanCrudService crudService, ILogger<GetAllMasterPlansQueryHandler> logger)
    {
        _crudService = crudService;
        _logger = logger;
    }

    public async Task<Result<List<MasterPlanDto>>> HandleAsync(GetAllMasterPlansQuery query)
    {
        try
        {
            _logger.LogInformation("Retrieving all master plans (page: {PageNumber}, size: {PageSize})", 
                query.PageNumber, query.PageSize);
            return await _crudService.GetAllAsync(query.PageNumber, query.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling GetAllMasterPlansQuery (page: {PageNumber}, size: {PageSize})", 
                query.PageNumber, query.PageSize);
            return Result<List<MasterPlanDto>>.Failure($"Error retrieving master plans: {ex.Message}");
        }
    }
}

#endregion
