using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Hubs;
using dotnet_rest_api.Services;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// Real-time dashboard API controller for live project monitoring
/// Provides endpoints for dashboard data and real-time updates
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/dashboard")]
[Authorize]
[Produces("application/json")]
public class DashboardController : BaseApiController
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly NotificationBackgroundService _backgroundService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        ApplicationDbContext context,
        IHubContext<NotificationHub> hubContext,
        NotificationBackgroundService backgroundService,
        ILogger<DashboardController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _backgroundService = backgroundService;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time dashboard overview data
    /// Includes project counts, recent activities, and live statistics
    /// </summary>
    [HttpGet("overview")]
    public async Task<ActionResult<ApiResponse<DashboardOverviewDto>>> GetDashboardOverview()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var weekStart = today.AddDays(-(int)today.DayOfWeek);

            var overview = new DashboardOverviewDto
            {
                ProjectSummary = new ProjectSummaryDto
                {
                    TotalProjects = await _context.Projects.CountAsync(),
                    ActiveProjects = await _context.Projects.CountAsync(p => p.Status == "Active"),
                    CompletedProjects = await _context.Projects.CountAsync(p => p.Status == "Completed"),
                    OnHoldProjects = await _context.Projects.CountAsync(p => p.Status == "On Hold")
                },
                
                DailyReportSummary = new DailyReportSummaryDto
                {
                    TodayReports = await _context.DailyReports.CountAsync(dr => dr.Date.Date == today),
                    PendingApproval = await _context.DailyReports.CountAsync(dr => dr.Status == "Pending"),
                    WeeklyReports = await _context.DailyReports.CountAsync(dr => dr.Date >= weekStart)
                },
                
                WorkRequestSummary = new WorkRequestSummaryDto
                {
                    OpenRequests = await _context.WorkRequests.CountAsync(wr => wr.Status == "Open"),
                    InProgressRequests = await _context.WorkRequests.CountAsync(wr => wr.Status == "In Progress"),
                    CompletedRequests = await _context.WorkRequests.CountAsync(wr => wr.Status == "Completed"),
                    UrgentRequests = await _context.WorkRequests.CountAsync(wr => wr.Priority == "Urgent")
                },
                
                RecentActivities = await GetRecentActivitiesAsync(),
                LastUpdated = DateTime.UtcNow
            };

            return SuccessResponse(overview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard overview");
            return ErrorResponse<DashboardOverviewDto>("Failed to load dashboard overview", 500);
        }
    }

    /// <summary>
    /// Get real-time project progress data
    /// Returns progress information that's continuously updated via SignalR
    /// </summary>
    [HttpGet("project-progress")]
    public async Task<ActionResult<ApiResponse<List<ProjectProgressDto>>>> GetProjectProgress()
    {
        try
        {
            var projectProgress = await _context.Projects
                .Select(p => new ProjectProgressDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    CompletionPercentage = p.CompletionPercentage,
                    Status = p.Status,
                    StartDate = p.CreatedAt,
                    EndDate = p.ExpectedEndDate,
                    TasksCompleted = _context.Tasks.Count(t => t.ProjectId == p.Id && t.Status == "Completed"),
                    TotalTasks = _context.Tasks.Count(t => t.ProjectId == p.Id),
                    LastUpdated = DateTime.UtcNow
                })
                .ToListAsync();

            return SuccessResponse(projectProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving project progress");
            return ErrorResponse<List<ProjectProgressDto>>("Failed to load project progress", 500);
        }
    }

    /// <summary>
    /// Trigger a real-time progress update broadcast
    /// Sends updated progress to all connected clients
    /// </summary>
    [HttpPost("broadcast-progress/{projectId}")]
    public async Task<ActionResult<ApiResponse<string>>> BroadcastProgressUpdate(Guid projectId)
    {
        try
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null)
            {
                return NotFoundResponse<string>("Project not found");
            }

            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System";
            
            // Queue background notification
            _backgroundService.QueueNotification(new NotificationQueueItem
            {
                Type = "ProjectProgressUpdate",
                Message = $"Project progress updated: {project.Name}",
                Data = new Dictionary<string, object>
                {
                    { "ProjectId", projectId },
                    { "CompletionPercentage", project.CompletionPercentage },
                    { "UpdatedBy", userName }
                },
                TargetGroup = $"project_{projectId}",
                Priority = 0
            });

            // Send immediate SignalR update
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("RealTimeProgressUpdate", new
                {
                    ProjectId = projectId,
                    CompletionPercentage = project.CompletionPercentage,
                    UpdatedBy = userName,
                    Timestamp = DateTime.UtcNow
                });

            return SuccessResponse("Progress update broadcasted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting progress update for project {ProjectId}", projectId);
            return ErrorResponse<string>("Failed to broadcast progress update", 500);
        }
    }

    /// <summary>
    /// Get live user activity feed
    /// Returns recent user actions and activities
    /// </summary>
    [HttpGet("live-activity")]
    public async Task<ActionResult<ApiResponse<List<LiveActivityDto>>>> GetLiveActivity(
        [FromQuery] int limit = 20)
    {
        try
        {
            var activities = new List<LiveActivityDto>();

            // Get recent daily reports
            var recentReports = await _context.DailyReports
                .OrderByDescending(dr => dr.CreatedAt)
                .Take(limit / 3)
                .Select(dr => new LiveActivityDto
                {
                    Id = dr.Id,
                    Type = "DailyReport",
                    Title = $"Daily Report - {dr.Date:MMM dd}",
                    Description = $"Report submitted for project",
                    UserId = dr.SubmittedById,
                    ProjectId = dr.ProjectId,
                    Timestamp = dr.CreatedAt,
                    Status = dr.Status
                })
                .ToListAsync();

            activities.AddRange(recentReports);

            // Get recent work requests
            var recentWorkRequests = await _context.WorkRequests
                .OrderByDescending(wr => wr.CreatedAt)
                .Take(limit / 3)
                .Select(wr => new LiveActivityDto
                {
                    Id = wr.Id,
                    Type = "WorkRequest",
                    Title = wr.Title,
                    Description = wr.Description,
                    UserId = wr.RequestedById,
                    ProjectId = wr.ProjectId,
                    Timestamp = wr.CreatedAt,
                    Status = wr.Status,
                    Priority = wr.Priority
                })
                .ToListAsync();

            activities.AddRange(recentWorkRequests);

            // Get recent task updates
            var recentTasks = await _context.Tasks
                .OrderByDescending(t => t.UpdatedAt)
                .Take(limit / 3)
                .Select(t => new LiveActivityDto
                {
                    Id = t.Id,
                    Type = "Task",
                    Title = t.Title,
                    Description = t.Description,
                    UserId = t.AssignedToId,
                    ProjectId = t.ProjectId,
                    Timestamp = t.UpdatedAt,
                    Status = t.Status,
                    Priority = t.Priority
                })
                .ToListAsync();

            activities.AddRange(recentTasks);

            // Sort by timestamp and take the requested limit
            var sortedActivities = activities
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .ToList();

            return SuccessResponse(sortedActivities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving live activity feed");
            return ErrorResponse<List<LiveActivityDto>>("Failed to load activity feed", 500);
        }
    }

    /// <summary>
    /// Send a live system announcement to all connected users
    /// </summary>
    [HttpPost("system-announcement")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<ActionResult<ApiResponse<string>>> SendSystemAnnouncement(
        [FromBody] SystemAnnouncementRequestForDashboard request)
    {
        try
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "System Admin";

            // Queue background notification for persistence and email
            _backgroundService.QueueNotification(new NotificationQueueItem
            {
                Type = "SystemAnnouncement",
                Message = request.Message,
                Data = new Dictionary<string, object>
                {
                    { "Priority", request.Priority },
                    { "AnnouncedBy", userName },
                    { "ExpiresAt", request.ExpiresAt ?? DateTime.UtcNow.AddDays(7) }
                },
                TargetGroup = "all_users",
                Priority = request.Priority switch
                {
                    "Urgent" => 2,
                    "High" => 1,
                    _ => 0
                }
            });

            // Send immediate SignalR broadcast
            await _hubContext.Clients.All.SendAsync("SystemAnnouncement", new
            {
                Id = Guid.NewGuid(),
                Message = request.Message,
                Priority = request.Priority,
                AnnouncedBy = userName,
                Timestamp = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt
            });

            _logger.LogInformation("System announcement sent by {UserName}: {Message}", userName, request.Message);
            return SuccessResponse("System announcement sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system announcement");
            return ErrorResponse<string>("Failed to send system announcement", 500);
        }
    }

    /// <summary>
    /// Get dashboard statistics for analytics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<ActionResult<ApiResponse<DashboardStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var statistics = new DashboardStatisticsDto
            {
                Period = new { StartDate = start, EndDate = end },
                
                ProjectStatistics = new
                {
                    Created = await _context.Projects.CountAsync(p => p.CreatedAt >= start && p.CreatedAt <= end),
                    Completed = await _context.Projects.CountAsync(p => p.Status == "Completed" && p.UpdatedAt >= start && p.UpdatedAt <= end),
                    AverageCompletionTime = await CalculateAverageCompletionTimeAsync(start, end)
                },
                
                DailyReportStatistics = new
                {
                    Total = await _context.DailyReports.CountAsync(dr => dr.Date >= start.Date && dr.Date <= end.Date),
                    Approved = await _context.DailyReports.CountAsync(dr => dr.Status == "Approved" && dr.Date >= start.Date && dr.Date <= end.Date),
                    Pending = await _context.DailyReports.CountAsync(dr => dr.Status == "Pending"),
                    ApprovalRate = await CalculateApprovalRateAsync(start, end)
                },
                
                WorkRequestStatistics = new
                {
                    Created = await _context.WorkRequests.CountAsync(wr => wr.CreatedAt >= start && wr.CreatedAt <= end),
                    Resolved = await _context.WorkRequests.CountAsync(wr => wr.Status == "Completed" && wr.UpdatedAt >= start && wr.UpdatedAt <= end),
                    AverageResolutionTime = await CalculateAverageResolutionTimeAsync(start, end)
                },
                
                LastUpdated = DateTime.UtcNow
            };

            return SuccessResponse(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics");
            return ErrorResponse<DashboardStatisticsDto>("Failed to load statistics", 500);
        }
    }

    private async Task<List<RecentActivityDto>> GetRecentActivitiesAsync()
    {
        var activities = new List<RecentActivityDto>();

        // Recent project updates
        var recentProjects = await _context.Projects
            .OrderByDescending(p => p.UpdatedAt)
            .Take(5)
            .Select(p => new RecentActivityDto
            {
                Type = "Project",
                Title = $"Project Updated: {p.Name}",
                Timestamp = p.UpdatedAt,
                ProjectId = p.Id
            })
            .ToListAsync();

        activities.AddRange(recentProjects);

        // Recent daily reports
        var recentReports = await _context.DailyReports
            .OrderByDescending(dr => dr.CreatedAt)
            .Take(5)
            .Select(dr => new RecentActivityDto
            {
                Type = "DailyReport",
                Title = $"Daily Report: {dr.Date:MMM dd, yyyy}",
                Timestamp = dr.CreatedAt,
                ProjectId = dr.ProjectId
            })
            .ToListAsync();

        activities.AddRange(recentReports);

        return activities.OrderByDescending(a => a.Timestamp).Take(10).ToList();
    }

    private async Task<double> CalculateAverageCompletionTimeAsync(DateTime start, DateTime end)
    {
        var completedProjects = await _context.Projects
            .Where(p => p.Status == "Completed" && p.UpdatedAt >= start && p.UpdatedAt <= end)
            .Select(p => new { p.CreatedAt, p.UpdatedAt })
            .ToListAsync();

        if (!completedProjects.Any())
            return 0;

        var totalDays = completedProjects.Sum(p => (p.UpdatedAt - p.CreatedAt).TotalDays);
        return totalDays / completedProjects.Count;
    }

    private async Task<double> CalculateApprovalRateAsync(DateTime start, DateTime end)
    {
        var totalReports = await _context.DailyReports
            .CountAsync(dr => dr.Date >= start.Date && dr.Date <= end.Date);

        if (totalReports == 0)
            return 0;

        var approvedReports = await _context.DailyReports
            .CountAsync(dr => dr.Status == "Approved" && dr.Date >= start.Date && dr.Date <= end.Date);

        return (double)approvedReports / totalReports * 100;
    }

    private async Task<double> CalculateAverageResolutionTimeAsync(DateTime start, DateTime end)
    {
        var resolvedRequests = await _context.WorkRequests
            .Where(wr => wr.Status == "Completed" && wr.UpdatedAt >= start && wr.UpdatedAt <= end)
            .Select(wr => new { wr.CreatedAt, wr.UpdatedAt })
            .ToListAsync();

        if (!resolvedRequests.Any())
            return 0;

        var totalHours = resolvedRequests.Sum(wr => (wr.UpdatedAt - wr.CreatedAt).TotalHours);
        return totalHours / resolvedRequests.Count;
    }
}

