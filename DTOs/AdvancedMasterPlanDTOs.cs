namespace dotnet_rest_api.DTOs;

// Advanced Analytics DTOs

public class CriticalPathAnalysisDto
{
    public int CriticalPathDuration { get; set; }
    public List<CriticalPathTaskDto> CriticalPathTasks { get; set; } = new();
    public List<ResourceBottleneckDto> ResourceBottlenecks { get; set; } = new();
}

public class CriticalPathTaskDto
{
    public string TaskId { get; set; } = string.Empty;
    public string TaskName { get; set; } = string.Empty;
    public int Duration { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Float { get; set; }
    public bool IsCritical { get; set; }
}

public class ResourceBottleneckDto
{
    public string ResourceType { get; set; } = string.Empty;
    public List<string> ConflictingTasks { get; set; } = new();
    public string RecommendedAction { get; set; } = string.Empty;
}

public class EarnedValueAnalysisDto
{
    public decimal BudgetAtCompletion { get; set; }
    public decimal ActualCostWorkPerformed { get; set; }
    public decimal BudgetedCostWorkPerformed { get; set; }
    public decimal BudgetedCostWorkScheduled { get; set; }
    public decimal CostVariance { get; set; }
    public decimal ScheduleVariance { get; set; }
    public decimal CostPerformanceIndex { get; set; }
    public decimal SchedulePerformanceIndex { get; set; }
    public decimal EstimateAtCompletion { get; set; }
    public decimal EstimateToComplete { get; set; }
    public decimal VarianceAtCompletion { get; set; }
    public decimal ToCompletePerformanceIndex { get; set; }
    public EarnedValueAnalysisResultDto Analysis { get; set; } = new();
}

public class EarnedValueAnalysisResultDto
{
    public string Status { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
}

public class ResourceUtilizationReportDto
{
    public AnalysisPeriodDto AnalysisPeriod { get; set; } = new();
    public List<ResourceUtilizationDto> ResourceUtilization { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class AnalysisPeriodDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class ResourceUtilizationDto
{
    public string ResourceType { get; set; } = string.Empty;
    public int TotalCapacity { get; set; }
    public int AllocatedHours { get; set; }
    public decimal UtilizationRate { get; set; }
    public decimal PeakUtilization { get; set; }
    public List<ResourceConflictDto> Conflicts { get; set; } = new();
}

public class ResourceConflictDto
{
    public DateTime Date { get; set; }
    public int Overallocation { get; set; }
    public List<string> ConflictingTasks { get; set; } = new();
}

// Task Dependency DTOs

public class TaskDependencyDto
{
    public string DependencyId { get; set; } = string.Empty;
    public string PredecessorTaskId { get; set; } = string.Empty;
    public string SuccessorTaskId { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public int LagTime { get; set; }
    public string DependencyDescription { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class CreateTaskDependencyRequest
{
    public string PredecessorTaskId { get; set; } = string.Empty;
    public string SuccessorTaskId { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public int LagTime { get; set; }
    public string DependencyDescription { get; set; } = string.Empty;
}

// Constraint Validation DTOs

public class ConstraintValidationResultDto
{
    public bool IsValid { get; set; }
    public List<ValidationResultDto> ValidationResults { get; set; } = new();
    public int CriticalIssues { get; set; }
    public int Warnings { get; set; }
}

public class ValidationResultDto
{
    public string ConstraintType { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> AffectedTasks { get; set; } = new();
    public string Resolution { get; set; } = string.Empty;
}

// Workflow Automation DTOs

public class WorkflowExecutionResultDto
{
    public string WorkflowExecutionId { get; set; } = string.Empty;
    public List<TriggeredActionDto> TriggeredActions { get; set; } = new();
}

public class TriggeredActionDto
{
    public string ActionType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public DateTime? ScheduledFor { get; set; }
    public string Details { get; set; } = string.Empty;
}

public class TriggerWorkflowRequest
{
    public string TriggerType { get; set; } = string.Empty;
    public Dictionary<string, object> TriggerData { get; set; } = new();
    public List<string> AutomationRules { get; set; } = new();
}

// Executive Dashboard DTOs

public class ExecutiveDashboardDto
{
    public ProjectOverviewDto ProjectOverview { get; set; } = new();
    public KeyMetricsDto KeyMetrics { get; set; } = new();
    public MilestoneStatusDto MilestoneStatus { get; set; } = new();
    public List<CriticalIssueDto> CriticalIssues { get; set; } = new();
    public List<NextKeyMilestoneDto> NextKeyMilestones { get; set; } = new();
}

public class ProjectOverviewDto
{
    public string ProjectName { get; set; } = string.Empty;
    public string CurrentPhase { get; set; } = string.Empty;
    public decimal OverallProgress { get; set; }
    public decimal BudgetUtilization { get; set; }
    public decimal SchedulePerformance { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

public class KeyMetricsDto
{
    public decimal CostPerformanceIndex { get; set; }
    public decimal SchedulePerformanceIndex { get; set; }
    public decimal QualityIndex { get; set; }
    public decimal SafetyIndex { get; set; }
}

public class MilestoneStatusDto
{
    public int Completed { get; set; }
    public int InProgress { get; set; }
    public int Upcoming { get; set; }
    public int Overdue { get; set; }
}

public class CriticalIssueDto
{
    public string IssueType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Mitigation { get; set; } = string.Empty;
}

public class NextKeyMilestoneDto
{
    public string MilestoneName { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
}

// Export DTOs

public class ProjectExportDto
{
    public string ExportId { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public List<string> Sections { get; set; } = new();
}

// Stakeholder Communication DTOs

public class StakeholderCommunicationDto
{
    public string CommunicationId { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public int RecipientCount { get; set; }
    public string DeliveryMethod { get; set; } = string.Empty;
    public DateTime ScheduledDelivery { get; set; }
    public CommunicationContentDto Content { get; set; } = new();
}

public class CommunicationContentDto
{
    public string ExecutiveSummary { get; set; } = string.Empty;
    public List<string> KeyHighlights { get; set; } = new();
    public List<AttachmentDto> Attachments { get; set; } = new();
}

public class AttachmentDto
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class CreateStakeholderCommunicationRequest
{
    public string CommunicationType { get; set; } = string.Empty;
    public string StakeholderGroup { get; set; } = string.Empty;
    public string CustomMessage { get; set; } = string.Empty;
    public bool IncludeFinancialData { get; set; }
    public bool IncludeRiskAnalysis { get; set; }
    public List<string> Attachments { get; set; } = new();
}

// Gantt Chart DTOs

public class GanttChartDataDto
{
    public List<GanttTaskDto> Tasks { get; set; } = new();
    public List<GanttLinkDto> Links { get; set; } = new();
}

public class GanttTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Progress { get; set; }
    public List<string> Dependencies { get; set; } = new();
    public GanttBaselineDto? Baseline { get; set; }
}

public class GanttBaselineDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GanttLinkDto
{
    public string Id { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

// Weekly View DTOs

public class WeeklyViewDto
{
    public string PlanId { get; set; } = string.Empty;
    public DateTime ViewStartDate { get; set; }
    public DateTime ViewEndDate { get; set; }
    public List<WeeklyDataDto> Weeks { get; set; } = new();
}

public class WeeklyDataDto
{
    public int WeekNumber { get; set; }
    public string WeekLabel { get; set; } = string.Empty;
    public MasterPlanWeeklySummaryDto Summary { get; set; } = new();
    public List<WeeklyTaskDto> Tasks { get; set; } = new();
}

public class MasterPlanWeeklySummaryDto
{
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int NotStartedTasks { get; set; }
    public int PlannedHours { get; set; }
    public int ActualHours { get; set; }
    public decimal Efficiency { get; set; }
}

public class WeeklyTaskDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public WeeklyTaskProgressDto Progress { get; set; } = new();
}

public class WeeklyTaskProgressDto
{
    public decimal PercentageComplete { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
