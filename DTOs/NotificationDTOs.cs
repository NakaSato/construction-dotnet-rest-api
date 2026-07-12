using System.ComponentModel.DataAnnotations;
using dotnet_rest_api.Common;

namespace dotnet_rest_api.DTOs;

// Notification DTOs
public class NotificationDto
{
    public Guid NotificationId { get; set; }
    public Guid WorkRequestId { get; set; }
    public string WorkRequestTitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsRead { get; set; }
}


