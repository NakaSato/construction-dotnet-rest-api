# Real-Time Notifications with SignalR

This document provides comprehensive information about the real-time notification system implemented using ASP.NET Core SignalR in the Solar Project Management API.

## Overview

The API implements a real-time notification system using SignalR to provide:
- Live project updates
- Real-time collaboration on daily reports  
- Instant notifications for work requests, approvals, and status changes
- Real-time progress tracking
- Live user presence indicators

## SignalR Hub Endpoint

**Hub URL:** `/notificationHub`

**Authentication:** Required (JWT Bearer token)

## Client Connection

### JavaScript/TypeScript Example
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => {
            return localStorage.getItem("jwt-token"); // Your JWT token
        }
    })
    .withAutomaticReconnect()
    .build();

// Start connection
await connection.start();
console.log("SignalR Connected");
```

### C# Client Example
```csharp
var connection = new HubConnectionBuilder()
    .WithUrl("https://your-api-url/notificationHub", options =>
    {
        options.AccessTokenProvider = () => Task.FromResult(yourJwtToken);
    })
    .WithAutomaticReconnect()
    .Build();

await connection.StartAsync();
```

## Available Hub Methods

### Project Management

#### Join Project Group
Join a project-specific group to receive real-time updates for that project.

```javascript
await connection.invoke("JoinProjectGroup", "project-id-here");
```

#### Leave Project Group  
Leave a project-specific group.

```javascript
await connection.invoke("LeaveProjectGroup", "project-id-here");
```

#### Send Project Message
Send a message to all users in a project group.

```javascript
await connection.invoke("SendProjectMessage", "project-id", "Hello team!");
```

### Daily Report Collaboration

#### Join Daily Report Session
Join a collaborative editing session for a specific daily report.

```javascript
await connection.invoke("JoinDailyReportSession", "daily-report-id");
```

#### Leave Daily Report Session
Leave a daily report editing session.

```javascript
await connection.invoke("LeaveDailyReportSession", "daily-report-id");  
```

#### Update Report Field
Notify others when editing a specific field in a daily report.

```javascript
await connection.invoke("UpdateReportField", "daily-report-id", "fieldName", "new-value");
```

#### Start/Stop Typing Indicator
Show typing indicators during collaborative editing.

```javascript
// Start typing
await connection.invoke("StartTyping", "daily-report-id", "fieldName");

// Stop typing  
await connection.invoke("StopTyping", "daily-report-id", "fieldName");
```

### User Presence

#### Update User Status
Update your online status.

```javascript
await connection.invoke("UpdateUserStatus", "online"); // "online", "away", "busy"
```

## Event Listeners

### Project Events

```javascript
// User joined project
connection.on("UserJoinedProject", (data) => {
    console.log(`${data.UserName} joined project ${data.ProjectId}`);
});

// User left project
connection.on("UserLeftProject", (data) => {
    console.log(`${data.UserName} left project ${data.ProjectId}`);
});

// Project message received
connection.on("ProjectMessageReceived", (data) => {
    console.log(`Message from ${data.SenderName}: ${data.Message}`);
});
```

### Daily Report Events

```javascript
// Someone joined report session
connection.on("UserJoinedReportSession", (data) => {
    console.log(`${data.UserName} started editing report ${data.ReportId}`);
});

// Report field updated
connection.on("ReportFieldUpdated", (data) => {
    console.log(`Field ${data.FieldName} updated to: ${data.NewValue}`);
    // Update your UI accordingly
});

// Typing indicators  
connection.on("UserStartedTyping", (data) => {
    showTypingIndicator(data.UserName, data.FieldName);
});

connection.on("UserStoppedTyping", (data) => {
    hideTypingIndicator(data.UserName, data.FieldName);
});
```

### Notification Events

```javascript
// General notification
connection.on("ReceiveNotification", (notification) => {
    showNotification(notification.Message, notification.Type);
});

// Work request notifications
connection.on("WorkRequestCreated", (data) => {
    console.log("New work request created:", data);
});

