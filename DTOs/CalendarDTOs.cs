using dotnet_rest_api.Models;
using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// DTO for creating a new calendar event
/// </summary>
public class CreateCalendarEventDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    public bool IsAllDay { get; set; } = false;

    [Required]
    public CalendarEventType EventType { get; set; }

    public CalendarEventStatus Status { get; set; } = CalendarEventStatus.Scheduled;

    public CalendarEventPriority Priority { get; set; } = CalendarEventPriority.Medium;

    [StringLength(500)]
    public string? Location { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? TaskId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    // Recurrence properties
    public bool IsRecurring { get; set; } = false;

    [StringLength(100)]
    public string? RecurrencePattern { get; set; }

    public DateTime? RecurrenceEndDate { get; set; }

    // Additional properties
    public int ReminderMinutes { get; set; } = 15;

    public bool IsPrivate { get; set; } = false;

    [StringLength(500)]
    public string? MeetingUrl { get; set; }

    [StringLength(1000)]
    public string? Attendees { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for updating an existing calendar event
/// </summary>
public class UpdateCalendarEventDto
{
    [StringLength(200)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? StartDateTime { get; set; }

    public DateTime? EndDateTime { get; set; }

    public bool? IsAllDay { get; set; }

    public CalendarEventType? EventType { get; set; }

    public CalendarEventStatus? Status { get; set; }

    public CalendarEventPriority? Priority { get; set; }

    [StringLength(500)]
    public string? Location { get; set; }

    public Guid? ProjectId { get; set; }

    public Guid? TaskId { get; set; }

    public Guid? AssignedToUserId { get; set; }

    // Recurrence properties
    public bool? IsRecurring { get; set; }

    [StringLength(100)]
    public string? RecurrencePattern { get; set; }

    public DateTime? RecurrenceEndDate { get; set; }

    // Additional properties
    public int? ReminderMinutes { get; set; }

    public bool? IsPrivate { get; set; }

    [StringLength(500)]
    public string? MeetingUrl { get; set; }

    [StringLength(1000)]
    public string? Attendees { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for calendar event responses
/// </summary>
public class CalendarEventResponseDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public CalendarEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public CalendarEventStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public CalendarEventPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskName { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    // Recurrence properties
    public bool IsRecurring { get; set; }
    public string? RecurrencePattern { get; set; }
    public DateTime? RecurrenceEndDate { get; set; }

    // Additional properties
    public int ReminderMinutes { get; set; }
    public bool IsPrivate { get; set; }
    public string? MeetingUrl { get; set; }
    public string? Attendees { get; set; }
    public string? Notes { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for calendar event summary (used in lists)
/// </summary>
public class CalendarEventSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public bool IsAllDay { get; set; }
    public CalendarEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public CalendarEventStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public CalendarEventPriority Priority { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ProjectName { get; set; }
    public string? TaskName { get; set; }
    public bool IsRecurring { get; set; }
}

/// <summary>
/// DTO for calendar query parameters
/// </summary>
public class CalendarQueryDto
{
    /// <summary>
    /// Start date for filtering events (inclusive)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for filtering events (inclusive)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Filter by event type
    /// </summary>
    public CalendarEventType? EventType { get; set; }

    /// <summary>
    /// Filter by event status
    /// </summary>
    public CalendarEventStatus? Status { get; set; }

    /// <summary>
    /// Filter by priority
    /// </summary>
    public CalendarEventPriority? Priority { get; set; }

    /// <summary>
    /// Filter by project ID
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Filter by task ID
    /// </summary>
    public Guid? TaskId { get; set; }

    /// <summary>
    /// Filter by assigned user ID
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Filter by created user ID
    /// </summary>
    public Guid? CreatedByUserId { get; set; }

    /// <summary>
    /// Include private events (default: false)
    /// </summary>
    public bool IncludePrivate { get; set; } = false;

    /// <summary>
    /// Search term for title and description
    /// </summary>
    public string? Search { get; set; }

    /// <summary>
    /// Page number for pagination (1-based)
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Page size for pagination (max 100)
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Sort field (StartDateTime, Title, CreatedAt)
    /// </summary>
    public string SortBy { get; set; } = "StartDateTime";

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string SortOrder { get; set; } = "asc";
}

/// <summary>
/// DTO for paginated calendar event results
/// </summary>
public class PaginatedCalendarEventsDto
{
    public IEnumerable<CalendarEventSummaryDto> Events { get; set; } = new List<CalendarEventSummaryDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

/// <summary>
/// Request model for checking event conflicts
/// </summary>
public class ConflictCheckRequest
{
    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    public Guid? UserId { get; set; }

    public Guid? ExcludeEventId { get; set; }
}

/// <summary>
/// Result model for conflict checking
/// </summary>
public class ConflictCheckResult
{
    public bool HasConflicts { get; set; }
    public List<CalendarEventSummaryDto> ConflictingEvents { get; set; } = new();
}