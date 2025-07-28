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
}
