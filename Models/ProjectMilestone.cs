using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Key milestones within the master plan for tracking major accomplishments
/// </summary>
public class ProjectMilestone
{
    [Key]
    public Guid MilestoneId { get; set; }
    
    [ForeignKey("MasterPlan")]
    public Guid MasterPlanId { get; set; }
    
    [ForeignKey("Phase")]
    public Guid? PhaseId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string MilestoneName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Planned date for milestone completion
    /// </summary>
    public DateTime PlannedDate { get; set; }
    
    /// <summary>
    /// Actual date milestone was achieved
    /// </summary>
    public DateTime? ActualDate { get; set; }
    
    /// <summary>
    /// Milestone type (e.g., phase completion, permit approval, etc.)
    /// </summary>
    public MilestoneType Type { get; set; }
    
    /// <summary>
    /// Importance level of this milestone
    /// </summary>
    public MilestoneImportance Importance { get; set; } = MilestoneImportance.Medium;
    
    /// <summary>
    /// Current status of the milestone
    /// </summary>
    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;
    
    /// <summary>
    /// Weight of this milestone in overall project progress (0-100)
    /// </summary>
    public decimal WeightPercentage { get; set; }
    
    /// <summary>
    /// Criteria that must be met for milestone completion
    /// </summary>
    [MaxLength(2000)]
    public string? CompletionCriteria { get; set; }
    
    /// <summary>
    /// Evidence or documentation of milestone completion
    /// </summary>
    [MaxLength(1000)]
    public string? CompletionEvidence { get; set; }
    
    /// <summary>
    /// User who verified/approved milestone completion
    /// </summary>
    [ForeignKey("VerifiedBy")]
    public Guid? VerifiedById { get; set; }
    
    /// <summary>
    /// Date milestone was verified
    /// </summary>
    public DateTime? VerifiedAt { get; set; }
    
    /// <summary>
    /// Notes about milestone completion or delays
    /// </summary>
    [MaxLength(2000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual MasterPlan MasterPlan { get; set; } = null!;
    public virtual ProjectPhase? Phase { get; set; }
    public virtual User? VerifiedBy { get; set; }
}

public enum MilestoneType
{
    PhaseCompletion = 1,
    PermitApproval = 2,
    MaterialDelivery = 3,
    InspectionPassed = 4,
    ClientApproval = 5,
    PaymentReceived = 6,
    TestingCompleted = 7,
    GridConnection = 8,
    CommissioningComplete = 9,
    ProjectHandover = 10,
    Custom = 99
}

public enum MilestoneImportance
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public enum MilestoneStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Delayed = 3,
    AtRisk = 4,
    Cancelled = 5
}
