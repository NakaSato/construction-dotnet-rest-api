using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace dotnet_rest_api.Hubs;

/// <summary>
/// SignalR Hub for real-time notifications and updates
/// Provides live updates for work requests, daily reports, and project status changes
/// Enhanced with real-time daily report collaboration features
/// </summary>
[Authorize]
public class NotificationHub : Hub
{
    private readonly ILogger<NotificationHub> _logger;

    public NotificationHub(ILogger<NotificationHub> logger)
    {
        _logger = logger;
    }

    #region Project Management

    /// <summary>
    /// Join a project-specific group for receiving project updates
    /// </summary>
    /// <param name="projectId">Project ID to subscribe to</param>
    public async Task JoinProjectGroup(string projectId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) joining project group {ProjectId}", userId, userName, projectId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("JoinedProjectGroup", projectId);
        
        // Notify other project members
        await Clients.OthersInGroup($"project_{projectId}").SendAsync("UserJoinedProject", new
        {
            UserId = userId,
            UserName = userName,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave a project-specific group
    /// </summary>
    /// <param name="projectId">Project ID to unsubscribe from</param>
    public async Task LeaveProjectGroup(string projectId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) leaving project group {ProjectId}", userId, userName, projectId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"project_{projectId}");
        await Clients.Caller.SendAsync("LeftProjectGroup", projectId);
        
        // Notify other project members
        await Clients.OthersInGroup($"project_{projectId}").SendAsync("UserLeftProject", new
        {
            UserId = userId,
            UserName = userName,
            ProjectId = projectId,
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    #region Daily Reports Real-time Features

    /// <summary>
    /// Join a daily report editing session for real-time collaboration
    /// </summary>
    /// <param name="reportId">Daily report ID</param>
    public async Task JoinDailyReportSession(string reportId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} joining daily report session {ReportId}", userId, reportId);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"report_session_{reportId}");
        await Clients.Caller.SendAsync("JoinedReportSession", reportId);
        
        // Notify others in the session
        await Clients.OthersInGroup($"report_session_{reportId}").SendAsync("UserJoinedReportSession", new
        {
            UserId = userId,
            UserName = userName,
            ReportId = reportId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave a daily report editing session
    /// </summary>
    /// <param name="reportId">Daily report ID</param>
    public async Task LeaveDailyReportSession(string reportId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"report_session_{reportId}");
        await Clients.Caller.SendAsync("LeftReportSession", reportId);
        
        await Clients.OthersInGroup($"report_session_{reportId}").SendAsync("UserLeftReportSession", new
        {
            UserId = userId,
            UserName = userName,
            ReportId = reportId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Broadcast real-time daily report field changes to collaborators
    /// </summary>
    /// <param name="reportId">Daily report ID</param>
    /// <param name="fieldName">Field being edited</param>
    /// <param name="value">New field value</param>
    public async Task UpdateReportField(string reportId, string fieldName, object value)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        await Clients.OthersInGroup($"report_session_{reportId}").SendAsync("ReportFieldUpdated", new
        {
            ReportId = reportId,
            FieldName = fieldName,
            Value = value,
            UpdatedBy = userId,
            UpdatedByName = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send typing indicator for real-time collaboration
    /// </summary>
    /// <param name="reportId">Daily report ID</param>
    /// <param name="fieldName">Field being typed in</param>
    /// <param name="isTyping">Whether user is typing</param>
    public async Task SendTypingIndicator(string reportId, string fieldName, bool isTyping)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        await Clients.OthersInGroup($"report_session_{reportId}").SendAsync("UserTyping", new
        {
            ReportId = reportId,
            FieldName = fieldName,
            UserId = userId,
            UserName = userName,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    #region User and Role Management

    #endregion

    #region User and Role Management

    /// <summary>
    /// Join user-specific notification group
    /// </summary>
    public async Task JoinUserGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            _logger.LogInformation("User {UserId} joining personal notification group", userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            await Clients.Caller.SendAsync("JoinedUserGroup", userId);
        }
    }

    /// <summary>
    /// Join role-based notification group (for managers, admins, etc.)
    /// </summary>
    public async Task JoinRoleGroup()
    {
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        if (!string.IsNullOrEmpty(userRole))
        {
            _logger.LogInformation("User joining role group {Role}", userRole);
            await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{userRole.ToLower()}");
            await Clients.Caller.SendAsync("JoinedRoleGroup", userRole);
        }
    }

    #endregion

    #region Notifications

    /// <summary>
    /// Mark notification as read
    /// </summary>
    /// <param name="notificationId">Notification ID</param>
    public async Task MarkNotificationRead(string notificationId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} marking notification {NotificationId} as read", userId, notificationId);
        
        // This would typically update the database
        await Clients.Caller.SendAsync("NotificationMarkedRead", notificationId);
    }

    /// <summary>
    /// Request current notification count
    /// </summary>
    public async Task GetNotificationCount()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // This would typically query the database for unread notifications
        var count = 0; // Placeholder
        
        await Clients.Caller.SendAsync("NotificationCountUpdated", count);
    }

    /// <summary>
    /// Send real-time message to project team
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="message">Message content</param>
    public async Task SendProjectMessage(string projectId, string message)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        await Clients.Group($"project_{projectId}").SendAsync("ProjectMessage", new
        {
            ProjectId = projectId,
            Message = message,
            SenderId = userId,
            SenderName = userName,
            Timestamp = DateTime.UtcNow
        });
    }

    #endregion

    #region Connection Management

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) with role {Role} connected to NotificationHub", 
            userId, userName, userRole);
        
        // Automatically join user's personal group
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
        
        // Automatically join role group
        await JoinRoleGroup();
        
        // Send connection confirmation
        await Clients.Caller.SendAsync("Connected", new
        {
            UserId = userId,
            UserName = userName,
            Role = userRole,
            ConnectionId = Context.ConnectionId,
            Timestamp = DateTime.UtcNow
        });
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from NotificationHub", userId);
        
        if (exception != null)
        {
            _logger.LogWarning(exception, "User {UserId} disconnected with exception", userId);
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    #endregion

    #region Enhanced Group Management (July 2025)

    /// <summary>
    /// Join a geographic region group for receiving regional project updates
    /// </summary>
    /// <param name="region">Region name (northern, western, central)</param>
    public async Task JoinRegionGroup(string region)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) joining region group {Region}", userId, userName, region);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"region_{region}");
        await Clients.Caller.SendAsync("JoinedRegionGroup", region);
    }

    /// <summary>
    /// Leave a geographic region group
    /// </summary>
    /// <param name="region">Region name to leave</param>
    public async Task LeaveRegionGroup(string region)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} leaving region group {Region}", userId, region);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"region_{region}");
        await Clients.Caller.SendAsync("LeftRegionGroup", region);
    }

    /// <summary>
    /// Join a facility type group for receiving facility-specific updates
    /// </summary>
    /// <param name="facilityType">Facility type (water_treatment, solar_installation, etc.)</param>
    public async Task JoinFacilityGroup(string facilityType)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) joining facility group {FacilityType}", userId, userName, facilityType);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, $"facility_{facilityType}");
        await Clients.Caller.SendAsync("JoinedFacilityGroup", facilityType);
    }

    /// <summary>
    /// Leave a facility type group
    /// </summary>
    /// <param name="facilityType">Facility type to leave</param>
    public async Task LeaveFacilityGroup(string facilityType)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} leaving facility group {FacilityType}", userId, facilityType);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"facility_{facilityType}");
        await Clients.Caller.SendAsync("LeftFacilityGroup", facilityType);
    }

    /// <summary>
    /// Join the map viewers group for receiving location updates
    /// </summary>
    public async Task JoinMapViewersGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        _logger.LogInformation("User {UserId} ({UserName}) joining map viewers group", userId, userName);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, "map_viewers");
        await Clients.Caller.SendAsync("JoinedMapViewersGroup");
    }

    /// <summary>
    /// Leave the map viewers group
    /// </summary>
    public async Task LeaveMapViewersGroup()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        _logger.LogInformation("User {UserId} leaving map viewers group", userId);
        
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "map_viewers");
        await Clients.Caller.SendAsync("LeftMapViewersGroup");
    }

    /// <summary>
    /// Send real-time location data to connected map viewers
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="latitude">GPS latitude</param>
    /// <param name="longitude">GPS longitude</param>
    /// <param name="address">Project address</param>
    public async Task UpdateProjectLocation(string projectId, decimal latitude, decimal longitude, string address)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        var locationData = new
        {
            ProjectId = projectId,
            Coordinates = new { Latitude = latitude, Longitude = longitude },
            Address = address,
            UpdatedBy = userName,
            Timestamp = DateTime.UtcNow
        };

        // Send to map viewers
        await Clients.Group("map_viewers").SendAsync("ProjectLocationUpdated", locationData);
        
        // Send to project group
        await Clients.Group($"project_{projectId}").SendAsync("LocationUpdated", locationData);
        
        _logger.LogInformation("User {UserId} updated location for project {ProjectId}", userId, projectId);
    }

    /// <summary>
    /// Broadcast project status change to relevant groups
    /// </summary>
    /// <param name="projectId">Project ID</param>
    /// <param name="newStatus">New project status</param>
    /// <param name="completionPercentage">Completion percentage</param>
    public async Task UpdateProjectStatus(string projectId, string newStatus, decimal? completionPercentage = null)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        
        var statusData = new
        {
            ProjectId = projectId,
            NewStatus = newStatus,
            CompletionPercentage = completionPercentage,
            UpdatedBy = userName,
            Timestamp = DateTime.UtcNow
        };

        // Send to project group
        await Clients.Group($"project_{projectId}").SendAsync("ProjectStatusChanged", statusData);
        
        // Send to managers and admins
        await Clients.Groups(new[] { "role_manager", "role_administrator" }).SendAsync("ProjectStatusChanged", statusData);
        
        _logger.LogInformation("User {UserId} updated status for project {ProjectId} to {NewStatus}", userId, projectId, newStatus);
    }

    #endregion
}
