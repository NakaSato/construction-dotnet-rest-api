using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace dotnet_rest_api.Models;

public enum WeeklyReportStatus
{
    Draft = 0,
    Submitted = 1,
    Approved = 2
}

public class WeeklyReport
{
    [Key]
    public Guid WeeklyReportId { get; set; }
    
    [Required]
    [ForeignKey("Project")]
    public Guid ProjectId { get; set; }
    
    [Required]
    public DateTime WeekStartDate { get; set; }
    
    public WeeklyReportStatus Status { get; set; } = WeeklyReportStatus.Draft;
    
    [Required]
    [MaxLength(2000)]
    public string SummaryOfProgress { get; set; } = string.Empty;
    
    // Aggregated Metrics (stored as JSON or individual fields)
    public int TotalManHours { get; set; }
    public int PanelsInstalled { get; set; }
    public int SafetyIncidents { get; set; }
    public int DelaysReported { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string MajorAccomplishments { get; set; } = "[]"; // JSON array of strings
    
    [Column(TypeName = "jsonb")]
    public string MajorIssues { get; set; } = "[]"; // JSON array of issue objects
    
    [MaxLength(2000)]
    public string? Lookahead { get; set; }
    
    [Range(0, 100)]
    public int CompletionPercentage { get; set; } = 0;
    
    [ForeignKey("SubmittedByUser")]
    public Guid SubmittedById { get; set; }
    
    [ForeignKey("ApprovedByUser")]
    public Guid? ApprovedById { get; set; }
    
    public DateTime? ApprovedAt { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public virtual Project Project { get; set; } = null!;
    public virtual User SubmittedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
}