connection.on("WorkRequestStatusChanged", (data) => {
    console.log(`Work request ${data.WorkRequestId} status: ${data.NewStatus}`);
});

// Daily report notifications
connection.on("DailyReportCreated", (data) => {
    console.log("New daily report created:", data);
});

connection.on("DailyReportApprovalStatusChanged", (data) => {
    console.log(`Daily report ${data.ReportId} approval: ${data.ApprovalStatus}`);
});

// Progress updates
connection.on("RealTimeProgressUpdate", (data) => {
    updateProgressBar(data.ProjectId, data.CompletionPercentage);
});

// System announcements
connection.on("SystemAnnouncement", (data) => {
    showSystemMessage(data.Message, data.Priority);
});
```

### User Presence Events

```javascript
connection.on("UserStatusChanged", (data) => {
    updateUserStatus(data.UserId, data.Status);
});
```

## Server-Side Service Integration

### Using INotificationService

The `SignalRNotificationService` implements `INotificationService` and provides methods for sending notifications:

```csharp
public class YourController : ControllerBase
{
    private readonly INotificationService _notificationService;
    
    public YourController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    
    [HttpPost("create-work-request")]
    public async Task<IActionResult> CreateWorkRequest(CreateWorkRequestDto dto)
    {
        // Create work request logic...
        
        // Send real-time notification
        await _notificationService.SendWorkRequestCreatedNotificationAsync(workRequest);
        
        return Ok(workRequest);
    }
}
```

### Available Notification Methods

- `SendNotificationAsync(message, userId)` - Send basic notification
- `SendWorkRequestCreatedNotificationAsync(workRequest)` - Work request created
- `SendWorkRequestStatusChangedNotificationAsync(workRequest, oldStatus)` - Status change
- `SendDailyReportCreatedNotificationAsync(dailyReport)` - Daily report created
- `SendDailyReportApprovalStatusChangeAsync(dailyReport, oldStatus)` - Approval change
- `SendRealTimeProgressUpdateAsync(projectId, completionPercentage, updatedBy)` - Progress update
- `SendSystemAnnouncementAsync(message, priority, targetAudience)` - System messages

## Testing SignalR Integration

### Test Endpoints

The API provides test endpoints for SignalR functionality:

```bash
# Send test notification
POST /api/v1/notifications/test
{
    "message": "Test notification",
    "type": "Info"
}

# Send system announcement  
POST /api/v1/notifications/system-announcement
{
    "message": "System maintenance in 30 minutes",
    "priority": "High",
    "targetAudience": "All"
}

# Get SignalR connection info
GET /api/v1/notifications/connection-info
```

### Manual Testing with Browser Console

```javascript
// Connect to SignalR
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .build();

await connection.start();

// Join a project group
await connection.invoke("JoinProjectGroup", "your-project-id");

// Listen for notifications
connection.on("ReceiveNotification", (data) => {
    console.log("Notification received:", data);
});
```

## Configuration

### SignalR Configuration in Program.cs

```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(5);
    options.KeepAliveInterval = TimeSpan.FromMinutes(2);
    options.MaximumReceiveMessageSize = 1024 * 1024; // 1MB
});
```

### CORS Configuration for SignalR

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("SignalRCorsPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Important for SignalR
    });
});
```

## Error Handling

### Connection Errors

```javascript
connection.onclose(async () => {
    console.log("SignalR connection closed");
    // Implement reconnection logic if needed
});

connection.onreconnecting((error) => {
    console.log("SignalR reconnecting:", error);
});

connection.onreconnected((connectionId) => {
    console.log("SignalR reconnected:", connectionId);
    // Re-join groups if needed
});
```

### Hub Method Errors

```javascript
try {
    await connection.invoke("JoinProjectGroup", projectId);
} catch (error) {
    console.error("Error joining project group:", error);
}
```

## Best Practices

### Client-Side

