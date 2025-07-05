# 🔧 SignalR Real-Time Configuration Guide

**📅 Version**: 2.0  
**🔄 Last Updated**: January 15, 2025  
**✅ Status**: Production Ready  

## 🚀 Complete SignalR Setup

This guide documents the complete SignalR configuration for real-time live updates in the Solar Project Management REST API.

## 📋 Prerequisites

- .NET 9.0
- ASP.NET Core SignalR
- JWT Authentication
- Entity Framework Core
- PostgreSQL Database

## 🔧 Backend Configuration

### 1. Program.cs Configuration

The following SignalR configuration is properly set up in `Program.cs`:

```csharp
// SignalR Configuration for Real-Time Updates
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});
```

### 2. JWT Authentication for SignalR

SignalR is configured to work with JWT authentication:

```csharp
// Configure SignalR token authentication
options.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        
        // If the request is for SignalR hub
        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/notificationHub"))
        {
            context.Token = accessToken;
        }
        
        return Task.CompletedTask;
    }
};
```

### 3. CORS Configuration

CORS is configured to support SignalR connections:

```csharp
// Specific policy for SignalR with credentials
options.AddPolicy("SignalRPolicy", policy =>
{
    policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "https://your-frontend-domain.com")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials();
});
```

### 4. Hub Mapping

The NotificationHub is mapped to the `/notificationHub` endpoint:

```csharp
// SignalR Hub Configuration for Real-Time Updates
app.MapHub<dotnet_rest_api.Hubs.NotificationHub>("/notificationHub");
```

### 5. Service Registration

Key services are registered for dependency injection:

```csharp
// Notification Services
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
builder.Services.AddHostedService<NotificationBackgroundService>();
```

## 🏗️ Architecture Components

### 1. NotificationHub (`/Hubs/NotificationHub.cs`)

Central SignalR hub handling:
- User connection management
- Group subscriptions (projects, regions, facilities)
- Real-time event broadcasting
- Authentication and authorization

**Key Features**:
- ✅ Project-specific groups
- ✅ Regional groups (Northern, Western, Central Thailand)
- ✅ Facility-type groups (water treatment, solar installation)
- ✅ Map viewer groups for location updates
- ✅ Role-based access control

### 2. SignalRNotificationService (`/Services/SignalRNotificationService.cs`)

Service layer handling:
- Real-time event creation and broadcasting
- Geographic region detection
- User context and permission management
- Dashboard statistics updates

**Key Methods**:
- `SendProjectUpdateAsync()` - Project CRUD notifications
- `SendProjectStatusUpdateAsync()` - Status change notifications
- `SendLocationUpdateAsync()` - GPS/address updates
- `SendDashboardStatsAsync()` - Live dashboard statistics
- `SendRegionalUpdateAsync()` - Region-specific updates

### 3. Enhanced Service Integration

**ProjectService Integration**:
```csharp
// Real-time notifications integrated into CRUD operations
public async Task<ServiceResult<Project>> CreateProjectAsync(CreateProjectRequest request, string? userId = null, string? userName = null)
{
    // ... project creation logic ...
    
    // Send real-time notification
    await _notificationService.SendProjectUpdateAsync(
        project, "created", userId, userName);
    
    return ServiceResult<Project>.Success(project);
}
```

**Controller Integration**:
```csharp
// Pass user context from controller to service
var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
var userName = HttpContext.User.FindFirst(ClaimTypes.Name)?.Value;

var result = await _projectService.CreateProjectAsync(request, userId, userName);
```

## 🌐 Client-Side Implementation

### 1. JavaScript/TypeScript Connection

```javascript
// SignalR connection setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => localStorage.getItem("jwtToken")
    })
    .withAutomaticReconnect([0, 2000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Start connection
await connection.start();
console.log("SignalR connected for real-time updates");
```

### 2. Event Handlers

```javascript
// Project update events
connection.on("ProjectCreated", (projectData) => {
    addProjectToUI(projectData);
    showNotification(`New project created: ${projectData.projectName}`);
});

connection.on("ProjectUpdated", (projectData) => {
    updateProjectInUI(projectData);
    showNotification(`Project updated: ${projectData.projectName}`);
});

connection.on("ProjectStatusChanged", (statusData) => {
    updateProjectStatus(statusData.projectId, statusData.newStatus);
    showNotification(`Status changed to: ${statusData.newStatus}`);
});

// Location and geographic updates
connection.on("LocationUpdated", (locationData) => {
    updateMapMarker(locationData.projectId, locationData.coordinates);
    updateProjectAddress(locationData.projectId, locationData.address);
});

// Dashboard statistics
connection.on("ProjectStatsUpdated", (statsData) => {
    updateDashboardMetrics(statsData);
    refreshCharts(statsData.regional);
});
```

### 3. Group Management

```javascript
// Join project-specific updates
await connection.invoke("JoinProjectGroup", projectId);

// Join regional updates (automatically based on user location)
await connection.invoke("JoinRegionalGroup", "northern");

// Join facility updates
await connection.invoke("JoinFacilityGroup", "water_treatment");

// Join map viewer updates
await connection.invoke("JoinMapViewerGroup");
```

