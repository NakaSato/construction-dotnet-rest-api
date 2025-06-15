using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Master plan created by Project Manager containing all project phases and milestones
/// </summary>
public class MasterPlan
{
    [Key]
    public Guid MasterPlanId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string PlanName { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Description { get; set; }
    
    [Required]
    public DateTime PlannedStartDate { get; set; }
    
    [Required]
    public DateTime PlannedEndDate { get; set; }
    
    /// <summary>
    /// Total planned duration in days
    /// </summary>
    public int TotalPlannedDays { get; set; }
    
    /// <summary>
    /// Current version of the master plan (for tracking changes)
    /// </summary>
    public int Version { get; set; } = 1;
    
    /// <summary>
    /// Plan status
    /// </summary>
    public MasterPlanStatus Status { get; set; } = MasterPlanStatus.Draft;
    
    /// <summary>
    /// Date when plan was approved
    /// </summary>
    public DateTime? ApprovedAt { get; set; }
    
    /// <summary>
    /// User who approved the plan
    /// </summary>
    [ForeignKey("ApprovedBy")]
    public Guid? ApprovedById { get; set; }
    
    /// <summary>
    /// Notes from approval process
    /// </summary>
    [MaxLength(2000)]
    public string? ApprovalNotes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    [ForeignKey("CreatedBy")]
    public Guid CreatedById { get; set; }
    
    // Navigation Properties
    public virtual Project Project { get; set; } = null!;
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? ApprovedBy { get; set; }
    public virtual ICollection<ProjectPhase> Phases { get; set; } = new List<ProjectPhase>();
    public virtual ICollection<ProjectMilestone> Milestones { get; set; } = new List<ProjectMilestone>();
    public virtual ICollection<ProgressReport> ProgressReports { get; set; } = new List<ProgressReport>();
}

public enum MasterPlanStatus
{
    Draft = 0,
    UnderReview = 1,
    Approved = 2,
    Active = 3,
    Completed = 4,
    Cancelled = 5
}
