using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

/// <summary>
/// Resources allocated to project phases (personnel, equipment, materials)
/// </summary>
public class PhaseResource
{
    [Key]
    public Guid PhaseResourceId { get; set; }
    
    [ForeignKey("Phase")]
    public Guid PhaseId { get; set; }
    
    /// <summary>
    /// Type of resource (Personnel, Equipment, Material)
    /// </summary>
    public ResourceType ResourceType { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string ResourceName { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Quantity needed for this phase
    /// </summary>
    public decimal QuantityRequired { get; set; }
    
    /// <summary>
    /// Unit of measurement (hours, pieces, tons, etc.)
    /// </summary>
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    /// <summary>
    /// Cost per unit
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitCost { get; set; }
    
    /// <summary>
    /// Total estimated cost for this resource in this phase
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalEstimatedCost { get; set; }
    
    /// <summary>
    /// Actual quantity used
    /// </summary>
    public decimal ActualQuantityUsed { get; set; } = 0;
    
    /// <summary>
    /// Actual cost incurred
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal ActualCost { get; set; } = 0;
    
    /// <summary>
    /// When this resource is needed (start date)
    /// </summary>
    public DateTime RequiredDate { get; set; }
    
    /// <summary>
    /// Duration this resource is needed (in days)
    /// </summary>
    public int DurationDays { get; set; }
    
    /// <summary>
    /// Current allocation status
    /// </summary>
    public ResourceAllocationStatus AllocationStatus { get; set; } = ResourceAllocationStatus.Planned;
    
    /// <summary>
    /// Supplier or source of this resource
    /// </summary>
    [MaxLength(255)]
    public string? Supplier { get; set; }
    
    /// <summary>
    /// Notes about resource allocation or usage
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public virtual ProjectPhase Phase { get; set; } = null!;
}

public enum ResourceType
{
    Personnel = 1,
    Equipment = 2,
    Material = 3,
    Service = 4,
    Permit = 5,
    Other = 99
}

public enum ResourceAllocationStatus
{
    Planned = 0,
    Requested = 1,
    Approved = 2,
    Ordered = 3,
    InTransit = 4,
    Available = 5,
    InUse = 6,
    Completed = 7,
    Returned = 8,
    Cancelled = 9
}
