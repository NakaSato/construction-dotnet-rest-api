using dotnet_rest_api.DTOs;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Stub implementation for INotificationService to get a working build
/// </summary>
public class StubNotificationService : INotificationService
{
    public Task SendNotificationAsync(Guid userId, string message, string type = "info")
    {
        // Stub implementation - does nothing
        return Task.CompletedTask;
    }

    public Task SendNotificationToAllAsync(string message, string type = "info")
    {
        // Stub implementation - does nothing
        return Task.CompletedTask;
    }

    public Task SendNotificationToGroupAsync(IEnumerable<Guid> userIds, string message, string type = "info")
    {
        // Stub implementation - does nothing
        return Task.CompletedTask;
    }

    public Task SendProjectNotificationAsync(Guid projectId, string message, string type = "info")
    {
        // Stub implementation - does nothing
        return Task.CompletedTask;
    }

    public Task SendTaskNotificationAsync(Guid taskId, string message, string type = "info")
    {
        // Stub implementation - does nothing
        return Task.CompletedTask;
    }

    public Task<ServiceResult<IEnumerable<NotificationDto>>> GetUserNotificationsAsync(Guid userId, int skip = 0, int take = 50)
    {
        var result = new List<NotificationDto>();
        return Task.FromResult(ServiceResult<IEnumerable<NotificationDto>>.SuccessResult(result, "Stub implementation"));
    }

    public Task<ServiceResult<bool>> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true, "Stub implementation"));
    }

    public Task<ServiceResult<bool>> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        return Task.FromResult(ServiceResult<bool>.SuccessResult(true, "Stub implementation"));
    }
}
