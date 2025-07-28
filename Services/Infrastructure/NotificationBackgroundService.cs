using dotnet_rest_api.Models;
using dotnet_rest_api.DTOs;
using System.Collections.Concurrent;
using TaskModel = dotnet_rest_api.Models.Task;

namespace dotnet_rest_api.Services.Infrastructure;

/// <summary>
/// Queue item for background notification processing
/// </summary>
public class NotificationQueueItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public List<Guid> TargetUserIds { get; set; } = new();
    public string? TargetGroup { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int Priority { get; set; } = 0; // 0 = normal, 1 = high, 2 = urgent
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;
}

/// <summary>
/// Background service for processing notification queue
/// Handles bulk notifications, email sending, and database logging
/// </summary>
public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NotificationBackgroundService> _logger;
    private readonly ConcurrentQueue<NotificationQueueItem> _notificationQueue;
    private readonly SemaphoreSlim _semaphore;
    private const int MAX_CONCURRENT_PROCESSING = 5;

    public NotificationBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<NotificationBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _notificationQueue = new ConcurrentQueue<NotificationQueueItem>();
        _semaphore = new SemaphoreSlim(MAX_CONCURRENT_PROCESSING);
    }

    /// <summary>
    /// Queue a notification for background processing
    /// </summary>
    public void QueueNotification(NotificationQueueItem item)
    {
        _notificationQueue.Enqueue(item);
        _logger.LogInformation("Queued notification {NotificationId} of type {Type}", item.Id, item.Type);
    }

    /// <summary>
    /// Queue a simple notification
    /// </summary>
    public void QueueSimpleNotification(string message, List<Guid> targetUserIds, string type = "Info", int priority = 0)
    {
        var item = new NotificationQueueItem
        {
            Type = type,
            Message = message,
            TargetUserIds = targetUserIds,
            Priority = priority
        };
        QueueNotification(item);
    }

    /// <summary>
    /// Queue a group notification
    /// </summary>
    public void QueueGroupNotification(string message, string targetGroup, string type = "Info", int priority = 0)
    {
        var item = new NotificationQueueItem
        {
            Type = type,
            Message = message,
            TargetGroup = targetGroup,
            Priority = priority
        };
        QueueNotification(item);
    }

    /// <summary>
    /// Queue a work request notification
    /// </summary>
    public void QueueWorkRequestNotification(WorkRequest workRequest, string notificationType)
    {
        var item = new NotificationQueueItem
        {
            Type = notificationType,
            Message = $"Work Request: {workRequest.Title}",
            Data = new Dictionary<string, object>
            {
                { "WorkRequestId", workRequest.WorkRequestId },
                { "ProjectId", workRequest.ProjectId },
                { "Title", workRequest.Title },
                { "Status", workRequest.Status },
                { "Priority", workRequest.Priority }
            },
            TargetGroup = $"project_{workRequest.ProjectId}",
            Priority = workRequest.Priority switch
            {
                WorkRequestPriority.Critical => 2,
                WorkRequestPriority.High => 1,
                _ => 0
            }
        };
        QueueNotification(item);
    }

    /// <summary>
    /// Queue a daily report notification
    /// </summary>
    public void QueueDailyReportNotification(DailyReport dailyReport, string notificationType)
    {
        var item = new NotificationQueueItem
        {
            Type = notificationType,
            Message = $"Daily Report: {dailyReport.ReportDate:yyyy-MM-dd}",
            Data = new Dictionary<string, object>
            {
                { "DailyReportId", dailyReport.DailyReportId },
                { "ProjectId", dailyReport.ProjectId },
                { "Date", dailyReport.ReportDate },
                { "Status", dailyReport.Status },
                { "SubmittedBy", dailyReport.SubmittedByUserId }
            },
            TargetGroup = $"project_{dailyReport.ProjectId}",
            Priority = notificationType.Contains("Approval") ? 1 : 0
        };
        QueueNotification(item);
    }

    protected override async System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var tasks = new List<System.Threading.Tasks.Task>();

                // Process up to MAX_CONCURRENT_PROCESSING notifications
                for (int i = 0; i < MAX_CONCURRENT_PROCESSING && _notificationQueue.TryDequeue(out var item); i++)
                {
                    await _semaphore.WaitAsync(stoppingToken);
                    tasks.Add(ProcessNotificationAsync(item, stoppingToken));
                }

                if (tasks.Any())
                {
                    await System.Threading.Tasks.Task.WhenAll(tasks);
                }
                else
                {
                    // No notifications to process, wait before checking again
                    await System.Threading.Tasks.Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in notification background service main loop");
                await System.Threading.Tasks.Task.Delay(5000, stoppingToken); // Wait before retrying
            }
        }

        _logger.LogInformation("Notification Background Service stopped");
    }

    private async System.Threading.Tasks.Task ProcessNotificationAsync(NotificationQueueItem item, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var signalRService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            
            _logger.LogInformation("Processing notification {NotificationId} of type {Type}", item.Id, item.Type);

            // Send SignalR notification
            if (!string.IsNullOrEmpty(item.TargetGroup))
            {
                await SendGroupNotificationAsync(signalRService, item);
            }
            else if (item.TargetUserIds.Any())
            {
                await SendUserNotificationsAsync(signalRService, item);
            }

            // Store notification in database
            await StoreNotificationInDatabaseAsync(scope, item);

            // Send email notifications for high priority items
            if (item.Priority >= 1)
            {
                await SendEmailNotificationAsync(scope, item);
            }

            _logger.LogInformation("Successfully processed notification {NotificationId}", item.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing notification {NotificationId}", item.Id);
            
            // Retry logic
            if (item.RetryCount < item.MaxRetries)
            {
                item.RetryCount++;
                _logger.LogInformation("Retrying notification {NotificationId}, attempt {RetryCount}/{MaxRetries}", 
                    item.Id, item.RetryCount, item.MaxRetries);
                
                // Re-queue with delay
                _ = System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, item.RetryCount)), cancellationToken)
                    .ContinueWith(_ => QueueNotification(item), cancellationToken);
            }
            else
            {
                _logger.LogError("Failed to process notification {NotificationId} after {MaxRetries} attempts", 
                    item.Id, item.MaxRetries);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async System.Threading.Tasks.Task SendGroupNotificationAsync(INotificationService signalRService, NotificationQueueItem item)
    {
        if (signalRService is SignalRNotificationService signalRNotificationService)
        {
            switch (item.Type)
            {
                case "WorkRequestCreated":
                    if (item.Data.TryGetValue("WorkRequestId", out var wrId) && wrId is Guid workRequestId)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                        var workRequest = await context.WorkRequests.FindAsync(workRequestId);
                        if (workRequest != null)
                        {
                            await signalRNotificationService.SendWorkRequestNotificationAsync(
                                workRequest.WorkRequestId, 
                                NotificationType.WorkRequestSubmitted, 
                                workRequest.RequestedById, 
                                $"Work Request '{workRequest.Title}' has been created");
                        }
                    }
                    break;

                case "DailyReportCreated":
                    if (item.Data.TryGetValue("DailyReportId", out var drId) && drId is Guid dailyReportId)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
                        var dailyReport = await context.DailyReports.FindAsync(dailyReportId);
                        if (dailyReport != null)
                        {
                            await signalRNotificationService.SendDailyReportCreatedNotificationAsync(
                                dailyReport.DailyReportId, 
                                dailyReport.ProjectId, 
                                "Reporter");
                        }
                    }
                    break;

                case "ProjectProgressUpdate":
                    if (item.Data.TryGetValue("ProjectId", out var projId) && projId is Guid projectId &&
                        item.Data.TryGetValue("CompletionPercentage", out var completion) && completion is decimal completionPercentage)
                    {
                        var updatedBy = item.Data.TryGetValue("UpdatedBy", out var updater) && updater is string updatedByName ? updatedByName : "System";
                        await signalRNotificationService.SendRealTimeProgressUpdateAsync(projectId, completionPercentage, updatedBy);
                    }
                    break;

                default:
                    // Generic notification
                    await signalRService.SendNotificationAsync(item.Message, Guid.Empty);
                    break;
            }
        }
    }

    private async System.Threading.Tasks.Task SendUserNotificationsAsync(INotificationService signalRService, NotificationQueueItem item)
    {
        foreach (var userId in item.TargetUserIds)
        {
            await signalRService.SendNotificationAsync(item.Message, userId);
        }
    }

    private async System.Threading.Tasks.Task StoreNotificationInDatabaseAsync(IServiceScope scope, NotificationQueueItem item)
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            
            // Store notification records for each target user
            var notifications = new List<WorkRequestNotification>();
            
            if (item.TargetUserIds.Any())
            {
                foreach (var userId in item.TargetUserIds)
                {
                    // For WorkRequest notifications, we need a WorkRequestId
                    if (item.Data.TryGetValue("WorkRequestId", out var wrId) && wrId is Guid workRequestId)
                    {
                        notifications.Add(new WorkRequestNotification
                        {
                            NotificationId = Guid.NewGuid(),
                            WorkRequestId = workRequestId,
                            RecipientId = userId,
                            Type = NotificationType.WorkRequestSubmitted, // Default type
                            Status = NotificationStatus.Pending,
                            Subject = GetNotificationTitle(item.Type),
                            Message = item.Message,
                            CreatedAt = item.CreatedAt
                        });
                    }
                }
            }
            else if (!string.IsNullOrEmpty(item.TargetGroup))
            {
                // For group notifications, skip database storage for now
                // as WorkRequestNotification requires a WorkRequestId
            }

            if (notifications.Any())
            {
                context.WorkRequestNotifications.AddRange(notifications);
                await context.SaveChangesAsync();
                
                _logger.LogInformation("Stored {Count} notification records in database for notification {NotificationId}", 
                    notifications.Count, item.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing notification {NotificationId} in database", item.Id);
            throw; // Re-throw to trigger retry logic
        }
    }

    private async System.Threading.Tasks.Task SendEmailNotificationAsync(IServiceScope scope, NotificationQueueItem item)
    {
        try
        {
            // TODO: Integrate with email service when IEmailService is implemented
            _logger.LogInformation("Email notification would be sent for high-priority item {NotificationId}: {Message}", 
                item.Id, item.Message);
            
            await System.Threading.Tasks.Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email notification for {NotificationId}", item.Id);
            // Don't re-throw for email errors - they shouldn't fail the entire notification
        }
    }

    private static string GetNotificationTitle(string type)
    {
        return type switch
        {
            "WorkRequestCreated" => "New Work Request",
            "WorkRequestStatusChanged" => "Work Request Updated",
            "DailyReportCreated" => "New Daily Report",
            "DailyReportApprovalStatusChanged" => "Daily Report Status Changed",
            "ProjectProgressUpdate" => "Project Progress Updated",
            "SystemAnnouncement" => "System Announcement",
            _ => "Notification"
        };
    }

    public override async System.Threading.Tasks.Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Notification Background Service is stopping...");
        
        // Process any remaining items in the queue before stopping
        var remainingItems = new List<NotificationQueueItem>();
        while (_notificationQueue.TryDequeue(out var item))
        {
            remainingItems.Add(item);
        }

        if (remainingItems.Any())
        {
            _logger.LogInformation("Processing {Count} remaining notifications before shutdown", remainingItems.Count);
            
            var tasks = remainingItems.Select(item => ProcessNotificationAsync(item, cancellationToken));
            await System.Threading.Tasks.Task.WhenAll(tasks);
        }

        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// Extensions for integrating NotificationBackgroundService
/// </summary>
public static class NotificationBackgroundServiceExtensions
{
    public static IServiceCollection AddNotificationBackgroundService(this IServiceCollection services)
    {
        services.AddHostedService<NotificationBackgroundService>();
        services.AddSingleton<NotificationBackgroundService>();
        return services;
    }
}
