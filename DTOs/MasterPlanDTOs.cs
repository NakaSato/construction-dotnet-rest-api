using dotnet_rest_api.Models;

namespace dotnet_rest_api.DTOs;

// Master Plan DTOs
public class MasterPlanDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Budget { get; set; }
    public string? Priority { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string? ProjectName { get; set; }
    public string? CreatedByName { get; set; }
    public string? ApprovedByName { get; set; }
}



// Project Phase DTOs
public class ProjectPhaseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PhaseStatus Status { get; set; }
    public decimal CompletionPercentage { get; set; }
    public Guid MasterPlanId { get; set; }
    public int TasksCompleted { get; set; }
    public int TotalTasks { get; set; }
    public decimal ActualDurationDays { get; set; }
    public bool IsOnSchedule { get; set; }
    public bool IsOnBudget { get; set; }
}

public class CreateProjectPhaseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    // For compatibility with mapping profile
    public DateTime PlannedStartDate => StartDate;
    public DateTime PlannedEndDate => EndDate;
}

public class UpdateProjectPhaseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PhaseStatus Status { get; set; }
    public decimal CompletionPercentage { get; set; }
}

// Project Milestone DTOs
public class ProjectMilestoneDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid MasterPlanId { get; set; }
    public string? Evidence { get; set; }
    public string? PhaseName { get; set; }
    public string? VerifiedByName { get; set; }
    public int DaysFromPlanned { get; set; }
    public bool IsOverdue { get; set; }
}

public class CreateProjectMilestoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}

public class UpdateProjectMilestoneRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
}

// Progress Report DTOs

// Using enums from Models namespace

// Progress Report DTOs
public class ProgressReportDto
{
    public Guid ProgressReportId { get; set; }
    public Guid MasterPlanId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal OverallCompletionPercentage { get; set; }
    public decimal SchedulePerformanceIndex { get; set; }
    public decimal CostPerformanceIndex { get; set; }
    public decimal ActualCostToDate { get; set; }
    public decimal EstimatedCostAtCompletion { get; set; }
    public decimal BudgetVariance { get; set; }
    public int ScheduleVarianceDays { get; set; }
    public DateTime ProjectedCompletionDate { get; set; }
    public int ActiveIssuesCount { get; set; }
    public int CompletedMilestonesCount { get; set; }
    public int TotalMilestonesCount { get; set; }
    public ProjectHealthStatus HealthStatus { get; set; }
    public string? KeyAccomplishments { get; set; }
    public string? CurrentChallenges { get; set; }
    public string? UpcomingActivities { get; set; }
    public string? RiskSummary { get; set; }
    public string? QualityNotes { get; set; }
    public string? WeatherImpact { get; set; }
    public string? ResourceNotes { get; set; }
    public string? ExecutiveSummary { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    
    // Related data
    public string ProjectName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public List<PhaseProgressDto> PhaseProgressDetails { get; set; } = new();
}

public class PhaseProgressDto
{
    public Guid PhaseProgressId { get; set; }
    public Guid PhaseId { get; set; }
    public string PhaseName { get; set; } = string.Empty;
    public decimal CompletionPercentage { get; set; }
    public decimal PlannedCompletionPercentage { get; set; }
    public decimal ProgressVariance { get; set; }
    public PhaseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? ActivitiesCompleted { get; set; }
    public string? Issues { get; set; }
}



// Phase Resource DTOs
public class PhaseResourceDto
{
    public Guid PhaseResourceId { get; set; }
    public Guid PhaseId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalEstimatedCost { get; set; }
    public decimal ActualQuantityUsed { get; set; }
    public decimal ActualCost { get; set; }
    public DateTime RequiredDate { get; set; }
    public int DurationDays { get; set; }
    public ResourceAllocationStatus AllocationStatus { get; set; }
    public string? Supplier { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreatePhaseResourceRequest
{
    public ResourceType ResourceType { get; set; }
    public string ResourceName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public DateTime RequiredDate { get; set; }
    public int DurationDays { get; set; }
    public string? Supplier { get; set; }
    public string? Notes { get; set; }
}





public class TaskProgressReportDto
{
    public Guid ProgressReportId { get; set; }
    public Guid TaskId { get; set; }
    public DateTime ReportDate { get; set; }
    public decimal CompletionPercentage { get; set; }
    public decimal PlannedCompletionPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? WorkCompleted { get; set; }
    public string? Issues { get; set; }
    public string? NextSteps { get; set; }
    public decimal HoursWorked { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

public class CreateTaskProgressReportRequest
{
    public decimal CompletionPercentage { get; set; }
    public string? WorkCompleted { get; set; }
    public string? Issues { get; set; }
    public string? NextSteps { get; set; }
    public decimal HoursWorked { get; set; }
}

public class ProgressSummaryDto
{
    public decimal OverallCompletion { get; set; }
    public ProjectHealthStatus HealthStatus { get; set; }
    public int CompletedPhases { get; set; }
    public int TotalPhases { get; set; }
    public int CompletedMilestones { get; set; }
    public int TotalMilestones { get; set; }
    public int DaysRemaining { get; set; }
    public bool IsOnSchedule { get; set; }
    public bool IsOnBudget { get; set; }
    public DateTime LastUpdated { get; set; }
}

// Enums
public enum ResourceType
{
    Labor = 0,
    Equipment = 1,
    Material = 2,
    Contractor = 3,
    Service = 4
}

public enum ResourceAllocationStatus
{
    NotAllocated = 0,
    Reserved = 1,
    Allocated = 2,
    InUse = 3,
    Completed = 4,
    Released = 5
}
