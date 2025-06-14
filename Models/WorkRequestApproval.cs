using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum ApprovalAction
{
    Submitted,
    ManagerReview,
    ManagerApproved,
    ManagerRejected,
    AdminReview,
    AdminApproved,
    AdminRejected,
    AutoApproved,
    Escalated,
    InProgress,
    Completed,
    Cancelled,
    Resubmitted
}

public enum ApprovalLevel
{
    None = 0,
    Manager = 1,
    Admin = 2
}

public class WorkRequestApproval
{
    [Key]
    public Guid ApprovalId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid WorkRequestId { get; set; }
    
    [ForeignKey("Approver")]
    public Guid ApproverId { get; set; }
    
    public ApprovalAction Action { get; set; }
    
    public ApprovalLevel Level { get; set; }
    
    public WorkRequestStatus PreviousStatus { get; set; }
    
    public WorkRequestStatus NewStatus { get; set; }
    
    [MaxLength(1000)]
    public string? Comments { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? RejectionReason { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? ProcessedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    // Escalation tracking
    public Guid? EscalatedFromId { get; set; }
    
    public DateTime? EscalationDate { get; set; }
    
    [MaxLength(500)]
    public string? EscalationReason { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    public virtual User Approver { get; set; } = null!;
    public virtual WorkRequestApproval? EscalatedFrom { get; set; }
    public virtual ICollection<WorkRequestApproval> Escalations { get; set; } = new List<WorkRequestApproval>();
}
