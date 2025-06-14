using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum NotificationType
{
    WorkRequestSubmitted,
    WorkRequestApproved,
    WorkRequestRejected,
    WorkRequestAssigned,
    WorkRequestCompleted,
    WorkRequestEscalated,
    WorkRequestDue,
    WorkRequestOverdue,
    ApprovalRequired,
    ApprovalReminder
}

public enum NotificationStatus
{
    Pending,
    Sent,
    Failed,
    Delivered,
    Read
}

public class WorkRequestNotification
{
    [Key]
    public Guid NotificationId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid WorkRequestId { get; set; }
    
    [ForeignKey("Recipient")]
    public Guid RecipientId { get; set; }
    
    [ForeignKey("Sender")]
    public Guid? SenderId { get; set; }
    
    public NotificationType Type { get; set; }
    
    public NotificationStatus Status { get; set; } = NotificationStatus.Pending;
    
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? EmailTo { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? EmailCc { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? SentAt { get; set; }
    
    public DateTime? DeliveredAt { get; set; }
    
    public DateTime? ReadAt { get; set; }
    
    public int RetryCount { get; set; } = 0;
    
    public DateTime? NextRetryAt { get; set; }
    
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; } = string.Empty;
    
    // Additional metadata as JSON
    [MaxLength(2000)]
    public string? Metadata { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual WorkRequest WorkRequest { get; set; } = null!;
    public virtual User Recipient { get; set; } = null!;
    public virtual User? Sender { get; set; }
}
