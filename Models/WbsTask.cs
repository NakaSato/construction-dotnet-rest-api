using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Represents a Work Breakdown Structure (WBS) task for solar PV installation projects
/// </summary>
public class WbsTask
{
    /// <summary>
    /// The unique identifier for the task in the hierarchical structure (e.g., '4.3.2.3')
    /// </summary>
    [Key]
    [Required]
    [StringLength(50)]
    public string WbsId { get; set; } = string.Empty;

    /// <summary>
    /// The ID of the parent task in the hierarchy
    /// </summary>
    [StringLength(50)]
    public string? ParentWbsId { get; set; }

    /// <summary>
    /// The name of the task in English
    /// </summary>
    [Required]
    [StringLength(200)]
    public string TaskNameEN { get; set; } = string.Empty;

    /// <summary>
    /// The name of the task in Thai
    /// </summary>
    [Required]
    [StringLength(200)]
    public string TaskNameTH { get; set; } = string.Empty;

    /// <summary>
    /// A detailed description of the task's scope
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// The current status of the task
    /// </summary>
    [Required]
    public WbsTaskStatus Status { get; set; } = WbsTaskStatus.NotStarted;

    /// <summary>
    /// The weight of the task relative to its parent or the total project (percentage)
    /// </summary>
    [Range(0, 100)]
    public double WeightPercent { get; set; }

    /// <summary>
    /// The installation area related to this task
    /// </summary>
    [StringLength(100)]
    public string? InstallationArea { get; set; }

    /// <summary>
    /// The criteria used to determine if the task is complete
    /// </summary>
    [StringLength(500)]
    public string? AcceptanceCriteria { get; set; }

    /// <summary>
    /// The planned start date for the task
    /// </summary>
    public DateTime? PlannedStartDate { get; set; }

    /// <summary>
    /// The actual start date of the task
    /// </summary>
    public DateTime? ActualStartDate { get; set; }

    /// <summary>
    /// The planned end date for the task
    /// </summary>
    public DateTime? PlannedEndDate { get; set; }

    /// <summary>
    /// The actual end date of the task
    /// </summary>
    public DateTime? ActualEndDate { get; set; }

    /// <summary>
    /// Project ID this WBS task belongs to
    /// </summary>
    [Required]
    public Guid ProjectId { get; set; }

    /// <summary>
    /// User ID of the person assigned to this task
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// Navigation property to the project
    /// </summary>
    [ForeignKey(nameof(ProjectId))]
    public virtual Project? Project { get; set; }

    /// <summary>
    /// Navigation property to the assigned user
    /// </summary>
    [ForeignKey(nameof(AssignedUserId))]
    public virtual User? AssignedUser { get; set; }

    /// <summary>
    /// Navigation property to parent task
    /// </summary>
    [ForeignKey(nameof(ParentWbsId))]
    public virtual WbsTask? ParentTask { get; set; }

    /// <summary>
    /// Navigation property to child tasks
    /// </summary>
    public virtual ICollection<WbsTask> ChildTasks { get; set; } = new List<WbsTask>();

    /// <summary>
    /// Navigation property to task dependencies
    /// </summary>
    public virtual ICollection<WbsTaskDependency> Dependencies { get; set; } = new List<WbsTaskDependency>();

    /// <summary>
    /// Navigation property to tasks that depend on this task
    /// </summary>
    public virtual ICollection<WbsTaskDependency> DependentTasks { get; set; } = new List<WbsTaskDependency>();

    /// <summary>
    /// Navigation property to task evidence
    /// </summary>
    public virtual ICollection<WbsTaskEvidence> Evidence { get; set; } = new List<WbsTaskEvidence>();

    /// <summary>
    /// Creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// User who created this task
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this task
    /// </summary>
    public Guid? UpdatedBy { get; set; }
}

/// <summary>
/// Enumeration of WBS task statuses
/// </summary>
public enum WbsTaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4,
    UnderReview = 5,
    Approved = 6
}

/// <summary>
/// Represents a dependency relationship between WBS tasks
/// </summary>
public class WbsTaskDependency
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The task that depends on another task
    /// </summary>
    [Required]
    [StringLength(50)]
    public string DependentTaskId { get; set; } = string.Empty;

    /// <summary>
    /// The task that must be completed first
    /// </summary>
    [Required]
    [StringLength(50)]
    public string PrerequisiteTaskId { get; set; } = string.Empty;

    /// <summary>
    /// Type of dependency relationship
    /// </summary>
    public DependencyType DependencyType { get; set; } = DependencyType.FinishToStart;

    /// <summary>
    /// Navigation property to the dependent task
    /// </summary>
    [ForeignKey(nameof(DependentTaskId))]
    public virtual WbsTask? DependentTask { get; set; }

    /// <summary>
    /// Navigation property to the prerequisite task
    /// </summary>
    [ForeignKey(nameof(PrerequisiteTaskId))]
    public virtual WbsTask? PrerequisiteTask { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of task dependencies
/// </summary>
public enum DependencyType
{
    FinishToStart = 0,
    StartToStart = 1,
    FinishToFinish = 2,
    StartToFinish = 3
}

/// <summary>
/// Represents evidence (photos, documents) for a WBS task
/// </summary>
public class WbsTaskEvidence
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// The WBS task this evidence belongs to
    /// </summary>
    [Required]
    [StringLength(50)]
    public string WbsTaskId { get; set; } = string.Empty;

    /// <summary>
    /// Type of evidence (photo, document, video, etc.)
    /// </summary>
    [Required]
    [StringLength(50)]
    public string EvidenceType { get; set; } = string.Empty;

    /// <summary>
    /// URL or path to the evidence file
    /// </summary>
    [Required]
    [StringLength(500)]
    public string FileUrl { get; set; } = string.Empty;

    /// <summary>
    /// Original filename
    /// </summary>
    [StringLength(200)]
    public string? FileName { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// MIME type of the file
    /// </summary>
    [StringLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// Description or notes about this evidence
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Navigation property to the WBS task
    /// </summary>
    [ForeignKey(nameof(WbsTaskId))]
    public virtual WbsTask? WbsTask { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
}
