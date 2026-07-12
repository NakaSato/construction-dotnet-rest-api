using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Work Requests DTOs
public class WorkRequestDto
{
    public Guid WorkRequestId { get; set; }
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedById { get; set; }
    public string? RequestedByName { get; set; } = string.Empty;
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; } = string.Empty;
    public DateTime? RequestedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedDate { get; set; }
    public string? Resolution { get; set; } = string.Empty;
    public decimal? EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public string? Location { get; set; } = string.Empty;
    public string? Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Approval workflow fields
    public Guid? ManagerApproverId { get; set; }
    public string? ManagerApproverName { get; set; } = string.Empty;
    public Guid? AdminApproverId { get; set; }
    public string? AdminApproverName { get; set; } = string.Empty;
    public DateTime? ManagerApprovalDate { get; set; }
    public DateTime? AdminApprovalDate { get; set; }
    public DateTime? SubmittedForApprovalDate { get; set; }
    public string? ManagerComments { get; set; } = string.Empty;
    public string? AdminComments { get; set; } = string.Empty;
    public string? RejectionReason { get; set; } = string.Empty;
    public bool RequiresManagerApproval { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool IsAutoApproved { get; set; }
    public string? CurrentApproverName { get; set; } = string.Empty;
    public string? NextApproverName { get; set; } = string.Empty;
    public int DaysPendingApproval { get; set; }
    
    // Related data
    public List<WorkRequestTaskDto> Tasks { get; set; } = new();
    public List<WorkRequestCommentDto> Comments { get; set; } = new();
    public List<WorkRequestApprovalDto> ApprovalHistory { get; set; } = new();
    public List<ImageMetadataDto> Images { get; set; } = new();
    public int ImageCount { get; set; }
    
    // HATEOAS Links
    public List<LinkDto> Links { get; set; } = new();
}

