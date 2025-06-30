# Quick Start Guide: Real-Time Notifications

## Overview
This guide provides quick instructions for using the real-time notification features in the Solar Project Management API.

## Prerequisites
- .NET 9.0 application running
- JWT authentication token
- SignalR client library (for frontend)

## SignalR Connection

### JavaScript Client Example
```javascript
// Install: npm install @microsoft/signalr

import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

// Create connection
const connection = new HubConnectionBuilder()
    .withUrl('/notificationHub', {
        accessTokenFactory: () => yourJwtToken
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Information)
    .build();

// Start connection
await connection.start();
```

## Real-Time Events

### 1. **Project Notifications**
```javascript
// Listen for project updates
connection.on('ProjectUpdated', (projectId, update) => {
    console.log('Project updated:', projectId, update);
});

// Listen for new daily reports
connection.on('DailyReportCreated', (report) => {
    console.log('New daily report:', report);
});
```

### 2. **Live Collaboration**
```javascript
// Join a project session
await connection.invoke('JoinProjectSession', projectId);

// Listen for user presence
connection.on('UserJoinedProject', (userId, userName) => {
    console.log(`${userName} joined the project`);
});

// Listen for field updates
connection.on('FieldUpdated', (field, value, userId) => {
    console.log(`${field} updated to ${value} by ${userId}`);
});

// Send typing indicator
await connection.invoke('SendTypingIndicator', projectId, fieldName, true);
```

### 3. **System Notifications**
```javascript
// Listen for system announcements
connection.on('SystemAnnouncement', (message, priority) => {
    console.log('System announcement:', message);
});

// Listen for general notifications
connection.on('NotificationReceived', (notification) => {
    console.log('New notification:', notification);
});
```

## API Endpoints

### Authentication Required
All endpoints require JWT Bearer token:
```
Authorization: Bearer <your-jwt-token>
```

### 1. **Get Notifications**
```bash
GET /api/v1/notifications
# Query parameters: page, pageSize, unreadOnly
```

### 2. **Mark as Read**
```bash
POST /api/v1/notifications/{notificationId}/read
```

### 3. **Dashboard Statistics**
```bash
GET /api/v1/dashboard/statistics
```

### 4. **System Announcements**
```bash
POST /api/v1/notifications/announcement
Content-Type: application/json

{
    "message": "System maintenance scheduled",
    "priority": "High"
}
```

## Testing Endpoints (Development Only)

### Test SignalR Connection
```bash
POST /api/v1/notifications/test-signalr
Content-Type: application/json

{
    "message": "Test message"
}
```

## Real-Time Features

### 1. **Daily Reports**
- Automatic notifications when reports are created/updated
- Real-time status changes
- Progress tracking updates

### 2. **Project Management**
- Live project updates
- Task assignment notifications
- Progress milestone alerts

### 3. **Work Requests**
- Immediate notification on new requests
- Status change alerts
- Approval workflow updates

### 4. **Collaboration**
- User presence indicators
- Real-time field editing
- Typing indicators
- Session management

## Error Handling

### Connection Errors
```javascript
connection.onclose(async () => {
    console.log('Connection lost. Attempting to reconnect...');
    await connection.start();
});

connection.onreconnected(() => {
    console.log('Successfully reconnected');
});
```

### API Error Responses
```json
{
    "success": false,
    "message": "Error description",
    "errors": ["Detailed error messages"]
}
```

## Performance Tips

### 1. **Connection Management**
- Use automatic reconnect
- Handle connection state properly
- Dispose connections when not needed

### 2. **Event Filtering**
- Subscribe only to needed events
- Use project-specific channels
- Unsubscribe when leaving views

### 3. **Message Batching**
- For high-frequency updates, consider batching
- Use debouncing for typing indicators
- Implement local caching for better UX

## Security Considerations

### 1. **Authentication**
- Always use JWT tokens
- Refresh tokens before expiry
- Handle authentication failures

### 2. **Authorization**
- Users only receive notifications they're authorized to see
- Project-specific filtering applied automatically
- Admin-only features properly protected

### 3. **Data Validation**
- All inputs validated server-side
- XSS protection implemented
- Rate limiting applied

## Troubleshooting

### Common Issues

1. **Connection Fails**
   - Check JWT token validity
   - Verify CORS configuration
   - Check network connectivity

2. **No Notifications Received**
   - Verify authentication
   - Check event subscriptions
   - Review server logs

3. **Performance Issues**
   - Monitor connection count
   - Check message frequency
   - Review client-side handling

### Health Check
```bash
GET /health
# Returns application status and database connectivity
```

## Example Integration

### React Component
```jsx
import React, { useEffect, useState } from 'react';
import { useSignalR } from './hooks/useSignalR';

export const ProjectDashboard = ({ projectId }) => {
    const [notifications, setNotifications] = useState([]);
    const { connection, isConnected } = useSignalR();

    useEffect(() => {
        if (isConnected) {
            // Join project session
            connection.invoke('JoinProjectSession', projectId);

            // Listen for updates
            connection.on('ProjectUpdated', (update) => {
                setNotifications(prev => [...prev, update]);
            });
        }

        return () => {
            if (connection) {
                connection.invoke('LeaveProjectSession', projectId);
            }
        };
    }, [isConnected, projectId]);

    return (
        <div>
            <div className="connection-status">
                Status: {isConnected ? 'Connected' : 'Disconnected'}
            </div>
            <div className="notifications">
                {notifications.map(notification => (
                    <div key={notification.id}>{notification.message}</div>
                ))}
            </div>
        </div>
    );
};
```

---

For more detailed information, see:
- [SignalR Implementation Summary](SIGNALR_IMPLEMENTATION_SUMMARY.md)
- [API Documentation](docs/api/README.md)
- [Real-Time Notifications Guide](docs/api/08_REAL_TIME_NOTIFICATIONS.md)
