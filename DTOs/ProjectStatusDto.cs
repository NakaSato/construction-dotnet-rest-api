namespace dotnet_rest_api.DTOs;

/// <summary>
/// DTO for project real-time status information
/// </summary>
public class ProjectStatusDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? PlannedStartDate { get; set; }
    public DateTime? PlannedEndDate { get; set; }
    public DateTime? ActualStartDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public decimal OverallCompletionPercentage { get; set; }
    public bool IsOnSchedule { get; set; }
    public bool IsOnBudget { get; set; }
    public int ActiveTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int TotalTasks { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<LinkDto>? Links { get; set; }
}
