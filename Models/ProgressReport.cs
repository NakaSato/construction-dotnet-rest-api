using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Overall progress tracking for the entire project based on master plan
/// </summary>
public class ProgressReport
{
    [Key]
    public Guid ProgressReportId { get; set; }
    
    [ForeignKey("MasterPlan")]
    public Guid MasterPlanId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Date of this progress report
    /// </summary>
    public DateTime ReportDate { get; set; }
    
    /// <summary>
    /// Overall project completion percentage (0-100)
    /// Calculated from phase completion and milestone achievement
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal OverallCompletionPercentage { get; set; }
    
    /// <summary>
    /// Schedule performance index (Planned vs Actual progress)
    /// SPI = 1.0 means on schedule, <1.0 behind, >1.0 ahead
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal SchedulePerformanceIndex { get; set; }
    
    /// <summary>
    /// Cost performance index (Planned vs Actual cost)
    /// CPI = 1.0 means on budget, <1.0 over budget, >1.0 under budget
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal CostPerformanceIndex { get; set; }
    
    /// <summary>
    /// Total actual cost spent to date
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCostToDate { get; set; }
    
    /// <summary>
    /// Estimated cost at completion based on current performance
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedCostAtCompletion { get; set; }
    
    /// <summary>
    /// Variance from original budget
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal BudgetVariance { get; set; }
    
    /// <summary>
    /// Number of days ahead or behind schedule (negative = behind)
    /// </summary>
    public int ScheduleVarianceDays { get; set; }
    
    /// <summary>
    /// Projected completion date based on current progress rate
    /// </summary>
    public DateTime ProjectedCompletionDate { get; set; }
    
    /// <summary>
    /// Number of active issues/risks
    /// </summary>
    public int ActiveIssuesCount { get; set; }
    
    /// <summary>
    /// Number of completed milestones
    /// </summary>
    public int CompletedMilestonesCount { get; set; }
    
    /// <summary>
    /// Total number of milestones
    /// </summary>
    public int TotalMilestonesCount { get; set; }
    
    /// <summary>
    /// Overall project health status
    /// </summary>
    public ProjectHealthStatus HealthStatus { get; set; }
    
    /// <summary>
    /// Key accomplishments since last report
    /// </summary>
    [MaxLength(4000)]
    public string? KeyAccomplishments { get; set; }
    
    /// <summary>
    /// Current challenges and issues
    /// </summary>
    [MaxLength(4000)]
    public string? CurrentChallenges { get; set; }
    
    /// <summary>
    /// Planned activities for next period
    /// </summary>
    [MaxLength(4000)]
    public string? UpcomingActivities { get; set; }
    
    /// <summary>
    /// Risk assessment summary
    /// </summary>
    [MaxLength(2000)]
    public string? RiskSummary { get; set; }
    
    /// <summary>
    /// Quality metrics and observations
    /// </summary>
    [MaxLength(2000)]
    public string? QualityNotes { get; set; }
    
    /// <summary>
    /// Weather impact on progress
    /// </summary>
    [MaxLength(1000)]
    public string? WeatherImpact { get; set; }
    
    /// <summary>
    /// Resource utilization notes
    /// </summary>
    [MaxLength(2000)]
    public string? ResourceNotes { get; set; }
    
    /// <summary>
    /// Overall executive summary
    /// </summary>
    [MaxLength(3000)]
    public string? ExecutiveSummary { get; set; }
    
    /// <summary>
    /// Title of the progress report
    /// </summary>
    [MaxLength(255)]
    public string? ReportTitle { get; set; }
    
    /// <summary>
    /// Main content/body of the report
    /// </summary>
    [MaxLength(8000)]
    public string? ReportContent { get; set; }
    
    /// <summary>
    /// Overall completion percentage (alternative name for OverallCompletionPercentage)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal CompletionPercentage { get; set; }
    
    /// <summary>
    /// Challenges faced (alternative name for CurrentChallenges)
    /// </summary>
    [MaxLength(4000)]
    public string? ChallengesFaced { get; set; }
    
    /// <summary>
    /// Next steps and actions to be taken (maps to UpcomingActivities)
    /// </summary>
    [MaxLength(4000)]
    public string? NextSteps { get; set; }
    
    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }
    
    // Navigation Properties
    public virtual MasterPlan MasterPlan { get; set; } = null!;
    public virtual Project Project { get; set; } = null!;
    public virtual User CreatedBy { get; set; } = null!;
    public virtual ICollection<PhaseProgress> PhaseProgressDetails { get; set; } = new List<PhaseProgress>();
}

/// <summary>
/// Detailed progress for individual phases
/// </summary>
public class PhaseProgress
{
    [Key]
    public Guid PhaseProgressId { get; set; }
    
    [ForeignKey("ProgressReport")]
    public Guid ProgressReportId { get; set; }
    
    [ForeignKey("Phase")]
    public Guid PhaseId { get; set; }
    
    /// <summary>
    /// Completion percentage for this phase (0-100)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal CompletionPercentage { get; set; }
    
    /// <summary>
    /// Planned completion percentage at this date
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal PlannedCompletionPercentage { get; set; }
    
    /// <summary>
    /// Variance from planned progress
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal ProgressVariance { get; set; }
    
    /// <summary>
    /// Status of this phase
    /// </summary>
    public PhaseStatus Status { get; set; }
    
    /// <summary>
    /// Notes specific to this phase progress
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    /// <summary>
    /// Key activities completed in this phase
    /// </summary>
    [MaxLength(2000)]
    public string? ActivitiesCompleted { get; set; }
    
    /// <summary>
    /// Issues encountered in this phase
    /// </summary>
    [MaxLength(2000)]
    public string? Issues { get; set; }
    
    // Navigation Properties
    public virtual ProgressReport ProgressReport { get; set; } = null!;
    public virtual ProjectPhase Phase { get; set; } = null!;
}

public enum ProjectHealthStatus
{
    Excellent = 0,    // Green - ahead of schedule and under budget
    Good = 1,         // Green - on track
    Caution = 2,      // Yellow - minor issues but manageable
    AtRisk = 3,       // Orange - significant issues requiring attention
    Critical = 4      // Red - major issues threatening project success
}