// Supporting DTOs for dashboard functionality
public class DashboardOverviewDto
{
    public ProjectSummaryDto ProjectSummary { get; set; } = new();
    public DailyReportSummaryDto DailyReportSummary { get; set; } = new();
    public WorkRequestSummaryDto WorkRequestSummary { get; set; } = new();
    public List<RecentActivityDto> RecentActivities { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class ProjectSummaryDto
{
    public int TotalProjects { get; set; }
    public int ActiveProjects { get; set; }
    public int CompletedProjects { get; set; }
    public int OnHoldProjects { get; set; }
}

public class DailyReportSummaryDto
{
    public int TodayReports { get; set; }
    public int PendingApproval { get; set; }
    public int WeeklyReports { get; set; }
}

public class WorkRequestSummaryDto
{
    public int OpenRequests { get; set; }
    public int InProgressRequests { get; set; }
    public int CompletedRequests { get; set; }
    public int UrgentRequests { get; set; }
}

public class ProjectProgressDto
{
    public Guid ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal CompletionPercentage { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int TasksCompleted { get; set; }
    public int TotalTasks { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class LiveActivityDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public Guid? ProjectId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Priority { get; set; }
}

public class RecentActivityDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public Guid? ProjectId { get; set; }
}

public class DashboardStatisticsDto
{
    public object Period { get; set; } = new();
    public object ProjectStatistics { get; set; } = new();
    public object DailyReportStatistics { get; set; } = new();
    public object WorkRequestStatistics { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class SystemAnnouncementRequestForDashboard
{
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal"; // Normal, High, Urgent
    public DateTime? ExpiresAt { get; set; }
}
