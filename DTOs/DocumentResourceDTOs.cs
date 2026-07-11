namespace dotnet_rest_api.DTOs;

// NOTE: Document and Resource DTOs/enums were removed in Phase 5 (stub reconciliation)
// when the Documents and Resources stub features were deleted. The DTOs below remain
// because they are consumed by the DailyReports feature.

/// <summary>
/// Daily report attachment DTO
/// </summary>
public class DailyReportAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public Guid DailyReportId { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}

/// <summary>
/// Weekly summary DTO
/// </summary>
public class WeeklySummaryDto
{
    public DateTime WeekStartDate { get; set; }
    public DateTime WeekEndDate { get; set; }
    public int TotalReports { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public decimal TotalHoursWorked { get; set; }
    public decimal AverageProgress { get; set; }
    public List<string> TopIssues { get; set; } = new();
    public List<string> CompletedMilestones { get; set; } = new();
    public List<DailyReportDto> Reports { get; set; } = new();
}
