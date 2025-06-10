using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Represents a calendar event for project planning and scheduling
/// </summary>
public class CalendarEvent
{
    [Key]
    public Guid EventId { get; set; }

    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; } = string.Empty;

    [Required]
    public DateTime StartDateTime { get; set; }

    [Required]
    public DateTime EndDateTime { get; set; }

    [Required]
    public CalendarEventType EventType { get; set; }

    [Required]
    public CalendarEventStatus Status { get; set; } = CalendarEventStatus.Scheduled;

    [Required]
    public CalendarEventPriority Priority { get; set; } = CalendarEventPriority.Medium;

    [MaxLength(500)]
    public string? Location { get; set; } = string.Empty;

    public bool IsAllDay { get; set; } = false;

    public bool IsRecurring { get; set; } = false;

    [MaxLength(100)]
    public string? RecurrencePattern { get; set; } = string.Empty; // Daily, Weekly, Monthly, etc.

    public DateTime? RecurrenceEndDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; } = string.Empty;

    public int? ReminderMinutes { get; set; } // Minutes before event to send reminder

    // Foreign Keys
    [ForeignKey("Project")]
    public Guid? ProjectId { get; set; }

    [ForeignKey("Task")]
    public Guid? TaskId { get; set; }

    [ForeignKey("CreatedBy")]
    public Guid CreatedByUserId { get; set; }

    [ForeignKey("AssignedTo")]
    public Guid? AssignedToUserId { get; set; }

    // Audit fields
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Project? Project { get; set; }
    public virtual ProjectTask? Task { get; set; }
    public virtual User CreatedBy { get; set; } = null!;
    public virtual User? AssignedTo { get; set; }

    // Calendar-specific properties
    [MaxLength(50)]
    public string? Color { get; set; } = "#3788d8"; // Default blue color

    public bool IsPrivate { get; set; } = false;

    [MaxLength(255)]
    public string? MeetingUrl { get; set; } = string.Empty; // For virtual meetings

    [MaxLength(2000)]
    public string? Attendees { get; set; } = string.Empty; // JSON array of attendee emails/names
}

/// <summary>
/// Types of calendar events
/// </summary>
public enum CalendarEventType
{
    Meeting = 1,
    ProjectDeadline = 2,
    TaskDueDate = 3,
    Inspection = 4,
    Installation = 5,
    Maintenance = 6,
    ClientCall = 7,
    Planning = 8,
    Review = 9,
    Training = 10,
    Other = 11
}

/// <summary>
/// Status of calendar events
/// </summary>
public enum CalendarEventStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4,
    Postponed = 5,
    Rescheduled = 6
}

/// <summary>
/// Priority levels for calendar events
/// </summary>
public enum CalendarEventPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}