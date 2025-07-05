# 🔧 Real-Time Integration Example

This file demonstrates how to add SignalR real-time broadcasting to any controller action.

## 📝 Example: Adding Real-Time to Project Creation

### Step 1: Inject Notification Service

```csharp
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly INotificationService _notificationService; // Add this

    public ProjectsController(
        IProjectService projectService,
        INotificationService notificationService) // Add this
    {
        _projectService = projectService;
        _notificationService = notificationService; // Add this
    }
```

### Step 2: Add Real-Time Broadcasting

```csharp
[HttpPost]
public async Task<ActionResult<ApiResponse<ProjectDto>>> CreateProject([FromBody] CreateProjectRequest request)
{
    // 1. Existing logic
    var result = await _projectService.CreateProjectAsync(request);
    
    if (result.IsSuccess)
    {
        // 2. Add real-time broadcast
        await _notificationService.SendProjectCreatedNotificationAsync(
            result.Data.ProjectId,
            result.Data.ProjectName,
            User.Identity?.Name ?? "Unknown"
        );

        // 3. Broadcast to project groups
        await _notificationService.BroadcastToAllUsersAsync("ProjectCreated", new
        {
            ProjectId = result.Data.ProjectId,
            ProjectName = result.Data.ProjectName,
            CreatedBy = User.Identity?.Name,
            Timestamp = DateTime.UtcNow,
            Data = result.Data
        });

        return StatusCode(201, CreateSuccessResponse(result.Data!, "Project created successfully"));
    }

    return BadRequest(CreateErrorResponse(result.Message));
}
```

### Step 3: Client-Side Reception

```javascript
// JavaScript client automatically receives updates
connection.on("ProjectCreated", (data) => {
    // Add new project to UI without page refresh
    addProjectToList(data.Data);
    
    // Show notification
    showNotification(`New project created: ${data.ProjectName}`, 'success');
    
    // Update counters
    updateProjectCount();
});
```

## 🎯 The Result

✅ **Instant Updates**: All connected users see new projects immediately  
✅ **No Page Refresh**: UI updates automatically  
✅ **Real-Time Collaboration**: Multiple users can work simultaneously  
✅ **Live Notifications**: Important events delivered instantly  

## 📊 Available Notification Methods

The `INotificationService` provides these broadcasting methods:

```csharp
// Send to specific user
await _notificationService.SendNotificationAsync(message, userId);

// Send to all users
await _notificationService.BroadcastToAllUsersAsync(eventName, data);

// Send to project team
await _notificationService.SendProjectCreatedNotificationAsync(projectId, name, creator);

// Send to specific group
await _notificationService.SendToGroupAsync("project_123", eventName, data);
```

## 🚀 Quick Implementation

1. **Add notification service** to controller constructor
2. **Add broadcast call** after successful operations
3. **Test with dashboard** to see real-time updates
4. **Monitor logs** for SignalR activity

That's it! Real-time updates are now working for that endpoint.
