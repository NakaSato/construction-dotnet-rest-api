using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum WorkRequestStatus
{
    Pending,
    InProgress,
    Completed,
    Cancelled,
    OnHold
}

public enum WorkRequestPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum WorkRequestType
{
    Maintenance,
    Repair,
    Installation,
    Inspection,
    Documentation,
    Other
}

public class WorkRequest
{
    [Key]
    public Guid RequestId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public WorkRequestType Type { get; set; } = WorkRequestType.Other;
    
    public WorkRequestPriority Priority { get; set; } = WorkRequestPriority.Medium;
    
    public WorkRequestStatus Status { get; set; } = WorkRequestStatus.Pending;
    
    [ForeignKey("RequestedByUser")]
    public Guid RequestedByUserId { get; set; }
    
    [ForeignKey("AssignedToUser")]
    public Guid? AssignedToUserId { get; set; }
    
    public DateTime? RequestedDate { get; set; }
    
    public DateTime? RequiredByDate { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    [MaxLength(1000)]
    public string? Resolution { get; set; } = string.Empty;
    
    [Range(0, int.MaxValue)]
    public decimal? EstimatedCost { get; set; }
    
    [Range(0, int.MaxValue)]
    public decimal? ActualCost { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? EstimatedHours { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? ActualHours { get; set; }
    
    [MaxLength(500)]
    public string? Location { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User RequestedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual ICollection<WorkRequestTask> Tasks { get; set; } = new List<WorkRequestTask>();
    public virtual ICollection<WorkRequestComment> Comments { get; set; } = new List<WorkRequestComment>();
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}

public class WorkRequestTask
{
    [Key]
    public Guid TaskId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid RequestId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public WorkRequestStatus Status { get; set; } = WorkRequestStatus.Pending;
    
    [ForeignKey("AssignedToUser")]
    public Guid? AssignedToUserId { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? CompletedAt { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? EstimatedHours { get; set; }
    
    [Range(0, double.MaxValue)]
    public double? ActualHours { get; set; }
    
    [MaxLength(500)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
}

public class WorkRequestComment
{
    [Key]
    public Guid CommentId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid RequestId { get; set; }
    
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
