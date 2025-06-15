using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum WeeklyRequestStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2,
    InProgress = 3,
    Completed = 4
}

public class WeeklyWorkRequest
{
    [Key]
    public Guid WeeklyRequestId { get; set; }
    
    [Required]
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    public DateTime WeekStartDate { get; set; }
    
    public WeeklyRequestStatus Status { get; set; } = WeeklyRequestStatus.Draft;
    
    [Required]
    [MaxLength(2000)]
    public string OverallGoals { get; set; } = string.Empty;
    
    [Column(TypeName = "jsonb")]
    public string KeyTasks { get; set; } = "[]"; // JSON array of strings
    
    [MaxLength(1000)]
    public string? PersonnelForecast { get; set; }
    
    [MaxLength(1000)]
    public string? MajorEquipment { get; set; }
    
    [MaxLength(1000)]
    public string? CriticalMaterials { get; set; }
    
    public int EstimatedHours { get; set; } = 0;
    
    [MaxLength(50)]
    public string? Priority { get; set; }
    
    [MaxLength(100)]
    public string? Type { get; set; }
    
    [ForeignKey("RequestedByUser")]
    public Guid RequestedById { get; set; }
    
    [ForeignKey("ApprovedByUser")]
    public Guid? ApprovedById { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User RequestedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
}
