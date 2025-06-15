using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum ProjectStatus
{
    Planning = 0,
    InProgress = 1,
    Completed = 2,
    OnHold = 3,
    Cancelled = 4
}

public class Project
{
    [Key]
    public Guid ProjectId { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ProjectName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(500)]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string ClientInfo { get; set; } = string.Empty;
    
    public ProjectStatus Status { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EstimatedEndDate { get; set; }
    
    public DateTime? ActualEndDate { get; set; }
    
    [ForeignKey("ProjectManager")]
    public Guid ProjectManagerId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    // Solar Project Specific Fields
    [MaxLength(50)]
    public string? Team { get; set; }
    
    [MaxLength(20)]
    public string? ConnectionType { get; set; }
    
    [MaxLength(1000)]
    public string? ConnectionNotes { get; set; }
    
    [Column(TypeName = "decimal(10,2)")]
    public decimal? TotalCapacityKw { get; set; }
    
    public int? PvModuleCount { get; set; }
    
    // Equipment Details
    public int? Inverter125kw { get; set; }
    public int? Inverter80kw { get; set; }
    public int? Inverter60kw { get; set; }
    public int? Inverter40kw { get; set; }
    
    // Business Values
    public int? FtsValue { get; set; }
    public int? RevenueValue { get; set; }
    public int? PqmValue { get; set; }
    
    // Location Coordinates
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Latitude { get; set; }
    
    [Column(TypeName = "decimal(10,7)")]
    public decimal? Longitude { get; set; }
    
    // Navigation properties
    public virtual User ProjectManager { get; set; } = null!;
    public virtual ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}
