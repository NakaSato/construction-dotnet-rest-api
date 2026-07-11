using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Hubs;
using dotnet_rest_api.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
// Models defines a `Task` entity that collides with System.Threading.Tasks.Task;
// alias the bare return type so method signatures resolve unambiguously.
using Task = System.Threading.Tasks.Task;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Real implementation of <see cref="INotificationService"/>. Replaces the former
/// StubNotificationService: pushes live events over the SignalR
/// <see cref="NotificationHub"/> and persists work-request notifications to the
/// WorkRequestNotifications table (the only notification store in the schema).
///
/// Group naming mirrors <see cref="NotificationHub"/>: <c>user_{userId}</c>,
/// <c>project_{projectId}</c>, <c>role_{role}</c>. Event names match the
/// SupportedEvents advertised by NotificationsController's connection-info.
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hub;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hub,
        ApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _hub = hub;
        _context = context;
        _logger = logger;
    }

    // ------------------------------------------------------------ Push only --

    public Task SendNotificationAsync(Guid userId, string message, string type = "info")
        => _hub.Clients.Group(UserGroup(userId)).SendAsync("ReceiveNotification", Payload(message, type));

    public Task SendNotificationAsync(string message, Guid userId)
        => SendNotificationAsync(userId, message);

    public Task SendNotificationToAllAsync(string message, string type = "info")
        => _hub.Clients.All.SendAsync("ReceiveNotification", Payload(message, type));

    public Task SendNotificationToGroupAsync(IEnumerable<Guid> userIds, string message, string type = "info")
    {
        var groups = userIds.Distinct().Select(UserGroup).ToList();
        if (groups.Count == 0)
            return Task.CompletedTask;
        return _hub.Clients.Groups(groups).SendAsync("ReceiveNotification", Payload(message, type));
    }

    public Task SendProjectNotificationAsync(Guid projectId, string message, string type = "info")
        => _hub.Clients.Group(ProjectGroup(projectId)).SendAsync("ReceiveNotification", new
        {
            ProjectId = projectId,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });

    public Task SendTaskNotificationAsync(Guid taskId, string message, string type = "info")
        => _hub.Clients.All.SendAsync("ReceiveNotification", new
        {
            TaskId = taskId,
            Message = message,
            Type = type,
            Timestamp = DateTime.UtcNow
        });

    public Task SendSystemAnnouncementAsync(string title, string message, string priority)
        => _hub.Clients.All.SendAsync("SystemAnnouncement", new
        {
            Title = title,
            Message = message,
            Priority = priority,
            Timestamp = DateTime.UtcNow
        });

    public Task SendWbsTaskCreatedNotificationAsync(Guid notificationId, string wbsId, string taskName, Guid projectId, string userName)
        => _hub.Clients.Group(ProjectGroup(projectId)).SendAsync("WbsTaskCreated", new
        {
            NotificationId = notificationId,
            WbsId = wbsId,
            TaskName = taskName,
            ProjectId = projectId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });

    public Task SendWbsTaskUpdatedNotificationAsync(Guid notificationId, string wbsId, string taskName, Guid projectId, string userName)
        => _hub.Clients.Group(ProjectGroup(projectId)).SendAsync("WbsTaskUpdated", new
        {
            NotificationId = notificationId,
            WbsId = wbsId,
            TaskName = taskName,
            ProjectId = projectId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });

    public Task SendWbsTaskDeletedNotificationAsync(string wbsId, string taskName, Guid projectId, string userName)
        => _hub.Clients.Group(ProjectGroup(projectId)).SendAsync("WbsTaskDeleted", new
        {
            WbsId = wbsId,
            TaskName = taskName,
            ProjectId = projectId,
            UserName = userName,
            Timestamp = DateTime.UtcNow
        });

    // --------------------------------------------------- Persisted / DB ops --

    public async Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 50)
    {
        if (skip < 0) skip = 0;
        if (take is < 1 or > 200) take = 50;

        var entities = await _context.WorkRequestNotifications
            .Where(n => n.RecipientId == userId)
            .Include(n => n.WorkRequest)
            .OrderByDescending(n => n.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var notifications = entities.Select(ToDto).ToList();
        return ServiceResult<IEnumerable<NotificationDto>>.SuccessResult(notifications, "Notifications retrieved successfully");
    }

    public async Task<ServiceResult<bool>> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.WorkRequestNotifications
            .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientId == userId);
        if (notification == null)
            return ServiceResult<bool>.ErrorResult("Notification not found");

        if (notification.ReadAt == null)
        {
            notification.ReadAt = DateTime.UtcNow;
            notification.Status = NotificationStatus.Read;
            await _context.SaveChangesAsync();
            await SendNotificationCountUpdateAsync(userId);
        }

        return ServiceResult<bool>.SuccessResult(true, "Notification marked as read");
    }

    public async Task<ServiceResult<bool>> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        var unread = await _context.WorkRequestNotifications
            .Where(n => n.RecipientId == userId && n.ReadAt == null)
            .ToListAsync();

        if (unread.Count > 0)
        {
            var now = DateTime.UtcNow;
            foreach (var n in unread)
            {
                n.ReadAt = now;
                n.Status = NotificationStatus.Read;
            }
            await _context.SaveChangesAsync();
            await SendNotificationCountUpdateAsync(userId);
        }

        return ServiceResult<bool>.SuccessResult(true, "All notifications marked as read");
    }

    public async Task SendNotificationCountUpdateAsync(Guid userId)
    {
        var count = await _context.WorkRequestNotifications
            .CountAsync(n => n.RecipientId == userId && n.ReadAt == null);
        await _hub.Clients.Group(UserGroup(userId)).SendAsync("NotificationCountUpdated", count);
    }

    public async Task<ServiceResult<NotificationDto>> CreateWorkRequestNotificationAsync(
        Guid workRequestId, Guid recipientId, string type, string subject, string message, Guid? senderId = null)
    {
        if (recipientId == Guid.Empty)
            return ServiceResult<NotificationDto>.ErrorResult("Recipient is required");
        if (!Enum.TryParse<NotificationType>(type, true, out var notificationType))
            return ServiceResult<NotificationDto>.ErrorResult($"Invalid notification type '{type}'");

        var notification = new WorkRequestNotification
        {
            NotificationId = Guid.NewGuid(),
            WorkRequestId = workRequestId,
            RecipientId = recipientId,
            SenderId = senderId,
            Type = notificationType,
            Status = NotificationStatus.Sent,
            Subject = subject,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            SentAt = DateTime.UtcNow
        };

        _context.WorkRequestNotifications.Add(notification);
        await _context.SaveChangesAsync();

        var title = await _context.WorkRequests
            .Where(w => w.WorkRequestId == workRequestId)
            .Select(w => w.Title)
            .FirstOrDefaultAsync() ?? string.Empty;

        var dto = new NotificationDto
        {
            NotificationId = notification.NotificationId,
            WorkRequestId = notification.WorkRequestId,
            WorkRequestTitle = title,
            Type = notification.Type.ToString(),
            Status = notification.Status.ToString(),
            Subject = notification.Subject,
            Message = notification.Message,
            CreatedAt = notification.CreatedAt,
            SentAt = notification.SentAt,
            ReadAt = notification.ReadAt,
            IsRead = false
        };

        // Push the live notification then the refreshed unread count.
        await _hub.Clients.Group(UserGroup(recipientId)).SendAsync("WorkRequestNotification", dto);
        await SendNotificationCountUpdateAsync(recipientId);

        return ServiceResult<NotificationDto>.SuccessResult(dto, "Notification created");
    }

    // -------------------------------------------------------------- Helpers --

    private static string UserGroup(Guid userId) => $"user_{userId}";
    private static string ProjectGroup(Guid projectId) => $"project_{projectId}";

    private static object Payload(string message, string type) => new
    {
        Message = message,
        Type = type,
        Timestamp = DateTime.UtcNow
    };

    private static NotificationDto ToDto(WorkRequestNotification n) => new()
    {
        NotificationId = n.NotificationId,
        WorkRequestId = n.WorkRequestId,
        WorkRequestTitle = n.WorkRequest != null ? n.WorkRequest.Title : string.Empty,
        Type = n.Type.ToString(),
        Status = n.Status.ToString(),
        Subject = n.Subject,
        Message = n.Message,
        CreatedAt = n.CreatedAt,
        SentAt = n.SentAt,
        ReadAt = n.ReadAt,
        IsRead = n.ReadAt != null
    };
}
