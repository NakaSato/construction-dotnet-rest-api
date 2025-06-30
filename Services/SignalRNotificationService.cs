using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Hubs;
using dotnet_rest_api.Models;
using TaskModel = dotnet_rest_api.Models.Task;

namespace dotnet_rest_api.Services;

/// <summary>
/// Real-time notification service using SignalR for live updates
/// Handles work request notifications, daily report updates, and project status changes
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<NotificationHub> hubContext,
        ApplicationDbContext context,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Send a simple notification to a specific user
    /// </summary>
    public async System.Threading.Tasks.Task SendNotificationAsync(string message, Guid userId)
    {
        try
        {
            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("ReceiveNotification", new
                {
                    Id = Guid.NewGuid(),
                    Message = message,
                    Type = "Info",
                    Timestamp = DateTime.UtcNow,
                    UserId = userId
                });

            _logger.LogInformation("Sent notification to user {UserId}: {Message}", userId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Send work request notification with full context
    /// </summary>
    public async System.Threading.Tasks.Task SendWorkRequestNotificationAsync(Guid workRequestId, NotificationType type, Guid recipientId, string message, Guid? senderId = null)
    {
        try
        {
            // Create notification record in database
            var notification = new WorkRequestNotification
            {
                NotificationId = Guid.NewGuid(),
                WorkRequestId = workRequestId,
                RecipientId = recipientId,
                SenderId = senderId,
                Type = type,
                Status = NotificationStatus.Pending,
                Subject = GetNotificationSubject(type),
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkRequestNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Get work request details for rich notification
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest != null)
            {
                var notificationDto = new NotificationDto
                {
                    NotificationId = notification.NotificationId,
                    WorkRequestId = workRequestId,
                    WorkRequestTitle = workRequest.Title,
                    Type = type.ToString(),
                    Status = NotificationStatus.Sent.ToString(),
                    Subject = notification.Subject,
                    Message = message,
                    CreatedAt = notification.CreatedAt
                };

                // Send to specific user
                await _hubContext.Clients.Group($"user_{recipientId}")
                    .SendAsync("WorkRequestNotification", notificationDto);

                // Send to project group if relevant
                await _hubContext.Clients.Group($"project_{workRequest.ProjectId}")
                    .SendAsync("ProjectWorkRequestUpdate", new
                    {
                        ProjectId = workRequest.ProjectId,
                        WorkRequestId = workRequestId,
                        Type = type.ToString(),
                        Title = workRequest.Title,
                        Status = workRequest.Status.ToString(),
                        UpdatedAt = DateTime.UtcNow
                    });

                // Update notification status
                notification.Status = NotificationStatus.Sent;
                notification.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Sent work request notification {Type} for request {WorkRequestId} to user {RecipientId}", 
                    type, workRequestId, recipientId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send work request notification for {WorkRequestId}", workRequestId);
            throw;
        }
    }

    private static string GetNotificationSubject(NotificationType type)
    {
        return type switch
        {
            NotificationType.WorkRequestSubmitted => "Work Request Submitted",
            NotificationType.WorkRequestApproved => "Work Request Approved",
            NotificationType.WorkRequestRejected => "Work Request Rejected", 
            NotificationType.WorkRequestAssigned => "Work Request Assigned",
            NotificationType.WorkRequestCompleted => "Work Request Completed",
            NotificationType.WorkRequestEscalated => "Work Request Escalated",
            NotificationType.WorkRequestDue => "Work Request Due",
            NotificationType.WorkRequestOverdue => "Work Request Overdue",
            NotificationType.ApprovalRequired => "Approval Required",
            NotificationType.ApprovalReminder => "Approval Reminder",
            _ => "Work Request Notification"
        };
    }

    /// <summary>
    /// Send daily report update notification
    /// </summary>
    public async System.Threading.Tasks.Task SendDailyReportNotificationAsync(Guid dailyReportId, string type, Guid projectId, string message)
    {
        try
        {
            // Send to project group
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("DailyReportUpdate", new
                {
                    DailyReportId = dailyReportId,
                    ProjectId = projectId,
                    Type = type,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });

            // Send to managers and admins
            await _hubContext.Clients.Groups(new[] { "role_manager", "role_administrator" })
                .SendAsync("DailyReportManagerUpdate", new
                {
                    DailyReportId = dailyReportId,
                    ProjectId = projectId,
                    Type = type,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("Sent daily report notification for report {DailyReportId}, type {Type}", dailyReportId, type);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily report notification for {DailyReportId}", dailyReportId);
            throw;
        }
    }

    // Enhanced Daily Report Notifications
    public async System.Threading.Tasks.Task SendDailyReportCreatedNotificationAsync(Guid dailyReportId, Guid projectId, string reporterName)
    {
        try
        {
            var notificationData = new
            {
                DailyReportId = dailyReportId,
                ProjectId = projectId,
                Type = "DailyReportCreated",
                Message = $"New daily report submitted by {reporterName}",
                ReporterName = reporterName,
                Timestamp = DateTime.UtcNow
            };

            // Send to project group
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("DailyReportCreated", notificationData);

            // Send to managers and admins
            await _hubContext.Clients.Groups(new[] { "role_manager", "role_administrator" })
                .SendAsync("NewDailyReportAlert", notificationData);

            _logger.LogInformation("Sent daily report created notification for report {DailyReportId}", dailyReportId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily report created notification for {DailyReportId}", dailyReportId);
            throw;
        }
    }

    public async System.Threading.Tasks.Task SendDailyReportApprovalStatusChangeAsync(Guid dailyReportId, Guid projectId, string newStatus, string approverName)
    {
        try
        {
            var notificationData = new
            {
                DailyReportId = dailyReportId,
                ProjectId = projectId,
                Type = "DailyReportStatusChange",
                Status = newStatus,
                ApproverName = approverName,
                Message = $"Daily report status changed to {newStatus} by {approverName}",
                Timestamp = DateTime.UtcNow
            };

            // Send to project group
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("DailyReportStatusChanged", notificationData);

            // Send to report session if anyone is editing
            await _hubContext.Clients.Group($"report_session_{dailyReportId}")
                .SendAsync("ReportStatusUpdated", notificationData);

            _logger.LogInformation("Sent daily report status change notification for report {DailyReportId}, new status: {Status}", 
                dailyReportId, newStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send daily report status change notification for {DailyReportId}", dailyReportId);
            throw;
        }
    }

    public async System.Threading.Tasks.Task SendRealTimeProgressUpdateAsync(Guid projectId, decimal progressPercentage, string milestone = "")
    {
        try
        {
            var updateData = new
            {
                ProjectId = projectId,
                ProgressPercentage = progressPercentage,
                Milestone = milestone,
                Type = "ProgressUpdate",
                Timestamp = DateTime.UtcNow
            };

            // Send to project group
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("ProjectProgressUpdate", updateData);

            // Send to dashboards and management views
            await _hubContext.Clients.Groups(new[] { "role_manager", "role_administrator" })
                .SendAsync("DashboardProgressUpdate", updateData);

            _logger.LogInformation("Sent real-time progress update for project {ProjectId}: {Progress}%", 
                projectId, progressPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send progress update for project {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Send project status update notification
    /// </summary>
    public async System.Threading.Tasks.Task SendProjectStatusUpdateAsync(Guid projectId, string statusUpdate, decimal? completionPercentage = null)
    {
        try
        {
            var updateData = new
            {
                ProjectId = projectId,
                StatusUpdate = statusUpdate,
                CompletionPercentage = completionPercentage,
                Timestamp = DateTime.UtcNow
            };

            // Send to project group
            await _hubContext.Clients.Group($"project_{projectId}")
                .SendAsync("ProjectStatusUpdate", updateData);

            // Send to managers and admins
            await _hubContext.Clients.Groups(new[] { "role_manager", "role_administrator" })
                .SendAsync("ProjectManagerUpdate", updateData);

            _logger.LogInformation("Sent project status update for project {ProjectId}: {StatusUpdate}", projectId, statusUpdate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send project status update for {ProjectId}", projectId);
            throw;
        }
    }

    /// <summary>
    /// Send approval required notification to managers/admins
    /// </summary>
    public async System.Threading.Tasks.Task SendApprovalRequiredNotificationAsync(Guid workRequestId, string title, decimal? estimatedCost)
    {
        try
        {
            var notificationData = new
            {
                WorkRequestId = workRequestId,
                Title = title,
                EstimatedCost = estimatedCost,
                Type = "ApprovalRequired",
                Timestamp = DateTime.UtcNow,
                Message = $"Work request '{title}' requires approval"
            };

            // Send to managers and admins
            await _hubContext.Clients.Groups(new[] { "role_manager", "role_administrator" })
                .SendAsync("ApprovalRequired", notificationData);

            _logger.LogInformation("Sent approval required notification for work request {WorkRequestId}", workRequestId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send approval required notification for {WorkRequestId}", workRequestId);
            throw;
        }
    }

    /// <summary>
    /// Send notification count update to a specific user
    /// </summary>
    public async System.Threading.Tasks.Task SendNotificationCountUpdateAsync(Guid userId)
    {
        try
        {
            var unreadCount = await _context.WorkRequestNotifications
                .Where(n => n.RecipientId == userId && n.ReadAt == null)
                .CountAsync();

            await _hubContext.Clients.Group($"user_{userId}")
                .SendAsync("NotificationCountUpdated", unreadCount);

            _logger.LogInformation("Sent notification count update to user {UserId}: {Count}", userId, unreadCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification count update to user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Broadcast system-wide announcement
    /// </summary>
    public async System.Threading.Tasks.Task SendSystemAnnouncementAsync(string title, string message, string priority = "Info")
    {
        try
        {
            var announcement = new
            {
                Title = title,
                Message = message,
                Priority = priority,
                Timestamp = DateTime.UtcNow,
                Type = "SystemAnnouncement"
            };

            await _hubContext.Clients.All.SendAsync("SystemAnnouncement", announcement);

            _logger.LogInformation("Sent system announcement: {Title}", title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system announcement: {Title}", title);
            throw;
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    public async System.Threading.Tasks.Task MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.WorkRequestNotifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientId == userId);

            if (notification != null)
            {
                notification.ReadAt = DateTime.UtcNow;
                notification.Status = NotificationStatus.Read;
                await _context.SaveChangesAsync();

                // Send updated count
                await SendNotificationCountUpdateAsync(userId);

                _logger.LogInformation("Marked notification {NotificationId} as read for user {UserId}", notificationId, userId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark notification {NotificationId} as read for user {UserId}", notificationId, userId);
            throw;
        }
    }

}
