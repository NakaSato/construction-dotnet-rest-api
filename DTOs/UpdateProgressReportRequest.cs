using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Request for updating an existing progress report
/// </summary>
public class UpdateProgressReportRequest
{
    /// <summary>
    /// Updated report title
    /// </summary>
    [StringLength(255)]
    public string? Title { get; set; }

    /// <summary>
    /// Updated executive summary of the report
    /// </summary>
    [StringLength(2000)]
    public string? ExecutiveSummary { get; set; }

    /// <summary>
    /// Updated key achievements
    /// </summary>
    [StringLength(2000)]
    public string? KeyAchievements { get; set; }

    /// <summary>
    /// Updated challenges and issues
    /// </summary>
    [StringLength(2000)]
    public string? ChallengesAndIssues { get; set; }

    /// <summary>
    /// Updated next steps
    /// </summary>
    [StringLength(2000)]
    public string? NextSteps { get; set; }

    /// <summary>
    /// Updated overall status notes
    /// </summary>
    [StringLength(2000)]
    public string? StatusNotes { get; set; }

    /// <summary>
    /// Updated recommendations
    /// </summary>
    [StringLength(2000)]
    public string? Recommendations { get; set; }

    /// <summary>
    /// Updated progress photos or documents URLs
    /// </summary>
    public List<string>? ProgressPhotos { get; set; }

    /// <summary>
    /// Updated budget information
    /// </summary>
    public decimal? BudgetSpent { get; set; }

    /// <summary>
    /// Updated budget remaining
    /// </summary>
    public decimal? BudgetRemaining { get; set; }

    /// <summary>
    /// Updated weather conditions
    /// </summary>
    [StringLength(500)]
    public string? WeatherConditions { get; set; }

    /// <summary>
    /// Updated safety incidents
    /// </summary>
    [StringLength(1000)]
    public string? SafetyIncidents { get; set; }

    /// <summary>
    /// Updated quality issues
    /// </summary>
    [StringLength(1000)]
    public string? QualityIssues { get; set; }
    
    // Additional properties for compatibility with MasterPlanReportingService
    public string? ReportTitle => Title;
    public string? ReportContent => ExecutiveSummary;
    public DateTime? ReportDate { get; set; }
    public decimal? CompletionPercentage { get; set; }
    public string? KeyAccomplishments => KeyAchievements;
    public string? ChallengesFaced => ChallengesAndIssues;
}
