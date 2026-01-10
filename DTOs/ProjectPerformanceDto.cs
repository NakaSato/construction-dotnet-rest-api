namespace dotnet_rest_api.DTOs;

/// <summary>
/// Project performance tracking response DTO
/// </summary>
public class ProjectPerformanceDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int PerformanceScore { get; set; }
    public ProjectKPIs KPIs { get; set; } = new();
    public List<PerformanceMilestoneDto> Milestones { get; set; } = new();
    public ResourceUtilization ResourceUtilization { get; set; } = new();
    public RiskAssessment RiskAssessment { get; set; } = new();
    public List<ProgressHistoryEntry> ProgressHistory { get; set; } = new();
}

/// <summary>
/// Key Performance Indicators for a project
/// </summary>
public class ProjectKPIs
{
    public decimal TimelineAdherence { get; set; } = 85m;
    public decimal BudgetAdherence { get; set; } = 90m;
    public decimal QualityScore { get; set; } = 88m;
    public decimal SafetyScore { get; set; } = 95m;
    public decimal ClientSatisfaction { get; set; } = 85m;
}

/// <summary>
/// Project milestone information
/// </summary>
public class PerformanceMilestoneDto
{
    public Guid MilestoneId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime TargetDate { get; set; }
    public DateTime? ActualDate { get; set; }
    public string Status { get; set; } = "Pending";
    public int VarianceDays { get; set; }
}

/// <summary>
/// Resource utilization metrics
/// </summary>
public class ResourceUtilization
{
    public decimal TeamUtilization { get; set; } = 75m;
    public decimal EquipmentUtilization { get; set; } = 80m;
    public decimal MaterialEfficiency { get; set; } = 85m;
}

/// <summary>
/// Risk assessment information
/// </summary>
public class RiskAssessment
{
    public string OverallRiskLevel { get; set; } = "Low";
    public int ActiveRisks { get; set; }
    public int MitigatedRisks { get; set; }
    public string RiskTrend { get; set; } = "Stable";
}

/// <summary>
/// Progress history entry for tracking project progress over time
/// </summary>
public class ProgressHistoryEntry
{
    public DateTime Date { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int TasksCompleted { get; set; }
    public decimal HoursWorked { get; set; }
    public int Issues { get; set; }
}
