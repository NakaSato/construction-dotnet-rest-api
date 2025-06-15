using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum TaskStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4
}

public enum TaskPriority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Critical = 3
}

public class ProjectTask
{
    [Key]
    public Guid TaskId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    /// <summary>
    /// Link task to a specific phase in the master plan
    /// </summary>
    [ForeignKey("Phase")]
    public Guid? PhaseId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public TaskStatus Status { get; set; }
    
    /// <summary>
    /// Task priority level
    /// </summary>
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    
    public DateTime? DueDate { get; set; }
    
    [ForeignKey("AssignedTechnician")]
    public Guid? AssignedTechnicianId { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    
    /// <summary>
    /// Estimated hours to complete this task
    /// </summary>
    public decimal EstimatedHours { get; set; }
    
    /// <summary>
    /// Actual hours spent on this task
    /// </summary>
    public decimal ActualHours { get; set; } = 0;
    
    /// <summary>
    /// Completion percentage (0-100)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal CompletionPercentage { get; set; } = 0;
    
    /// <summary>
    /// Weight of this task in the overall phase completion (0-100)
    /// </summary>
    [Column(TypeName = "decimal(5,2)")]
    public decimal WeightInPhase { get; set; } = 0;
    
    /// <summary>
    /// Dependencies - tasks that must be completed before this one
    /// </summary>
    [MaxLength(500)]
    public string? Dependencies { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ProjectPhase? Phase { get; set; }
    public virtual User? AssignedTechnician { get; set; }
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}
