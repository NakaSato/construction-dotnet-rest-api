using dotnet_rest_api.Data;
using dotnet_rest_api.DTOs;
using dotnet_rest_api.Models;
using Microsoft.EntityFrameworkCore;

namespace dotnet_rest_api.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ApiResponse<bool>> SendWorkRequestNotificationAsync(
        Guid workRequestId,
        Guid recipientId,
        NotificationType type,
        string? customMessage = null,
        Guid? senderId = null)
    {
        try
        {
            var workRequest = await _context.WorkRequests
                .Include(wr => wr.Project)
                .Include(wr => wr.RequestedBy)
                .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

            if (workRequest == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Work request not found"
                };
            }

            var recipient = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == recipientId);

            if (recipient == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Recipient not found"
                };
            }

            var (subject, message) = GenerateNotificationContent(workRequest, type, customMessage);

            var notification = new WorkRequestNotification
            {
                NotificationId = Guid.NewGuid(),
                WorkRequestId = workRequestId,
                RecipientId = recipientId,
                SenderId = senderId,
                Type = type,
                Subject = subject,
                Message = message,
                EmailTo = recipient.Email,
                CreatedAt = DateTime.UtcNow
            };

            _context.WorkRequestNotifications.Add(notification);
            await _context.SaveChangesAsync();

            // Send email asynchronously
            _ = Task.Run(async () => await SendEmailNotificationAsync(notification.NotificationId));

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Notification queued successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending work request notification");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to send notification"
            };
        }
    }

    public async Task<ApiResponse<bool>> SendBulkNotificationsAsync(
        List<Guid> workRequestIds,
        List<Guid> recipientIds,
        NotificationType type,
        string? customMessage = null,
        Guid? senderId = null)
    {
        try
        {
            var notifications = new List<WorkRequestNotification>();

            foreach (var workRequestId in workRequestIds)
            {
                foreach (var recipientId in recipientIds)
                {
                    var workRequest = await _context.WorkRequests
                        .Include(wr => wr.Project)
                        .Include(wr => wr.RequestedBy)
                        .FirstOrDefaultAsync(wr => wr.WorkRequestId == workRequestId);

                    if (workRequest == null) continue;

                    var recipient = await _context.Users
                        .FirstOrDefaultAsync(u => u.UserId == recipientId);

                    if (recipient == null) continue;

                    var (subject, message) = GenerateNotificationContent(workRequest, type, customMessage);

                    notifications.Add(new WorkRequestNotification
                    {
                        NotificationId = Guid.NewGuid(),
                        WorkRequestId = workRequestId,
                        RecipientId = recipientId,
                        SenderId = senderId,
                        Type = type,
                        Subject = subject,
                        Message = message,
                        EmailTo = recipient.Email,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            _context.WorkRequestNotifications.AddRange(notifications);
            await _context.SaveChangesAsync();

            // Send emails asynchronously
            foreach (var notification in notifications)
            {
                _ = Task.Run(async () => await SendEmailNotificationAsync(notification.NotificationId));
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Queued {notifications.Count} notifications successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending bulk notifications");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to send bulk notifications"
            };
        }
    }

    public async Task<ApiResponse<PagedResult<NotificationDto>>> GetUserNotificationsAsync(
        Guid userId, 
        int pageNumber = 1, 
        int pageSize = 20,
        bool unreadOnly = false)
    {
        try
        {
            var query = _context.WorkRequestNotifications
                .Include(n => n.WorkRequest)
                .Where(n => n.RecipientId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => n.ReadAt == null);
            }

            var totalCount = await query.CountAsync();
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
                    IsRead = n.ReadAt.HasValue
                })
                .ToListAsync();

            var result = new PagedResult<NotificationDto>
            {
                Items = notifications,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            return new ApiResponse<PagedResult<NotificationDto>>
            {
                Success = true,
                Data = result
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user notifications");
            return new ApiResponse<PagedResult<NotificationDto>>
            {
                Success = false,
                Message = "Failed to get notifications"
            };
        }
    }

    public async Task<ApiResponse<bool>> MarkNotificationAsReadAsync(Guid notificationId, Guid userId)
    {
        try
        {
            var notification = await _context.WorkRequestNotifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && n.RecipientId == userId);

            if (notification == null)
            {
                return new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Notification not found"
                };
            }

            if (notification.ReadAt == null)
            {
                notification.ReadAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = "Notification marked as read"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking notification as read");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to mark notification as read"
            };
        }
    }

    public async Task<ApiResponse<bool>> MarkAllNotificationsAsReadAsync(Guid userId)
    {
        try
        {
            var unreadNotifications = await _context.WorkRequestNotifications
                .Where(n => n.RecipientId == userId && n.ReadAt == null)
                .ToListAsync();

            var readTime = DateTime.UtcNow;
            foreach (var notification in unreadNotifications)
            {
                notification.ReadAt = readTime;
            }

            await _context.SaveChangesAsync();

            return new ApiResponse<bool>
            {
                Success = true,
                Data = true,
                Message = $"Marked {unreadNotifications.Count} notifications as read"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking all notifications as read");
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to mark notifications as read"
            };
        }
    }

    private async Task SendEmailNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _context.WorkRequestNotifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);

            if (notification == null || notification.Status != NotificationStatus.Pending)
                return;

            var emailResult = await _emailService.SendEmailAsync(
                notification.EmailTo!,
                notification.Subject,
                notification.Message,
                notification.EmailCc);

            notification.Status = emailResult.Success ? NotificationStatus.Sent : NotificationStatus.Failed;
            notification.SentAt = emailResult.Success ? DateTime.UtcNow : null;
            notification.ErrorMessage = emailResult.Success ? null : emailResult.Message;

            if (!emailResult.Success)
            {
                notification.RetryCount++;
                if (notification.RetryCount < 3)
                {
                    notification.NextRetryAt = DateTime.UtcNow.AddMinutes(Math.Pow(2, notification.RetryCount) * 15);
                }
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email notification {notificationId}");
            
            var notification = await _context.WorkRequestNotifications
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId);
            
            if (notification != null)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;
                notification.RetryCount++;
                await _context.SaveChangesAsync();
            }
        }
    }

    private (string subject, string message) GenerateNotificationContent(
        WorkRequest workRequest,
        NotificationType type,
        string? customMessage = null)
    {
        var subject = type switch
        {
            NotificationType.WorkRequestSubmitted => $"Work Request Submitted: {workRequest.Title}",
            NotificationType.WorkRequestApproved => $"Work Request Approved: {workRequest.Title}",
            NotificationType.WorkRequestRejected => $"Work Request Rejected: {workRequest.Title}",
            NotificationType.WorkRequestAssigned => $"Work Request Assigned: {workRequest.Title}",
            NotificationType.WorkRequestCompleted => $"Work Request Completed: {workRequest.Title}",
            NotificationType.WorkRequestEscalated => $"Work Request Escalated: {workRequest.Title}",
            NotificationType.ApprovalRequired => $"Approval Required: {workRequest.Title}",
            NotificationType.ApprovalReminder => $"Approval Reminder: {workRequest.Title}",
            NotificationType.WorkRequestDue => $"Work Request Due: {workRequest.Title}",
            NotificationType.WorkRequestOverdue => $"Work Request Overdue: {workRequest.Title}",
            _ => $"Work Request Update: {workRequest.Title}"
        };

        var message = customMessage ?? type switch
        {
            NotificationType.WorkRequestSubmitted => 
                $"A new work request has been submitted for project {workRequest.Project.ProjectName}.\n\n" +
                $"Description: {workRequest.Description}\n" +
                $"Priority: {workRequest.Priority}\n" +
                $"Requested by: {workRequest.RequestedBy.FullName}",
            
            NotificationType.ApprovalRequired => 
                $"A work request requires your approval.\n\n" +
                $"Project: {workRequest.Project.ProjectName}\n" +
                $"Description: {workRequest.Description}\n" +
                $"Priority: {workRequest.Priority}\n" +
                $"Estimated Cost: {workRequest.EstimatedCost:C}",
            
            NotificationType.WorkRequestApproved => 
                $"Your work request has been approved and is ready to proceed.\n\n" +
                $"Project: {workRequest.Project.ProjectName}\n" +
                $"You can now start working on this request.",
            
            NotificationType.WorkRequestRejected => 
                $"Your work request has been rejected.\n\n" +
                $"Project: {workRequest.Project.ProjectName}\n" +
                $"Please review the comments and resubmit if necessary.",
            
            _ => $"There has been an update to work request: {workRequest.Title}"
        };

        return (subject, message);
    }
}