## 🗺️ Geographic Features

### 1. Thailand Region Detection

Projects are automatically categorized by Thai provinces:

```csharp
// Automatic region detection
private string DetermineRegion(string? address, decimal? latitude, decimal? longitude)
{
    if (string.IsNullOrEmpty(address))
        return "unknown";

    var lowerAddress = address.ToLower();
    
    // Northern Thailand
    if (lowerAddress.Contains("เชียงใหม่") || lowerAddress.Contains("เชียงราย") ||
        lowerAddress.Contains("ลำปาง") || lowerAddress.Contains("ลำพูน") ||
        lowerAddress.Contains("พะเยา") || lowerAddress.Contains("แพร่") ||
        lowerAddress.Contains("น่าน"))
        return "northern";
    
    // Western Thailand  
    if (lowerAddress.Contains("ตาก"))
        return "western";
    
    // Central Thailand
    if (lowerAddress.Contains("กรุงเทพ") || lowerAddress.Contains("พิจิตร") ||
        lowerAddress.Contains("พิษณุโลก") || lowerAddress.Contains("สุโขทัย") ||
        lowerAddress.Contains("อุตรดิตถ์"))
        return "central";
    
    return "unknown";
}
```

### 2. Regional Broadcasting

```csharp
// Broadcast to regional groups
await _hubContext.Clients.Group($"region_{region}")
    .SendAsync("RegionalProjectUpdate", new
    {
        Region = region,
        Projects = regionalProjects,
        Timestamp = DateTime.UtcNow
    });
```

## 🔐 Security Configuration

### 1. Authentication Requirements

- All SignalR connections require valid JWT authentication
- Hub methods are protected with `[Authorize]` attribute
- User context is validated on all operations

### 2. Authorization Groups

```csharp
// Role-based group access
public async Task JoinRoleGroup(string roleName)
{
    var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
    
    if (userRole != null && userRole.Equals(roleName, StringComparison.OrdinalIgnoreCase))
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"role_{roleName}");
    }
}
```

## 📊 Monitoring and Logging

### 1. Connection Logging

```csharp
public override async Task OnConnectedAsync()
{
    var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userName = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
    
    _logger.LogInformation("User {UserId} ({UserName}) connected to SignalR hub", userId, userName);
    
    await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
    await base.OnConnectedAsync();
}
```

### 2. Performance Monitoring

- Connection count tracking
- Message delivery metrics
- Group subscription analytics
- Error rate monitoring

## 🚀 Production Deployment

### 1. Scale-Out Configuration

For production environments with multiple server instances:

```csharp
// Redis backplane for scale-out
builder.Services.AddSignalR().AddStackExchangeRedis("your-redis-connection-string");
```

### 2. Production Settings

```csharp
// Production SignalR configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = false; // Security: Disable in production
    options.KeepAliveInterval = TimeSpan.FromSeconds(30);
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(1);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
    options.MaximumReceiveMessageSize = 32 * 1024; // 32KB limit
});
```

## 🧪 Testing

### 1. Real-Time Testing Dashboard

Use the provided test dashboard at `/docs/testing/real-time-dashboard.html`:

- Interactive SignalR connection testing
- Event simulation and monitoring
- Group management testing
- Performance metrics visualization

### 2. Automated Testing

```csharp
// Unit test example for SignalR notifications
[Test]
public async Task CreateProject_ShouldSendRealTimeNotification()
{
    // Arrange
    var mockHub = new Mock<IHubContext<NotificationHub>>();
    var service = new SignalRNotificationService(mockHub.Object, _context, _logger);
    
    // Act
    await service.SendProjectUpdateAsync(project, "created", "user123", "John Doe");
    
    // Assert
    mockHub.Verify(h => h.Clients.All.SendAsync("ProjectCreated", It.IsAny<object>(), default), Times.Once);
}
```

## ✅ Verification Checklist

- [ ] SignalR service registered in Program.cs
- [ ] NotificationHub mapped to `/notificationHub`
- [ ] JWT authentication configured for SignalR
- [ ] CORS policy includes SignalR support
- [ ] NotificationService implemented and registered
- [ ] Project service integration completed
- [ ] Controller user context passing implemented
- [ ] Client-side connection established
- [ ] Event handlers implemented
- [ ] Group management working
- [ ] Geographic features functional
- [ ] Security measures in place
- [ ] Testing dashboard operational
- [ ] Production configuration ready

## 📚 Additional Resources

- [Official ASP.NET Core SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [SignalR Client API Reference](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client)
- [Real-Time Feature Testing Guide](/docs/testing/REAL_TIME_FEATURE_TESTING.md)
- [Interactive Testing Dashboard](/docs/testing/real-time-dashboard.html)
- [API Documentation](/docs/api/00_REAL_TIME_LIVE_UPDATES.md)

---

**🔥 Real-time updates are now fully operational!** All project, location, and status changes are broadcast instantly to connected users with advanced geographic and facility-based filtering.
