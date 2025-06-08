using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum DailyReportStatus
{
    Draft,
    Submitted,
    Approved,
    Rejected,
    RevisionRequested
}

public enum WeatherCondition
{
    Sunny,
    PartlyCloudy,
    Cloudy,
    Rainy,
    Stormy,
    Foggy,
    Snow,
    Windy
}

public class DailyReport
{
    [Key]
    public Guid DailyReportId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    public DateTime ReportDate { get; set; }
    
    public DailyReportStatus Status { get; set; } = DailyReportStatus.Draft;
    
    [ForeignKey("Reporter")]
    public Guid ReporterId { get; set; }
    
    [ForeignKey("SubmittedByUser")]
    public Guid? SubmittedByUserId { get; set; }
    
    public DateTime? SubmittedAt { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    [MaxLength(2000)]
    public string? RejectionReason { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? GeneralNotes { get; set; } = string.Empty;
    
    // Weather Information
    public WeatherCondition? WeatherCondition { get; set; }
    
    [Range(-40, 50)]
    public double? TemperatureHigh { get; set; }
    
    [Range(-40, 50)]
    public double? TemperatureLow { get; set; }
    
    [Range(0, 100)]
    public int? Humidity { get; set; }
    
    [Range(0, 200)]
    public double? WindSpeed { get; set; }
    
    // Work Progress Summary
    public int TotalWorkHours { get; set; }
    
    public int PersonnelOnSite { get; set; }
    
    [MaxLength(1000)]
    public string? SafetyIncidents { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? QualityIssues { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Summary { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? WorkAccomplished { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? WorkPlanned { get; set; } = string.Empty;
    
    [MaxLength(2000)]
    public string? Issues { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User Reporter { get; set; } = null!;
    public virtual User? SubmittedByUser { get; set; }
    public virtual ICollection<WorkProgressItem> WorkProgressItems { get; set; } = new List<WorkProgressItem>();
    public virtual ICollection<PersonnelLog> PersonnelLogs { get; set; } = new List<PersonnelLog>();
    public virtual ICollection<MaterialUsage> MaterialUsages { get; set; } = new List<MaterialUsage>();
    public virtual ICollection<EquipmentLog> EquipmentLogs { get; set; } = new List<EquipmentLog>();
    public virtual ICollection<ImageMetadata> Images { get; set; } = new List<ImageMetadata>();
}

public class WorkProgressItem
{
    [Key]
    public Guid WorkProgressItemId { get; set; }
    
    [ForeignKey("DailyReport")]
    public Guid DailyReportId { get; set; }
    
    [ForeignKey("Task")]
    public Guid? TaskId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Activity { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Range(0, 24)]
    public double HoursWorked { get; set; }
    
    [Range(0, 100)]
    public int PercentageComplete { get; set; }
    
    [Range(0, int.MaxValue)]
    public int WorkersAssigned { get; set; }
    
    [MaxLength(1000)]
    public string? Notes { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Issues { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? NextSteps { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual DailyReport DailyReport { get; set; } = null!;
    public virtual ProjectTask? Task { get; set; }
}

public class PersonnelLog
{
    [Key]
    public Guid PersonnelLogId { get; set; }
    
    [ForeignKey("DailyReport")]
    public Guid DailyReportId { get; set; }
    
    [ForeignKey("User")]
    public Guid UserId { get; set; }
    
    [Range(0, 24)]
    public double HoursWorked { get; set; }
    
    [MaxLength(200)]
    public string? Position { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual DailyReport DailyReport { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}

public class MaterialUsage
{
    [Key]
    public Guid MaterialUsageId { get; set; }
    
    [ForeignKey("DailyReport")]
    public Guid DailyReportId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string MaterialName { get; set; } = string.Empty;
    
    [Required]
    public double QuantityUsed { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue)]
    public decimal? Cost { get; set; }
    
    [MaxLength(200)]
    public string? Supplier { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual DailyReport DailyReport { get; set; } = null!;
}

public class EquipmentLog
{
    [Key]
    public Guid EquipmentLogId { get; set; }
    
    [ForeignKey("DailyReport")]
    public Guid DailyReportId { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string EquipmentName { get; set; } = string.Empty;
    
    [Range(0, 24)]
    public double HoursUsed { get; set; }
    
    [MaxLength(100)]
    public string? OperatorName { get; set; } = string.Empty;
    
    public bool MaintenanceRequired { get; set; }
    
    [MaxLength(500)]
    public string? MaintenanceNotes { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Purpose { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Issues { get; set; } = string.Empty;
    
    [MaxLength(1000)]
    public string? Notes { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public virtual DailyReport DailyReport { get; set; } = null!;
}
