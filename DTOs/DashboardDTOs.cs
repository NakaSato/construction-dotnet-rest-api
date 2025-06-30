using System.ComponentModel.DataAnnotations;

namespace dotnet_rest_api.DTOs;

/// <summary>
/// Dashboard overview data for real-time monitoring
/// </summary>
public class DashboardOverviewDto
{
    public ProjectSummaryDto ProjectSummary { get; set; } = new();
    public DailyReportSummaryDto DailyReportSummary { get; set; } = new();
    public WorkRequestSummaryDto WorkRequestSummary { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Project summary statistics
/// </summary>
public class ProjectSummaryDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OnHoldProjects { get; set; }
    public double CompletionRate => TotalProjects > 0 ? (double)CompletedProjects / TotalProjects * 100 : 0;
}

/// <summary>
/// Daily report summary statistics
/// </summary>
public class DailyReportSummaryDto
{
    public int TodayReports { get; set; }
    public int PendingApproval { get; set; }
    public int WeeklyReports { get; set; }
    public double ApprovalRate => WeeklyReports > 0 ? (double)(WeeklyReports - PendingApproval) / WeeklyReports * 100 : 0;
}

/// <summary>
/// Work request summary statistics
/// </summary>
public class WorkRequestSummaryDto
{
    public int OpenRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int UrgentRequests { get; set; }
    public int TotalRequests => OpenRequests + InProgressRequests + CompletedRequests;
}

/// <summary>
/// Recent activity item for dashboard feed
/// </summary>
public class RecentActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Project, DailyReport, WorkRequest
    public string Action { get; set; } = string.Empty; // Created, Updated, Approved, etc.
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string? ProjectName { get; set; }
    public Guid? ProjectId { get; set; }
}

/// <summary>
/// Project progress data for real-time updates
/// </summary>
public class ProjectProgressDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EstimatedEndDate { get; set; }
    public DateTime? ActualEndDate { get; set; }
    public string ProjectManager { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int PendingTasks { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Live daily report metrics
/// </summary>
public class LiveDailyReportMetricsDto
{
    public DateTime Date { get; set; }
    public int TotalReports { get; set; }
    public int SubmittedReports { get; set; }
    public int ApprovedReports { get; set; }
    public int PendingReports { get; set; }
    public int RejectedReports { get; set; }
    public double SubmissionRate => TotalReports > 0 ? (double)SubmittedReports / TotalReports * 100 : 0;
    public double ApprovalRate => SubmittedReports > 0 ? (double)ApprovedReports / SubmittedReports * 100 : 0;
    public List<ProjectReportCountDto> ProjectBreakdown { get; set; } = new();
}

/// <summary>
/// Project-specific report count
/// </summary>
public class ProjectReportCountDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public int ReportCount { get; set; }
    public int SubmittedCount { get; set; }
    public int ApprovedCount { get; set; }
}

/// <summary>
/// System alert for dashboard notifications
/// </summary>
public class SystemAlertDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Info, Warning, Error, Critical
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsAcknowledged { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Real-time notification for dashboard updates
/// </summary>
public class DashboardNotificationDto
{
    public string Type { get; set; } = string.Empty; // DataUpdate, Alert, UserAction
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string? TargetUserId { get; set; }
    public string? TargetGroup { get; set; } // All, Managers, Admins, Project-specific
}

/// <summary>
/// Live activity item for real-time activity feed
/// </summary>
public class LiveActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; // Project, DailyReport, WorkRequest
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