public class CreateWorkRequestRequest
{
    [Required(ErrorMessage = "Project ID is required")]
    public Guid ProjectId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Maintenance|Repair|Installation|Inspection|Documentation|Other)$", 
        ErrorMessage = "Type must be one of: Maintenance, Repair, Installation, Inspection, Documentation, Other")]
    public string Type { get; set; } = "Other";

    [RegularExpression(@"^(Low|Medium|High|Critical)$", 
        ErrorMessage = "Priority must be one of: Low, Medium, High, Critical")]
    public string Priority { get; set; } = "Medium";

    public Guid? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated cost must be non-negative")]
    public decimal? EstimatedCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
    public string? Location { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class UpdateWorkRequestRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Maintenance|Repair|Installation|Inspection|Documentation|Other)$", 
        ErrorMessage = "Type must be one of: Maintenance, Repair, Installation, Inspection, Documentation, Other")]
    public string Type { get; set; } = "Other";

    [RegularExpression(@"^(Low|Medium|High|Critical)$", 
        ErrorMessage = "Priority must be one of: Low, Medium, High, Critical")]
    public string Priority { get; set; } = "Medium";

    [RegularExpression(@"^(Pending|InProgress|Completed|Cancelled|OnHold)$", 
        ErrorMessage = "Status must be one of: Pending, InProgress, Completed, Cancelled, OnHold")]
    public string Status { get; set; } = "Pending";

    public Guid? AssignedToId { get; set; }

    public DateTime? DueDate { get; set; }

    [StringLength(1000, ErrorMessage = "Resolution cannot exceed 1000 characters")]
    public string? Resolution { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Estimated cost must be non-negative")]
    public decimal? EstimatedCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual cost must be non-negative")]
    public decimal? ActualCost { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
    public double? ActualHours { get; set; }

    [StringLength(500, ErrorMessage = "Location cannot exceed 500 characters")]
    public string? Location { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class WorkRequestTaskDto
{
    public Guid WorkRequestTaskId { get; set; }
    public Guid RequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public UserDto? AssignedTo { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public double? EstimatedHours { get; set; }
    public double? ActualHours { get; set; }
    public string? Notes { get; set; } = string.Empty;
}

public class CreateWorkRequestTaskRequest
{
    [Required(ErrorMessage = "Request ID is required")]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Estimated hours must be non-negative")]
    public double? EstimatedHours { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class UpdateWorkRequestTaskRequest
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [RegularExpression(@"^(Pending|InProgress|Completed|Cancelled|OnHold)$", 
        ErrorMessage = "Status must be one of: Pending, InProgress, Completed, Cancelled, OnHold")]
    public string Status { get; set; } = "Pending";

    public Guid? AssignedToUserId { get; set; }

    public DateTime? DueDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Actual hours must be non-negative")]
    public double? ActualHours { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; } = string.Empty;
}

public class WorkRequestCommentDto
{
    public Guid WorkRequestCommentId { get; set; }
    public Guid RequestId { get; set; }
    public Guid AuthorId { get; set; }
    public string? AuthorName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateWorkRequestCommentRequest
{
    [Required(ErrorMessage = "Request ID is required")]
    public Guid RequestId { get; set; }

    [Required(ErrorMessage = "Comment is required")]
    [StringLength(2000, MinimumLength = 1, ErrorMessage = "Comment must be between 1 and 2000 characters")]
    public string Comment { get; set; } = string.Empty;
}

// HATEOAS Link DTO
public class LinkDto
{
    public string Rel { get; set; } = string.Empty;
    public string Href { get; set; } = string.Empty;
    public string Method { get; set; } = "GET";
}

// Work Request Approval DTOs
public class WorkRequestApprovalDto
{
    public Guid ApprovalId { get; set; }
    public Guid WorkRequestId { get; set; }
    public string WorkRequestTitle { get; set; } = string.Empty;
    public Guid ApproverId { get; set; }
    public string ApproverName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string PreviousStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string? Comments { get; set; } = string.Empty;
    public string? RejectionReason { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public bool IsActive { get; set; }
    public string? EscalationReason { get; set; } = string.Empty;
    public DateTime? EscalationDate { get; set; }
}

public class ApprovalRequest
{
    [Required(ErrorMessage = "Action is required")]
    [RegularExpression(@"^(Approve|Reject|Escalate)$", 
        ErrorMessage = "Action must be one of: Approve, Reject, Escalate")]
    public string Action { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
    public string? RejectionReason { get; set; } = string.Empty;

    public Guid? EscalateToUserId { get; set; }

    [StringLength(500, ErrorMessage = "Escalation reason cannot exceed 500 characters")]
    public string? EscalationReason { get; set; } = string.Empty;
}

public class SubmitForApprovalRequest
{
    public Guid? PreferredManagerId { get; set; }
    
    public bool RequiresAdminApproval { get; set; } = false;
    
    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; } = string.Empty;
}

public class ApprovalWorkflowStatusDto
{
    public Guid WorkRequestId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string CurrentStatus { get; set; } = string.Empty;
    public bool RequiresManagerApproval { get; set; }
    public bool RequiresAdminApproval { get; set; }
    public bool IsManagerApproved { get; set; }
    public bool IsAdminApproved { get; set; }
    public string? CurrentApproverName { get; set; } = string.Empty;
    public string? NextApproverName { get; set; } = string.Empty;
    public List<WorkRequestApprovalDto> ApprovalHistory { get; set; } = new();
    public DateTime? SubmittedForApprovalDate { get; set; }
    public DateTime? LastActionDate { get; set; }
    public int DaysPendingApproval { get; set; }
}

public class BulkApprovalRequest
{
    [Required]
    [MinLength(1, ErrorMessage = "At least one work request ID is required")]
    public List<Guid> WorkRequestIds { get; set; } = new();

    [Required(ErrorMessage = "Action is required")]
    [RegularExpression(@"^(Approve|Reject)$", 
        ErrorMessage = "Action must be one of: Approve, Reject")]
    public string Action { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Rejection reason cannot exceed 500 characters")]
    public string? RejectionReason { get; set; } = string.Empty;
}

public class ApprovalStatisticsDto
{
    public int TotalPendingApprovals { get; set; }
    public int ManagerPendingApprovals { get; set; }
    public int AdminPendingApprovals { get; set; }
    public int OverdueApprovalsCount { get; set; }
    public int ApprovalsTodayCount { get; set; }
    public int ApprovalsThisWeekCount { get; set; }
    public int ApprovalsThisMonthCount { get; set; }
    public double AverageApprovalTimeHours { get; set; }
    public List<WorkRequestDto> UrgentPendingApprovals { get; set; } = new();
    public List<WorkRequestDto> OverdueApprovalsList { get; set; } = new();
}