1. **Handle Connection State**: Always check connection state before invoking methods
2. **Implement Reconnection**: Use automatic reconnection and handle reconnection events
3. **Manage Groups**: Re-join groups after reconnection
4. **Error Handling**: Wrap hub method calls in try-catch blocks
5. **Memory Management**: Remove event listeners when components unmount

```javascript
// Check connection state
if (connection.state === signalR.HubConnectionState.Connected) {
    await connection.invoke("JoinProjectGroup", projectId);
}

// Clean up on component unmount
useEffect(() => {
    return () => {
        connection.off("ReceiveNotification");
        connection.stop();
    };
}, []);
```

### Server-Side

1. **Use Groups Efficiently**: Leverage SignalR groups for targeted messaging
2. **Handle Exceptions**: Wrap hub methods in try-catch blocks
3. **Validate Parameters**: Always validate input parameters in hub methods
4. **Use Background Services**: For heavy operations, use background services instead of hub methods
5. **Monitor Performance**: Log connection metrics and monitor hub performance

## Security Considerations

1. **Authentication Required**: All hub connections require JWT authentication
2. **Authorization**: Hub methods verify user permissions before executing
3. **Input Validation**: All hub method parameters are validated
4. **Rate Limiting**: Consider implementing rate limiting for hub methods
5. **Group Access Control**: Users can only join groups they have permission to access

## Monitoring and Logging

The system includes comprehensive logging for SignalR operations:

- Connection/disconnection events
- Group join/leave operations  
- Message sending/receiving
- Error conditions
- Performance metrics

Logs are available in the application logs with the category `dotnet_rest_api.Hubs.NotificationHub`.

## Scaling Considerations

For production deployments with multiple server instances:

1. **Azure SignalR Service**: Use Azure SignalR Service for cloud deployments
2. **Redis Backplane**: Configure Redis backplane for on-premises scaling
3. **Connection Limits**: Monitor and plan for connection limits
4. **Load Balancing**: Ensure sticky sessions if not using backplane

```csharp
// Azure SignalR Service
builder.Services.AddSignalR().AddAzureSignalR(connectionString);

// Redis backplane  
builder.Services.AddSignalR().AddStackExchangeRedis(connectionString);
```

## Common Integration Patterns

### React/Next.js Integration

```typescript
// hooks/useSignalR.ts
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

export const useSignalR = (token: string) => {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [isConnected, setIsConnected] = useState(false);

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('/notificationHub', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);

        newConnection.start()
            .then(() => setIsConnected(true))
            .catch(err => console.error('SignalR connection error:', err));

        return () => {
            newConnection.stop();
        };
    }, [token]);

    return { connection, isConnected };
};
```

### Vue.js Integration

```typescript
// composables/useSignalR.ts
import { ref, onUnmounted } from 'vue';
import * as signalR from '@microsoft/signalr';

export function useSignalR(token: string) {
    const connection = ref<signalR.HubConnection | null>(null);
    const isConnected = ref(false);

    const connect = async () => {
        connection.value = new signalR.HubConnectionBuilder()
            .withUrl('/notificationHub', {
                accessTokenFactory: () => token
            })
            .withAutomaticReconnect()
            .build();

        try {
            await connection.value.start();
            isConnected.value = true;
        } catch (error) {
            console.error('SignalR connection error:', error);
        }
    };

    onUnmounted(() => {
        connection.value?.stop();
    });

    return { connection, isConnected, connect };
}
```

## Support and Troubleshooting

### Common Issues

1. **Connection Fails**: Check JWT token validity and CORS configuration
2. **Messages Not Received**: Verify group membership and event listeners
3. **Frequent Disconnections**: Check network stability and timeout settings
4. **Authentication Errors**: Ensure JWT token is properly formatted and not expired

### Debug Mode

Enable detailed errors in development:

```csharp
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; // Only in development
});
```

This comprehensive real-time notification system provides a robust foundation for live updates and collaboration features in your Solar Project Management application.

---
*Last Updated: July 4, 2025*
