using dotnet_rest_api.Common;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Interfaces;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Services.Commands;

/// <summary>
/// Command pattern implementation for master plan operations
/// Reduces controller complexity by encapsulating business logic
/// </summary>

#region Master Plan Commands

public class CreateMasterPlanCommand : ICommand<MasterPlanDto>
{
    public CreateMasterPlanRequest Request { get; set; } = null!;
    public Guid CreatedById { get; set; }

    public Task<Result<MasterPlanDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class UpdateMasterPlanCommand : ICommand<MasterPlanDto>
{
    public Guid MasterPlanId { get; set; }
    public UpdateMasterPlanRequest Request { get; set; } = null!;

    public Task<Result<MasterPlanDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class ApproveMasterPlanCommand : ICommand<bool>
{
    public Guid MasterPlanId { get; set; }
    public Guid ApprovedById { get; set; }
    public string? Notes { get; set; }

    public Task<Result<bool>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class ActivateMasterPlanCommand : ICommand<bool>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<bool>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class AddPhaseCommand : ICommand<ProjectPhaseDto>
{
    public Guid MasterPlanId { get; set; }
    public CreateProjectPhaseRequest Request { get; set; } = null!;

    public Task<Result<ProjectPhaseDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class UpdatePhaseProgressCommand : ICommand<bool>
{
    public Guid PhaseId { get; set; }
    public decimal CompletionPercentage { get; set; }
    public PhaseStatus Status { get; set; }

    public Task<Result<bool>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class AddMilestoneCommand : ICommand<ProjectMilestoneDto>
{
    public Guid MasterPlanId { get; set; }
    public CreateProjectMilestoneRequest Request { get; set; } = null!;

    public Task<Result<ProjectMilestoneDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class CompleteMilestoneCommand : ICommand<bool>
{
    public Guid MilestoneId { get; set; }
    public Guid CompletedById { get; set; }
    public string? Evidence { get; set; }

    public Task<Result<bool>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class CreateProgressReportCommand : ICommand<ProgressReportDto>
{
    public Guid MasterPlanId { get; set; }
    public CreateProgressReportRequest Request { get; set; } = null!;
    public Guid CreatedById { get; set; }

    public Task<Result<ProgressReportDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

#endregion

#region Query Commands

public class GetMasterPlanQuery : IQuery<MasterPlanDto>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<MasterPlanDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetMasterPlanByProjectQuery : IQuery<MasterPlanDto>
{
    public Guid ProjectId { get; set; }

    public Task<Result<MasterPlanDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetProgressSummaryQuery : IQuery<ProgressSummaryDto>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<ProgressSummaryDto>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetOverallProgressQuery : IQuery<decimal>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<decimal>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetPhasesQuery : IQuery<List<ProjectPhaseDto>>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<List<ProjectPhaseDto>>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetMilestonesQuery : IQuery<List<ProjectMilestoneDto>>
{
    public Guid MasterPlanId { get; set; }

    public Task<Result<List<ProjectMilestoneDto>>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetUpcomingMilestonesQuery : IQuery<List<ProjectMilestoneDto>>
{
    public Guid MasterPlanId { get; set; }
    public int Days { get; set; } = 30;

    public Task<Result<List<ProjectMilestoneDto>>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetProgressReportsQuery : IQuery<List<ProgressReportDto>>
{
    public Guid MasterPlanId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public Task<Result<List<ProgressReportDto>>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

public class GetAllMasterPlansQuery : IQuery<List<MasterPlanDto>>
{
    /// <summary>
    /// Page number for pagination (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (default: 50, max: 100)
    /// </summary>
    public int PageSize { get; set; } = 50;

    public Task<Result<List<MasterPlanDto>>> ExecuteAsync()
    {
        throw new NotImplementedException("Use handler");
    }
}

#endregion
