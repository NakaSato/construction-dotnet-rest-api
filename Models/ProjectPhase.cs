using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Project phases within a master plan (e.g., Site Preparation, Installation, Testing)
/// </summary>
public class ProjectPhase
{
    [Key]
    public Guid PhaseId { get; set; }
    
    [ForeignKey("MasterPlan")]
    public Guid MasterPlanId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string PhaseName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Order of execution (1, 2, 3, etc.)
    /// </summary>
    public int PhaseOrder { get; set; }
    
    /// <summary>
    /// Planned start date for this phase
    /// </summary>
    public DateTime PlannedStartDate { get; set; }
    
    /// <summary>
    /// Planned end date for this phase
    /// </summary>
    public DateTime PlannedEndDate { get; set; }
    
    /// <summary>
    /// Actual start date (when work began)
    /// </summary>
    public DateTime? ActualStartDate { get; set; }
    
    /// <summary>
    /// Actual end date (when phase was completed)
    /// </summary>
    public DateTime? ActualEndDate { get; set; }
    
    /// <summary>
    /// Planned duration in days
    /// </summary>
    public int PlannedDurationDays { get; set; }
    
    /// <summary>
    /// Budget allocated for this phase
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal EstimatedBudget { get; set; }
    
    /// <summary>
    /// Actual cost spent on this phase
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; } = 0;
    
    /// <summary>
    /// Weight of this phase in overall project completion (0-100)
    /// </summary>
    public decimal WeightPercentage { get; set; }
    
    /// <summary>
    /// Current completion percentage of this phase (0-100)
    /// </summary>
    public decimal CompletionPercentage { get; set; } = 0;
    
    /// <summary>
    /// Phase status
    /// </summary>
    public PhaseStatus Status { get; set; } = PhaseStatus.NotStarted;
    
    /// <summary>
    /// Prerequisites - phases that must be completed before this one starts
    /// </summary>
    [MaxLength(500)]
    public string? Prerequisites { get; set; }
    
    /// <summary>
    /// Risk level for this phase
    /// </summary>
    public RiskLevel RiskLevel { get; set; } = RiskLevel.Low;
    
    /// <summary>
    /// Notes about the phase
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual MasterPlan MasterPlan { get; set; } = null!;
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<PhaseResource> Resources { get; set; } = new List<PhaseResource>();
}

/// <summary>
/// Solar-specific project phases enum
/// </summary>
public enum SolarPhaseType
{
    SiteAssessment = 1,
    PermitApplication = 2,
    MaterialProcurement = 3,
    SitePreparation = 4,
    ElectricalPrep = 5,
    PanelInstallation = 6,
    InverterInstallation = 7,
    Wiring = 8,
    GridConnection = 9,
    Testing = 10,
    Commissioning = 11,
    Cleanup = 12,
    HandoverTraining = 13
}

public enum PhaseStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Delayed = 4,
    Cancelled = 5
}

public enum RiskLevel
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}
