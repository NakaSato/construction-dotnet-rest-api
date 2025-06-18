using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Models;

namespace dotnet_rest_api.Models;

/// <summary>
/// Task model for project management
/// </summary>
public class Task
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal EstimatedHours { get; set; }
    public decimal ActualHours { get; set; }
    public int Progress { get; set; } // 0-100
    
    // Foreign Keys
    public Guid ProjectId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? PhaseId { get; set; }
    
    // Navigation Properties
    public Project Project { get; set; } = null!;
    public User? AssignedTo { get; set; }
    public MasterPlan? Phase { get; set; }
    public ICollection<TaskProgressReport>? ProgressReports { get; set; }
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}

/// <summary>
/// Daily report attachment model
/// </summary>
public class DailyReportAttachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    
    // Foreign Key
    public Guid DailyReportId { get; set; }
    
    // Navigation Property
    public DailyReport DailyReport { get; set; } = null!;
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
}

/// <summary>
/// Task progress report model
/// </summary>
public class TaskProgressReport
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Progress { get; set; } // 0-100
    public decimal HoursWorked { get; set; }
    public string Issues { get; set; } = string.Empty;
    public string NextSteps { get; set; } = string.Empty;
    
    // Foreign Key
    public Guid TaskId { get; set; }
    
    // Navigation Property
    public Task Task { get; set; } = null!;
    
    // Audit Fields
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public Guid? UpdatedById { get; set; }
}
