using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum WorkRequestStatus
{
    Open,
    PendingManagerApproval,
    PendingAdminApproval,
    Approved,
    Rejected,
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
    public Guid WorkRequestId { get; set; }
    
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
    
    public WorkRequestStatus Status { get; set; } = WorkRequestStatus.Open;
    
    [ForeignKey("RequestedBy")]
    public Guid RequestedById { get; set; }
    
    [ForeignKey("AssignedTo")]
    public Guid? AssignedToId { get; set; }
    
    public DateTime? RequestedDate { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    public DateTime? StartedAt { get; set; }
    
    public DateTime? CompletedDate { get; set; }
    
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
    
    // Approval workflow fields
    [ForeignKey("ManagerApprover")]
    public Guid? ManagerApproverId { get; set; }
    
    [ForeignKey("AdminApprover")]
    public Guid? AdminApproverId { get; set; }
    
    public DateTime? ManagerApprovalDate { get; set; }
    
    public DateTime? AdminApprovalDate { get; set; }
    
    public DateTime? SubmittedForApprovalDate { get; set; }
    
    [MaxLength(1000)]
    public string? ManagerComments { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? AdminComments { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? RejectionReason { get; set; } = string.Empty;
    
    public bool RequiresManagerApproval { get; set; } = true;
    
    public bool RequiresAdminApproval { get; set; } = false;
    
    // Auto-approval settings
    public decimal? AutoApprovalThreshold { get; set; }
    
    public bool IsAutoApproved { get; set; } = false;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User RequestedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }
    public virtual User? ManagerApprover { get; set; }
    public virtual User? AdminApprover { get; set; }
    public virtual ICollection<WorkRequestTask> Tasks { get; set; } = new List<WorkRequestTask>();
    public virtual ICollection<WorkRequestComment> Comments { get; set; } = new List<WorkRequestComment>();
    public virtual ICollection<WorkRequestApproval> ApprovalHistory { get; set; } = new List<WorkRequestApproval>();
    public virtual ICollection<WorkRequestNotification> Notifications { get; set; } = new List<WorkRequestNotification>();
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}

public class WorkRequestTask
{
    [Key]
    public Guid WorkRequestTaskId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid WorkRequestId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public WorkRequestStatus Status { get; set; } = WorkRequestStatus.Open;
    
    [ForeignKey("AssignedToUser")]
    public Guid? AssignedToId { get; set; }
    
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
    public Guid WorkRequestCommentId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid WorkRequestId { get; set; }
    
    [ForeignKey("Author")]
    public Guid AuthorId { get; set; }
    
    [Required]
    [MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    public virtual User Author { get; set; } = null!;
}
