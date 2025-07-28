using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Services.Shared;
using dotnet_rest_api.Services.Users;
using dotnet_rest_api.Services.Tasks;
using dotnet_rest_api.Services.Projects;
using dotnet_rest_api.Services.MasterPlans;
using dotnet_rest_api.Services.WBS;
using dotnet_rest_api.Services.Infrastructure;
using dotnet_rest_api.Models;
using Asp.Versioning;

namespace dotnet_rest_api.Controllers.V1;

/// <summary>
/// API controller for managing real-time notifications
/// Provides endpoints for notification management, marking as read, and getting notification history
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
[Produces("application/json")]
public class NotificationsController : BaseApiController
{
    private readonly INotificationService _notificationService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        ApplicationDbContext context,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get user's notifications with pagination
    /// Available to: All authenticated users (own notifications only)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<EnhancedPagedResult<NotificationDto>>>> GetNotifications(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<EnhancedPagedResult<NotificationDto>>("Invalid user ID in token", 401);

            var query = _context.WorkRequestNotifications
                .Where(n => n.RecipientId == userId)
                .Include(n => n.WorkRequest)
                .ThenInclude(wr => wr.Project)
                .AsQueryable();

            // Apply filters
            if (isRead.HasValue)
            {
                if (isRead.Value)
                    query = query.Where(n => n.ReadAt != null);
                else
                    query = query.Where(n => n.ReadAt == null);
            }

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<NotificationType>(type, out var notificationType))
            {
                query = query.Where(n => n.Type == notificationType);
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply pagination
            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    WorkRequestId = n.WorkRequestId,
                    WorkRequestTitle = n.WorkRequest.Title,
                    Type = n.Type.ToString(),
                    Status = n.Status.ToString(),
                    Subject = n.Subject,
                    Message = n.Message,
                    CreatedAt = n.CreatedAt,
                    SentAt = n.SentAt,
                    ReadAt = n.ReadAt,
                    IsRead = n.ReadAt != null
                })
                .ToListAsync();

            var result = new EnhancedPagedResult<NotificationDto>
            {
                Items = notifications,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            return ToApiResponse(ServiceResult<EnhancedPagedResult<NotificationDto>>.SuccessResult(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notifications for user");
            return CreateErrorResponse<EnhancedPagedResult<NotificationDto>>("Error retrieving notifications", 500);
        }
    }

    /// <summary>
    /// Get unread notification count
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<ApiResponse<int>>> GetUnreadCount()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<int>("Invalid user ID in token", 401);

            var count = await _context.WorkRequestNotifications
                .Where(n => n.RecipientId == userId && n.ReadAt == null)
                .CountAsync();

            return ToApiResponse(ServiceResult<int>.SuccessResult(count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification count for user");
            return CreateErrorResponse<int>("Error retrieving notification count", 500);
        }
    }

    /// <summary>
    /// Mark notification as read
    /// Available to: All authenticated users (own notifications only)
    /// </summary>
    [HttpPost("{notificationId:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid notificationId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<bool>("Invalid user ID in token", 401);

            await _notificationService.MarkNotificationAsReadAsync(notificationId, userId);

            return ToApiResponse(ServiceResult<bool>.SuccessResult(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification {NotificationId} as read", notificationId);
            return CreateErrorResponse<bool>("Error marking notification as read", 500);
        }
    }

    /// <summary>
    /// Mark all notifications as read
    /// Available to: All authenticated users
    /// </summary>
    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<int>("Invalid user ID in token", 401);

            var unreadNotifications = await _context.WorkRequestNotifications
                .Where(n => n.RecipientId == userId && n.ReadAt == null)
                .ToListAsync();

            var readTime = DateTime.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                notification.ReadAt = readTime;
                notification.Status = NotificationStatus.Read;
            }

            await _context.SaveChangesAsync();

            // Send updated count via SignalR
            await _notificationService.SendNotificationCountUpdateAsync(userId);

            return ToApiResponse(ServiceResult<int>.SuccessResult(unreadNotifications.Count));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read for user");
            return CreateErrorResponse<int>("Error marking notifications as read", 500);
        }
    }

    /// <summary>
    /// Send test notification (for development/testing)
    /// Available to: Administrators only
    /// </summary>
    [HttpPost("test")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> SendTestNotification(
        [FromBody] TestNotificationRequest request)
    {
        try
        {
            await _notificationService.SendNotificationAsync(request.Message, request.UserId);

            return ToApiResponse(ServiceResult<bool>.SuccessResult(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending test notification");
            return CreateErrorResponse<bool>("Error sending test notification", 500);
        }
    }

    /// <summary>
    /// Send system announcement
    /// Available to: Administrators only
    /// </summary>
    [HttpPost("announcement")]
    [Authorize(Roles = "Administrator")]
    public async Task<ActionResult<ApiResponse<bool>>> SendAnnouncement(
        [FromBody] SystemAnnouncementRequest request)
    {
        try
        {
            await _notificationService.SendSystemAnnouncementAsync(
                request.Title, 
                request.Message, 
                request.Priority);

            return ToApiResponse(ServiceResult<bool>.SuccessResult(true));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system announcement");
            return CreateErrorResponse<bool>("Error sending system announcement", 500);
        }
    }

    /// <summary>
    /// Get notification statistics
    /// Available to: Administrators and Managers
    /// </summary>
    [HttpGet("statistics")]
    [Authorize(Roles = "Administrator,Manager")]
    public async Task<ActionResult<ApiResponse<NotificationStatisticsDto>>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            var totalNotifications = await _context.WorkRequestNotifications
                .Where(n => n.CreatedAt >= start && n.CreatedAt <= end)
                .CountAsync();

            var readNotifications = await _context.WorkRequestNotifications
                .Where(n => n.CreatedAt >= start && n.CreatedAt <= end && n.ReadAt != null)
                .CountAsync();

            var notificationsByType = await _context.WorkRequestNotifications
                .Where(n => n.CreatedAt >= start && n.CreatedAt <= end)
                .GroupBy(n => n.Type)
                .Select(g => new { Type = g.Key.ToString(), Count = g.Count() })
                .ToListAsync();

            var statistics = new NotificationStatisticsDto
            {
                TotalNotifications = totalNotifications,
                ReadNotifications = readNotifications,
                UnreadNotifications = totalNotifications - readNotifications,
                ReadRate = totalNotifications > 0 ? (double)readNotifications / totalNotifications * 100 : 0,
                NotificationsByType = notificationsByType.ToDictionary(x => x.Type, x => x.Count),
                PeriodStart = start,
                PeriodEnd = end
            };

            return ToApiResponse(ServiceResult<NotificationStatisticsDto>.SuccessResult(statistics));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving notification statistics");
            return CreateErrorResponse<NotificationStatisticsDto>("Error retrieving statistics", 500);
        }
    }

    #region Real-time SignalR Enhancements

    /// <summary>
    /// Send a test notification to verify SignalR connectivity
    /// Available to: All authenticated users
    /// </summary>
    [HttpPost("test-signalr")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> SendTestNotification([FromBody] string message)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
                return CreateErrorResponse<object>("Invalid user ID in token", 401);

            await _notificationService.SendNotificationAsync(message, userId);

            return CreateSuccessResponse((object)new { Message = "Test notification sent successfully" }, 
                "Test notification sent");
        }
        catch (Exception ex)
        {
            return HandleException<object>(_logger, ex, "sending test notification");
        }
    }

    /// <summary>
    /// Send system-wide announcement
    /// Available to: Administrator only
    /// </summary>
    /// <param name="request">System announcement details</param>
    [HttpPost("system-announcement")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<object>>> SendSystemAnnouncement(
        [FromBody] SystemAnnouncementRequest request)
    {
        try
        {
            await _notificationService.SendSystemAnnouncementAsync(
                request.Title, 
                request.Message, 
                request.Priority);

            return CreateSuccessResponse((object)new { AnnouncementId = Guid.NewGuid() }, 
                "System announcement sent successfully");
        }
        catch (Exception ex)
        {
            return HandleException<object>(_logger, ex, "sending system announcement");
        }
    }

    /// <summary>
    /// Get SignalR connection information for client setup
    /// Available to: All authenticated users
    /// </summary>
    [HttpGet("connection-info")]
    [ProducesResponseType(typeof(ApiResponse<SignalRConnectionInfoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<ApiResponse<SignalRConnectionInfoDto>> GetConnectionInfo()
    {
        try
        {
            var connectionInfo = new SignalRConnectionInfoDto
            {
                HubUrl = "/notificationHub",
                AuthenticationRequired = true,
                SupportedEvents = new[]
                {
                    "ReceiveNotification",
                    "WorkRequestNotification", 
                    "DailyReportUpdate",
                    "DailyReportCreated",
                    "DailyReportStatusChanged",
                    "ProjectStatusUpdate",
                    "ProjectProgressUpdate",
                    "SystemAnnouncement",
                    "NotificationCountUpdated",
                    "ReportFieldUpdated",
                    "UserTyping"
                },
                ConnectionMethods = new[]
                {
                    "JoinProjectGroup",
                    "LeaveProjectGroup", 
                    "JoinDailyReportSession",
                    "LeaveDailyReportSession",
                    "UpdateReportField",
                    "SendTypingIndicator",
                    "MarkNotificationRead",
                    "GetNotificationCount"
                }
            };

            return CreateSuccessResponse(connectionInfo, "SignalR connection information retrieved");
        }
        catch (Exception ex)
        {
            return HandleException<SignalRConnectionInfoDto>(_logger, ex, "retrieving connection info");
        }
    }

    #endregion
}

/// <summary>
/// Request DTO for test notifications
/// </summary>
public class TestNotificationRequest
{
    public string Message { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}

/// <summary>
/// Request DTO for system announcements
/// </summary>
public class SystemAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Info";
}

/// <summary>
/// DTO for notification statistics
/// </summary>
public class NotificationStatisticsDto
{
    public int TotalNotifications { get; set; }
    public int ReadNotifications { get; set; }
    public int UnreadNotifications { get; set; }
    public double ReadRate { get; set; }
    public Dictionary<string, int> NotificationsByType { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

/// <summary>
/// DTO for SignalR connection information
/// </summary>
public class SignalRConnectionInfoDto
{
    public string HubUrl { get; set; } = string.Empty;
    public bool AuthenticationRequired { get; set; }
    public string[] SupportedEvents { get; set; } = Array.Empty<string>();
    public string[] ConnectionMethods { get; set; } = Array.Empty<string>();
}
