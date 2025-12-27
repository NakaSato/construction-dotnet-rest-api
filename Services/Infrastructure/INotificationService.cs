using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Interface for notification services
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Send notification to specific user
    /// </summary>
    Task SendNotificationAsync(Guid userId, string message, string type = "info");
    
    /// <summary>
    /// Send notification to specific user (alternate signature)
    /// </summary>
    Task SendNotificationAsync(string message, Guid userId);
    
    /// <summary>
    /// Send notification to all users
    /// </summary>
    Task SendNotificationToAllAsync(string message, string type = "info");
    
    /// <summary>
    /// Send notification to specific group of users
    /// </summary>
    Task SendNotificationToGroupAsync(IEnumerable<Guid> userIds, string message, string type = "info");
    
    /// <summary>
    /// Send project-related notification
    /// </summary>
    Task SendProjectNotificationAsync(Guid projectId, string message, string type = "info");
    
    /// <summary>
    /// Send task-related notification
    /// </summary>
    Task SendTaskNotificationAsync(Guid taskId, string message, string type = "info");
    
    /// <summary>
    /// Get notifications for user
    /// </summary>
    Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 50);
    
    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<ServiceResult<bool>> MarkNotificationAsReadAsync(Guid notificationId, Guid userId);
    
    /// <summary>
    /// Mark all notifications as read for user
    /// </summary>
    Task<ServiceResult<bool>> MarkAllNotificationsAsReadAsync(Guid userId);
    
    /// <summary>
    /// Send notification count update to user
    /// </summary>
    Task SendNotificationCountUpdateAsync(Guid userId);
    
    /// <summary>
    /// Send system announcement to all users
    /// </summary>
    Task SendSystemAnnouncementAsync(string title, string message, string priority);

    /// <summary>
    /// Send WBS task created notification
    /// </summary>
    Task SendWbsTaskCreatedNotificationAsync(Guid notificationId, string wbsId, string taskName, Guid projectId, string userName);

    /// <summary>
    /// Send WBS task updated notification
    /// </summary>
    Task SendWbsTaskUpdatedNotificationAsync(Guid notificationId, string wbsId, string taskName, Guid projectId, string userName);

    /// <summary>
    /// Send WBS task deleted notification
    /// </summary>
    Task SendWbsTaskDeletedNotificationAsync(string wbsId, string taskName, Guid projectId, string userName);
}
