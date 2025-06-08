using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public class ImageMetadata
{
    [Key]
    public Guid ImageId { get; set; }
    
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [ForeignKey("Task")]
    public Guid? TaskId { get; set; }
    
    [ForeignKey("DailyReport")]
    public Guid? DailyReportId { get; set; }
    
    [ForeignKey("WorkRequest")]
    public Guid? WorkRequestId { get; set; }
    
    [ForeignKey("UploadedByUser")]
    public Guid UploadedByUserId { get; set; }
    
    [Required]
    [MaxLength(500)]
    public string CloudStorageKey { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ContentType { get; set; } = string.Empty;
    
    public long FileSizeInBytes { get; set; }
    
    public DateTime UploadTimestamp { get; set; }
    
    public DateTime? CaptureTimestamp { get; set; }
    
    public double? GPSLatitude { get; set; }
    
    public double? GPSLongitude { get; set; }
    
    [MaxLength(255)]
    public string? DeviceModel { get; set; }
    
    public short? Orientation { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string? EXIFData { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual ProjectTask? Task { get; set; }
    public virtual DailyReport? DailyReport { get; set; }
    public virtual WorkRequest? WorkRequest { get; set; }
    public virtual User UploadedByUser { get; set; } = null!;
}
