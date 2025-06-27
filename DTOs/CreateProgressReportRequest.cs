using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Request DTO for creating a progress report
/// </summary>
public class CreateProgressReportRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000, MinimumLength = 10)]
    public string Summary { get; set; } = string.Empty;

    [StringLength(5000)]
    public string? DetailedReport { get; set; }

    [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100")]
    public decimal CompletionPercentage { get; set; }

    public List<string>? Accomplishments { get; set; }

    /// <summary>
    /// Key accomplishments for the report
    /// </summary>
    public string? KeyAccomplishments 
    { 
        get => Accomplishments?.Count > 0 ? string.Join("; ", Accomplishments) : null;
        set => Accomplishments = !string.IsNullOrEmpty(value) ? value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : null;
    }

    public List<string>? Issues { get; set; }

    /// <summary>
    /// Current challenges for the report
    /// </summary>
    public string? CurrentChallenges 
    { 
        get => Issues?.Count > 0 ? string.Join("; ", Issues) : null;
        set => Issues = !string.IsNullOrEmpty(value) ? value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : null;
    }

    public List<string>? NextSteps { get; set; }

    /// <summary>
    /// Upcoming activities for the report
    /// </summary>
    public string? UpcomingActivities 
    { 
        get => NextSteps?.Count > 0 ? string.Join("; ", NextSteps) : null;
        set => NextSteps = !string.IsNullOrEmpty(value) ? value.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList() : null;
    }

    public DateTime? ReportDate { get; set; }

    public string? WeatherConditions { get; set; }

    /// <summary>
    /// Weather impact notes
    /// </summary>
    public string? WeatherImpact { get; set; }

    public int? WorkersOnSite { get; set; }

    public string? SafetyNotes { get; set; }

    /// <summary>
    /// Quality notes for the report
    /// </summary>
    public string? QualityNotes { get; set; }

    /// <summary>
    /// Risk summary for the report
    /// </summary>
    public string? RiskSummary { get; set; }

    /// <summary>
    /// Resource notes for the report
    /// </summary>
    public string? ResourceNotes { get; set; }

    /// <summary>
    /// Executive summary for the report
    /// </summary>
    public string? ExecutiveSummary { get; set; }

    /// <summary>
    /// Phase updates for the report
    /// </summary>
    public List<UpdatePhaseProgressRequest>? PhaseUpdates { get; set; }
    
    // Additional properties for compatibility with MasterPlanReportingService
    public string ReportTitle => Title;
    public string ReportContent => Summary;
    public string? ChallengesFaced => CurrentChallenges;
}
