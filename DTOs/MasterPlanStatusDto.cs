namespace dotnet_rest_api.DTOs;

/// <summary>
/// DTO for master plan real-time status
/// </summary>
public class MasterPlanStatusDto
{
    public Guid MasterPlanId { get; set; }
    public decimal OverallCompletionPercentage { get; set; }
    public string HealthStatus { get; set; } = string.Empty;
    public bool IsOnSchedule { get; set; }
    public bool IsOnBudget { get; set; }
    public int CompletedPhases { get; set; }
    public int TotalPhases { get; set; }
    public int CompletedMilestones { get; set; }
    public int TotalMilestones { get; set; }
    public int? DaysRemaining { get; set; }
    public DateTime LastUpdated { get; set; }
    public List<LinkDto> Links { get; set; } = new();
}
