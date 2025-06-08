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

public class ProjectTask
{
    [Key]
    public Guid TaskId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    public TaskStatus Status { get; set; }
    
    public DateTime? DueDate { get; set; }
    
    [ForeignKey("AssignedTechnician")]
    public Guid? AssignedTechnicianId { get; set; }
    
    public DateTime? CompletionDate { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User? AssignedTechnician { get; set; }
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}
