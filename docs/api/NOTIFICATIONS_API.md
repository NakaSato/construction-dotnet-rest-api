# 🔔 Notifications API - Complete Documentation

## 📋 Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Authentication](#authentication)
4. [Core Endpoints](#core-endpoints)
5. [Notification Types](#notification-types)
6. [Real-time Integration](#real-time-integration)
7. [Mobile Push Notifications](#mobile-push-notifications)
8. [Flutter Integration](#flutter-integration)
9. [Notification Settings](#notification-settings)
10. [Error Handling](#error-handling)
11. [Performance Considerations](#performance-considerations)
12. [Best Practices](#best-practices)
13. [Troubleshooting](#troubleshooting)

---

## Overview

The Notifications API provides comprehensive notification management for the Solar Projects platform, supporting real-time notifications, mobile push notifications, email alerts, and customizable notification preferences. This API is designed to work seamlessly with web applications, mobile applications (including Flutter), and system integrations.

### Key Features
- 🔄 **Real-time Notifications**: Instant delivery via SignalR with WebSocket and Long Polling fallbacks
- 📱 **Mobile Push**: Native mobile app notifications with rich media support
- 📧 **Email Integration**: Automated email notifications with HTML templates
- ⚙️ **Customizable Settings**: User-controlled notification preferences for each notification type
- 🎯 **Smart Filtering**: Priority-based and type-based filtering with contextual relevance
- 📊 **Analytics**: Notification metrics, delivery statistics, and engagement insights
- 🔒 **Secure**: Role-based notification access control with end-to-end encryption
- ⚡ **High Performance**: Optimized for minimal battery and bandwidth usage in mobile scenarios
- 🌐 **Offline Support**: Notification queueing and synchronization for offline scenarios

### Base Information
- **Base URL**: `/api/v1/notifications`
- **Authentication**: JWT Bearer Token required
- **Content-Type**: `application/json`
- **Real-time Hub**: `/notificationHub`

---

## Quick Start

### 1. Get User Notifications
```http
GET /api/v1/notifications
Authorization: Bearer <your-jwt-token>
```

**Response Structure:**
- `success`: Boolean indicating success
- `message`: Response message
- `data`: Object containing notification items, total count and unread count
  - `items`: Array of notification objects
  - `totalCount`: Total number of notifications
  - `unreadCount`: Number of unread notifications

*JSON response example removed - see implementation guide*

### 2. Mark as Read
```http
PATCH /api/v1/notifications/{id}/read
Authorization: Bearer <your-jwt-token>
```

**Response Structure:**
- `success`: Boolean indicating success
- `message`: Confirmation message

*JSON response example removed - see implementation guide*

### 3. Real-time Connection

#### Web Applications (JavaScript)

*Code example removed - see implementation guide*

#### Mobile Applications
For mobile applications, see the [Flutter Integration](#flutter-integration) and [Mobile Push Notifications](#mobile-push-notifications) sections.

---

## Authentication

All notification endpoints require JWT authentication. Include the token in the Authorization header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

For real-time connections via SignalR, provide the token either:
1. In the query string: `?access_token=<token>` (Not recommended for production)
2. Using the accessTokenFactory option in the client configuration (preferred method)

### Permission Levels
- **User**: Can view and manage own notifications
- **Manager**: Can view team notifications and create system announcements
- **Admin**: Full notification management and system-wide announcements

### Token Renewal and Expiration
SignalR connections need valid tokens throughout the connection lifetime. Implement token renewal to maintain long-lived connections.

*Code example removed - see implementation guide*

Mobile applications should implement similar token refresh mechanisms using platform-specific methods.

---

## Core Endpoints

<!-- All endpoint documentation remains unchanged -->

---

## Notification Types

<!-- All notification type documentation remains unchanged -->

---

## Real-time Integration

<!-- All SignalR and real-time integration documentation remains unchanged -->

---

## Mobile Push Notifications

### Setup and Configuration

The notification system supports native mobile push notifications through Firebase Cloud Messaging (FCM) for Android and Apple Push Notification service (APNs) for iOS.

#### Registration

The registration process involves:
1. Requesting notification permission from the user
2. Retrieving a push notification token
3. Registering the token with the backend API

*Code example removed - see implementation guide*

#### Device Registration Endpoint
**POST** `/api/v1/notifications/register-device`

**Request Structure:**
- `pushToken`: Device push notification token
- `platform`: Device platform (ios/android)
- `deviceInfo`: Device information object
- `preferences`: User preferences for push notifications

*JSON request example removed - see implementation guide*

### Push Notification Handling

Push notification handling involves:

1. **Receiving Notifications:**
   - Listening for incoming push notifications
   - Extracting notification data
   - Updating local notification store
   - Updating badge counts
   - Handling specific notification types

2. **Handling User Interactions:**
   - Detecting notification taps
   - Navigating to relevant screens
   - Marking notifications as read

3. **Background Notification Handling:**
   - Determining alert behavior based on priority
   - Managing notification sounds and badges
   - Setting appropriate notification priority

*Code examples removed - see implementation guide*

### Rich Push Notifications

Rich push notifications include:

1. **Core Components:**
   - Title and body text
   - Notification type and priority
   - Deep link action URL
   - Custom payload data

2. **iOS-Specific Features:**
   - Badge count
   - Custom sound files
   - Category ID for action grouping
   - Media attachments

3. **Android-Specific Features:**
   - Channel ID for grouping
   - Priority setting
   - Custom actions with icons

*Code example removed - see implementation guide*
```

### Notification Categories and Actions

Notification categories allow grouping notifications by type and defining available actions:

1. **Category Definition:**
   - Each category has a unique ID and a set of actions
   - Actions include accept, defer, approve, review, etc.
   - Actions can be configured with various options (foreground, destructive, etc.)

2. **Category Registration:**
   - Categories must be registered with the notification system
   - Different platforms may require different registration methods

3. **Action Handling:**
   - Actions trigger specific handlers when selected
   - Handlers may perform tasks like accepting assignments, approving reports, or scheduling reminders

*Code example removed - see implementation guide*

---

## Notification Settings

### Overview

The Notification Settings API allows users to customize their notification preferences. Users can control which types of notifications they receive, how they receive them (email, in-app, mobile push), and set custom filters and schedules.

### Endpoints

#### Get User Notification Preferences

```http
GET /api/v1/notifications/preferences
Authorization: Bearer <your-jwt-token>
```

**Response Structure:**
- `success`: Boolean indicating success
- `data`: Object containing user notification preferences
  - `emailNotifications`: Boolean for email notifications
  - `pushNotifications`: Boolean for push notifications
  - `inAppNotifications`: Boolean for in-app notifications
  - `digestFrequency`: Frequency for digest notifications
  - `quietHours`: Object containing quiet hours settings
  - `typePreferences`: Array of notification type preferences
  - `projectPreferences`: Array of project-specific preferences
  - `filters`: Object containing regional and facility filters

*JSON response example removed - see implementation guide*
```

#### Update User Notification Preferences

```http
PUT /api/v1/notifications/preferences
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request Structure:**
- `emailNotifications`: Boolean for email notifications
- `pushNotifications`: Boolean for push notifications
- `inAppNotifications`: Boolean for in-app notifications
- `digestFrequency`: Frequency for digest notifications
- `quietHours`: Object containing quiet hours settings
- `typePreferences`: Array of notification type preferences
- `projectPreferences`: Array of project-specific preferences
- `filters`: Object containing regional and facility filters

**Response Structure:**
- `success`: Boolean indicating success
- `message`: Confirmation message

*JSON request and response examples removed - see implementation guide*

#### Update Notification Type Preference

```http
PATCH /api/v1/notifications/preferences/types/{notificationType}
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request Structure:**
- `email`: Boolean for email notifications
- `push`: Boolean for push notifications
- `inApp`: Boolean for in-app notifications
- `priority`: Priority level for this notification type

**Response Structure:**
- `success`: Boolean indicating success
- `message`: Confirmation message

*JSON request and response examples removed - see implementation guide*

#### Update Project Notification Preference

```http
PATCH /api/v1/notifications/preferences/projects/{projectId}
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```

**Request Structure:**
- `muted`: Boolean indicating if project notifications are muted
- `priority`: Priority level for this project's notifications

**Response Structure:**
- `success`: Boolean indicating success
- `message`: Confirmation message

*JSON request and response examples removed - see implementation guide*

### Client Implementation Examples

#### JavaScript/Web

Client implementation for JavaScript/Web applications should include:

1. **Core Functions:**
   - Getting user notification preferences
   - Updating user notification preferences
   - Toggling individual notification types
   - Managing project notification settings

2. **Implementation Considerations:**
   - JWT token authentication
   - Error handling
   - Response parsing
   - Preference state management

*Code examples removed - see implementation guide*

#### Flutter/Mobile

Flutter implementation should include:

1. **Service Class:**
   - Methods for getting and updating preferences
   - Type-specific preference updates
   - Project-specific preference updates
   - Error handling and logging

2. **UI Components:**
   - Preference management screen
   - Loading state handling
   - User feedback for operations
   - Form components for preference editing

*Code examples removed - see implementation guide*
```

### Database Schema

The notification preference system uses three primary tables:

1. **User Notification Preferences Table:**
   - Stores global notification settings for each user
   - Includes email, push, and in-app notification toggles
   - Manages digest frequency and quiet hours
   - Contains timezone and filter preferences

2. **Notification Type Preferences Table:**
   - Stores per-notification-type settings for each user
   - Controls delivery channel preferences (email/push/in-app)
   - Sets priority levels for different notification types
   - Enforces unique constraints on user-type combinations

3. **Project Notification Preferences Table:**
   - Manages project-specific notification settings
   - Allows muting notifications for specific projects
   - Sets project-specific priority levels
   - Maintains referential integrity with users and projects tables

*SQL schema examples removed - see implementation guide*

### Best Practices for Notification Preferences

1. **Default Preferences**
   - Set sensible default preferences for new users
   - Consider user roles when establishing defaults
   - Allow administrators to configure organization-wide defaults

2. **Preference Granularity**
   - Offer both global and fine-grained control
   - Allow per-project notification settings
   - Enable channel-specific preferences (email, push, in-app)

3. **User Experience**
   - Implement an intuitive preference management UI
   - Group related settings logically
   - Provide explanations for each notification type
   - Include examples of when notifications would be sent

4. **Compliance and Privacy**
   - Honor user preferences strictly
   - Allow complete opt-out from non-critical notifications
   - Include privacy policy information
   - Keep preference change audit logs

5. **Integration**
   - Sync preferences across platforms and devices
   - Validate preferences during notification dispatch
   - Cache preferences for performance
   - Implement bulk preference changes

---

## Error Handling

<!-- All error handling documentation remains unchanged -->

---

## Best Practices

<!-- All best practices documentation remains unchanged -->

---

## Performance Considerations

### Mobile Optimization

When implementing the Notifications API for mobile applications, consider these performance optimizations:

1. **Battery Conservation**
   - Use a hybrid approach: WebSockets for active use, FCM/APNs for background notifications
   - Implement adaptive polling frequency based on application state
   - Set appropriate reconnection intervals based on battery level

2. **Bandwidth Management**
   - Request compressed payloads using `Accept-Encoding: gzip` header
   - Implement pagination with reasonable page sizes (20-50 items)
   - Use notification cursors for efficient pagination
   - Consider implementing notification batching for bulk operations

3. **Memory Usage**
   - Limit in-memory notification cache size
   - Implement virtual scrolling for long notification lists
   - Use lazy loading for notification details and media content

### Server-Side Efficiency

1. **Database Optimization**
   - Implement proper indexes on notification tables
   - Use read replicas for notification queries
   - Implement time-based partitioning for notification history
   - Consider time-to-live (TTL) policies for old notifications

2. **Caching Strategy**
   - Cache frequently accessed notifications
   - Use Redis for notification counters and status
   - Implement notification deduplication

3. **Load Balancing**
   - Use sticky sessions for SignalR connections
   - Implement horizontal scaling with Redis backplane
   - Distribute push notification workloads across dedicated workers

### High Volume Scenarios

For systems with high notification volumes (>100k notifications/day):

1. **Queue Processing**
   - Implement notification prioritization
   - Use separate queues for different notification types
   - Set up dead letter queues for failed notifications
   - Implement retry policies with exponential backoff

2. **Rate Limiting**
   - Apply per-user rate limits to prevent notification flooding
   - Implement throttling for high-frequency notification sources
   - Consolidate similar notifications into digest formats

3. **Monitoring**
   - Track notification delivery metrics
   - Monitor client reconnection patterns
   - Set up alerts for notification delivery failures
   - Use distributed tracing to identify bottlenecks

---

## Troubleshooting

### Common Issues and Solutions

#### Connection Problems

1. **SignalR connection fails to establish**
   - **Symptoms**: Connection timeouts, 401 responses, client reconnection loops
   - **Possible causes**:
     - Expired authentication token
     - CORS configuration issues
     - Network restrictions
     - Server unavailability
   - **Solutions**:
     - Verify token expiration and renewal logic
     - Check CORS settings on server
     - Try transport fallback options
     - Implement a reconnection strategy with backoff

2. **Notifications not received in real-time**
   - **Symptoms**: Delayed notifications, missing notifications
   - **Possible causes**:
     - SignalR connection dropped
     - Client in background/sleep mode
     - Server-side scaling issues
   - **Solutions**:
     - Implement client-side connection monitoring
     - Use push notifications for background delivery
     - Check server-side SignalR backplane configuration
     - Implement notification delivery confirmation

#### Mobile Push Notification Issues

1. **Push notifications not delivered**
   - **Symptoms**: FCM/APNs tokens registered but no notifications arriving
   - **Possible causes**:
     - Invalid/expired push tokens
     - User disabled notifications
     - App in background for too long
     - Push service quota exceeded
   - **Solutions**:
     - Implement token refresh logic
     - Check notification permission status
     - Verify FCM/APNs configuration
     - Monitor push service quotas and errors

2. **Duplicate notifications**
   - **Symptoms**: Users receive the same notification multiple times
   - **Possible causes**:
     - Multiple registered devices
     - Reconnection causing redelivery
     - Missing notification acknowledgment
   - **Solutions**:
     - Implement notification IDs and deduplication
     - Track notification delivery status
     - Use idempotent notification processing

### Debug Endpoints

For development and testing purposes, use these endpoints:

```http
GET /api/v1/notifications/debug/status
Authorization: Bearer <your-jwt-token>
```
Returns connection status, queue stats, and delivery metrics

```http
POST /api/v1/notifications/debug/test
Authorization: Bearer <your-jwt-token>
Content-Type: application/json
```
Sends a test notification to the authenticated user

**Request Structure:**
- `type`: Notification type (e.g., "Test")
- `priority`: Notification priority (e.g., "Medium")
- `message`: Notification message content

*JSON request example removed - see implementation guide*

### Logging and Diagnostics

Enable detailed client-side logging:

#### Web SignalR
- Configure logging level (Debug, Information, Warning, Error)
- Set URL and connection options
- Configure automatic reconnection

#### Flutter
- Set up HTTP connection options
- Configure logging function
- Manage log message content visibility

*Code examples removed - see implementation guide*

---

## Analytics

The Notifications API includes built-in analytics capabilities that allow you to track, analyze, and optimize notification delivery and engagement. These analytics provide valuable insights into notification performance and user interaction patterns.

### Core Metrics Endpoints

#### Get Notification Analytics Summary

```http
GET /api/v1/notifications/analytics/summary
Authorization: Bearer <your-jwt-token>
```

**Query Parameters:**
- `startDate` - Start date for analytics period (YYYY-MM-DD)
- `endDate` - End date for analytics period (YYYY-MM-DD)
- `groupBy` - Group metrics by: `day`, `week`, or `month` (default: `day`)

**Response Structure:**
- `success`: Boolean indicating success
- `data`: Object containing analytics data
  - `deliveryMetrics`: Statistics about notification delivery
  - `engagementMetrics`: User engagement statistics
  - `typeBreakdown`: Performance metrics by notification type
  - `timeDistribution`: Distribution of notifications by time of day

*JSON response example removed - see implementation guide*
```

#### Get User-Specific Analytics

```http
GET /api/v1/notifications/analytics/user
Authorization: Bearer <your-jwt-token>
```

**Response Structure:**
- `success`: Boolean indicating success
- `data`: Object containing user-specific analytics
  - `totalReceived`: Total notifications received
  - `openRate`: Overall open rate percentage
  - `actionRate`: Overall action rate percentage
  - `averageTimeToOpen`: Average time to open notifications
  - `preferredChannels`: Performance metrics by delivery channel
  - `mostEngagedTypes`: Notification types with highest engagement
  - `peakEngagementTimes`: Times with highest engagement rates

*JSON response example removed - see implementation guide*
    ],
    "peakEngagementTimes": [
      {
        "hour": 9,
        ### Client-Side Event Tracking

For optimal analytics, track notification events from the client side:

#### Web Client Implementation

1. **Event Tracking Function:**
   - Sends event data to tracking endpoint
   - Includes notification ID, event type, and timestamp
   - Supports custom metadata for additional context
   - Handles errors gracefully

2. **Event Types to Track:**
   - `delivered`: When notification is received by client
   - `opened`: When notification is viewed by user
   - `actioned`: When user takes action on notification
   - `dismissed`: When notification is dismissed

3. **Event Binding:**
   - Attach event tracking to notification interaction elements
   - Track different interaction types with appropriate event types
   - Include relevant metadata with events

*JavaScript code example removed - see implementation guide*

#### Flutter/Mobile Implementation

1. **Event Tracking Function:**
   - Posts event data to tracking endpoint
   - Includes platform-specific information
   - Handles network errors appropriately
   - Supports optional metadata parameters

2. **Implementation Considerations:**
   - Add authentication headers
   - Use proper JSON serialization
   - Handle background vs. foreground tracking differently
   - Consider offline tracking with later synchronization

*Dart/Flutter code example removed - see implementation guide*

### Integration with Firebase Analytics

For mobile applications, you can integrate with Firebase Analytics:

1. **Dual Tracking Approach:**
   - Track events in your API for consistency across platforms
   - Track events in Firebase for mobile-specific analytics
   - Handle errors independently for each tracking system

2. **Firebase Event Parameters:**
   - Notification ID for unique identification
   - Notification type for categorization
   - Priority level for importance analysis
   - Timestamp for temporal analysis
   - Additional contextual parameters as needed

3. **Event Naming Convention:**
   - Use consistent prefixes (`notification_`)
   - Append event type (opened, actioned, etc.)
   - Follow Firebase naming conventions

*Dart/Flutter code example removed - see implementation guide*

### Analytics Dashboard

The analytics dashboard is available to administrators and managers at:
```
https://your-api-domain.com/admin/analytics/notifications
```

The dashboard provides:

1. **Overview Metrics**
   - Delivery rates
   - Engagement trends over time
   - Performance by notification type
   - User engagement heat maps

2. **User Segmentation**
   - Response rates by user role
   - Engagement by geographic region
   - Device and platform preferences
   - Time-of-day effectiveness

3. **Content Analysis**
   - Highest performing message formats
   - Optimal message length
   - Most effective call-to-action phrases
   - A/B testing results

4. **Technical Performance**
   - Delivery latency metrics
   - SignalR connection stability
   - Push notification delivery success rates
   - Error analysis and troubleshooting

### Notification Campaign Analysis

For large-scale notification campaigns (e.g., system-wide announcements), use the campaign analysis endpoint:

```http
GET /api/v1/notifications/analytics/campaigns/{campaignId}
Authorization: Bearer <your-jwt-token>
```

**Response Structure:**
- `success`: Boolean indicating success
- `data`: Object containing campaign analytics
  - `campaignId`: Unique campaign identifier
  - `campaignName`: Name of the notification campaign
  - `startDate` & `endDate`: Campaign duration
  - `deliveryMetrics`: Delivery statistics across channels
  - `engagementMetrics`: Engagement statistics by platform
  - `timeSeries`: Day-by-day campaign performance
  - `userSegmentPerformance`: Performance metrics by user role

*JSON response example removed - see implementation guide*
  }
}
```

### Notification Lifecycle Tracking

Track the complete lifecycle of notifications:

1. **Creation**: When a notification is generated
2. **Queued**: When a notification enters the delivery queue
3. **Sent**: When a notification is sent from the server
4. **Delivered**: When a notification is received by the client
5. **Displayed**: When a notification is displayed to the user
6. **Opened**: When a user opens/views a notification
7. **Actioned**: When a user takes action on a notification
8. **Dismissed**: When a user dismisses a notification
9. **Expired**: When a notification reaches its expiration time

### Best Practices for Analytics

1. **Privacy Considerations**
   - Only collect necessary data for analytics purposes
   - Anonymize sensitive user data
   - Comply with privacy regulations (GDPR, CCPA)
   - Provide transparency about analytics data collection

2. **Performance Optimization**
   - Use batch processing for analytics events
   - Implement a separate database for analytics data
   - Consider time-series databases for metrics
   - Set up proper indexing for analytics queries

3. **Business Intelligence**
   - Export analytics data to BI tools for advanced analysis
   - Set up regular reporting on key notification metrics
   - Use insights to optimize notification strategy
   - Create custom dashboards for different stakeholders

4. **A/B Testing**
   - Test notification content variations
   - Experiment with delivery timing
   - Compare different notification designs
   - Evaluate the impact of personalization

---

## Support Resources

- Join the developer community on Discord: [discord.example.com/solar-api](https://discord.example.com/solar-api)
- Report issues: [github.com/solar-project/api/issues](https://github.com/solar-project/api/issues)
- API status page: [status.solar-api.example.com](https://status.solar-api.example.com)

---

## Flutter Integration

### Overview

Flutter integration for the Notifications API provides comprehensive support for real-time notifications in mobile applications. This section covers both REST API and SignalR integration patterns to deliver a seamless notification experience in Flutter apps.

### Setup and Dependencies

Required packages for Flutter integration:

1. **Core Dependencies:**
   - HTTP client for REST API calls
   - SignalR client for real-time notifications
   - Local notification management
   - Push notification handling
   - State management solution

2. **Version Requirements:**
   - Use compatible package versions
   - Consider platform-specific requirements
   - Ensure dependency compatibility

*YAML dependency example removed - see implementation guide*

### Authentication Configuration

Authentication implementation should include:

1. **Token Management:**
   - Secure storage of authentication tokens
   - Automatic inclusion in API requests
   - Token expiration checking
   - Token refresh mechanism

2. **Error Handling:**
   - Token refresh failure handling
   - Redirection to login when authentication fails
   - Logging for authentication issues

*Dart/Flutter code example removed - see implementation guide*
```

### REST API Notification Client

Create a dedicated notification service for API calls with the following features:

1. **Core Functionality:**
   - Fetch notifications with pagination and filtering
   - Mark notifications as read (single or all)
   - Update notification preferences
   - Register devices for push notifications

2. **Implementation Requirements:**
   - HTTP client integration (Dio recommended)
   - Proper error handling
   - Response parsing and model mapping
   - Query parameter management
   - Authentication header handling

3. **Service Structure:**
   - Constructor with dependency injection
   - Public methods for API operations
   - Private error handling method
   - Response transformation utilities

*Dart/Flutter code example removed - see implementation guide*
```

### SignalR Real-Time Integration

Create a SignalR client for real-time notifications with these key components:

1. **Connection Management:**
   - Initialize and configure the SignalR connection
   - Handle authentication with token factory
   - Implement automatic reconnection with backoff
   - Manage connection state tracking

2. **Group Membership:**
   - Join user-specific notification groups
   - Join role-based groups for targeted notifications
   - Join and leave project-specific groups
   - Rejoin groups after reconnection

3. **Event Handling:**
   - Register handlers for incoming notifications
   - Handle project status changes
   - Monitor connection state changes
   - Process reconnecting, reconnected, and close events

4. **Error Handling:**
   - Graceful connection failure handling
   - Reconnection retry logic
   - Error logging and reporting
   - Fallback mechanisms for persistent failures

*Dart/Flutter code example removed - see implementation guide*
```

### Notification Provider

Create a notification provider for state management with these key components:

1. **State Variables:**
   - Notification collection
   - Unread count tracker
   - Loading state indicator
   - Error state handling
   - Pagination controls

2. **Key Functionalities:**
   - Initialize SignalR connection
   - Fetch notifications with filtering options
   - Mark notifications as read (single or all)
   - Handle real-time notification receipt
   - Process connection state changes
   - Show local notifications when appropriate

3. **Implementation Pattern:**
   - Combine ChangeNotifier for state management
   - Implement NotificationHandler interface
   - Use dependency injection for services
   - Provide public getters for state access
   - Include private helper methods for state updates

*Dart/Flutter code example removed - see implementation guide*
  
*Implementation details removed - see implementation guide for full code examples*
```

### Local Notification Service

Handle local notifications when the app is in the background with the following features:

1. **Initialization:**
   - Configure platform-specific settings
   - Set up notification channels and categories
   - Configure permission requests
   - Set up notification event handlers

2. **Core Functionality:**
   - Show local notifications with customizable content
   - Handle notification taps and actions
   - Support rich notifications with images
   - Manage notification lifecycle

*Dart/Flutter code example removed - see implementation guide*
*Implementation details removed - see implementation guide for full code examples*
}
```

### Complete Integration Example

The complete integration involves setting up a provider architecture with:

1. **Service Registration:**
   - Authentication service for token management
   - Local notification service for device notifications
   - API notification service for REST endpoints
   - SignalR service for real-time connections

2. **Dependency Injection:**
   - Proper order of service initialization
   - Service dependencies passed through constructors
   - Circular dependency resolution

3. **Application Configuration:**
   - Theme and styling setup
   - Navigation configuration
   - Global error handling

*Dart/Flutter code example removed - see implementation guide*
            
            final provider = NotificationProvider(
              apiService: apiService,
              signalRService: signalRService,
              localNotificationService: localNotificationService,
            );
            
            // Now we can set the handler
            signalRService._notificationHandler = provider;
            
            // Initialize
            provider.initialize();
            
            return provider;
          },
          dispose: (_, provider) => provider._signalRService.stop(),
        ),
      ],
      child: MaterialApp(
        title: 'Solar Projects',
        theme: ThemeData(
          primarySwatch: Colors.blue,
          visualDensity: VisualDensity.adaptivePlatformDensity,
        ),
        home: HomeScreen(),
      ),
    );
  }
}
```

### Notification UI Example

Create a notification center screen with the following components:

1. **UI Structure:**
   - App bar with notification title and actions
   - Mark all as read button
   - Loading indicator for initial fetch
   - Error handling with retry option
   - Empty state messaging
   - Pull-to-refresh functionality

2. **State Management:**
   - Consume notification provider state
   - Display notifications in a scrollable list
   - Handle loading and error states
   - Support pagination for large notification sets

3. **User Interactions:**
   - Tap to view notification details
   - Mark individual notifications as read
   - Swipe actions for quick operations
   - Filter options for notification types

*Dart/Flutter code example removed - see implementation guide*
            return Center(
              child: Column(
                mainAxisAlignment: MainAxisAlignment.center,
                children: [
                  Text('Error: ${provider.errorMessage}'),
                  SizedBox(height: 16),
                  ElevatedButton(
                    onPressed: () => provider.fetchNotifications(refresh: true),
                    child: Text('Retry'),
                  ),
                ],
              ),
            );
          }
          
          if (provider.notifications.isEmpty) {
            return Center(child: Text('No notifications'));
          }
          
          return RefreshIndicator(
            onRefresh: () => provider.fetchNotifications(refresh: true),
            child: ListView.builder(
              itemCount: provider.notifications.length,
              itemBuilder: (context, index) {
                final notification = provider.notifications[index];
                return NotificationTile(notification: notification);
              },
            ),
          );
        },
      ),
    );
  }
}

class NotificationTile extends StatelessWidget {
  final NotificationModel notification;
  
  const NotificationTile({Key? key, required this.notification}) : super(key: key);
  
  @override
  Widget build(BuildContext context) {
    return ListTile(
      leading: _buildLeadingIcon(),
      title: Text(notification.title),
      subtitle: Text(notification.message),
      trailing: Text(
        _formatTimeAgo(notification.createdAt),
        style: TextStyle(
          fontSize: 12,
          color: Colors.grey,
        ),
      ),
      contentPadding: EdgeInsets.symmetric(horizontal: 16, vertical: 8),
      tileColor: notification.isRead ? null : Colors.blue.withOpacity(0.05),
      onTap: () {
        // Mark as read
        if (!notification.isRead) {
          context.read<NotificationProvider>().markAsRead(notification.id);
        }
        
        // Handle navigation based on notification type
        _handleNotificationTap(context, notification);
      },
    );
  }
  
  Widget _buildLeadingIcon() {
    IconData iconData;
    Color iconColor;
    
    switch (notification.type) {
      case 'TaskAssigned':
        iconData = Icons.assignment;
        iconColor = Colors.orange;
        break;
      case 'ProjectUpdated':
        iconData = Icons.edit;
        iconColor = Colors.blue;
        break;
      case 'StatusChanged':
        iconData = Icons.sync;
        iconColor = Colors.green;
        break;
      default:
        iconData = Icons.notifications;
        iconColor = Colors.grey;
    }
    
    return CircleAvatar(
      backgroundColor: iconColor.withOpacity(0.2),
      child: Icon(iconData, color: iconColor),
    );
  }
  
  void _handleNotificationTap(BuildContext context, NotificationModel notification) {
    // Navigate based on notification data
    if (notification.actionUrl != null) {
      // Parse the URL and navigate
      final segments = notification.actionUrl!.split('/');
      
      if (segments.length >= 3 && segments[1] == 'projects') {
        final projectId = segments[2];
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => ProjectDetailScreen(projectId: projectId),
          ),
        );
        return;
      }
    }
    
    // Default navigation based on notification type
    switch (notification.type) {
      case 'TaskAssigned':
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => TaskDetailScreen(taskId: notification.payload?['taskId']),
          ),
        );
        break;
      case 'ProjectUpdated':
        Navigator.push(
          context,
          MaterialPageRoute(
            builder: (context) => ProjectDetailScreen(projectId: notification.payload?['projectId']),
          ),
        );
        break;
    }
  }
  
### Notification Badge Example

Implement notification badges in your app's navigation:

1. **Badge Components:**
   - Unread count indicator
   - Visual styling for attention
   - State-based visibility control
   - Animation for new notifications

2. **Integration Points:**
   - Bottom navigation bar items
   - Tab bar icons
   - App icon badge (platform-specific)
   - Header notification icons

3. **State Management:**
   - Real-time unread count updates
   - Badge visibility based on count
   - Badge clearing on notification view

*Dart/Flutter code example removed - see implementation guide*
        },
      ),
      label: 'Notifications',
    ),
    BottomNavigationBarItem(
      icon: Icon(Icons.person),
      label: 'Profile',
    ),
  ],
)
```

### Best Practices for Flutter Integration

1. **Performance Optimization**
   - Use connection state management to handle background/foreground transitions
   - Implement efficient caching for offline access to notifications
   - Use pagination with lazy loading for notification lists
   - Consider debouncing real-time events to prevent UI jank

2. **Battery Efficiency**
   - Disconnect SignalR when app is in background for extended periods
   - Use Firebase Cloud Messaging (FCM) for background notifications
   - Reconnect SignalR when app returns to foreground
   - Implement adaptive polling intervals based on app state

3. **Error Handling**
   - Implement robust reconnection logic for SignalR
   - Cache notifications locally to handle offline scenarios
   - Provide clear feedback to users during connectivity issues
   - Implement graceful degradation to REST API when real-time fails

4. **Security Considerations**
   - Securely store authentication tokens
   - Implement proper token refresh logic
   - Sanitize notification content before display
   - Validate deep links in notification payloads

5. **Testing**
   - Test notification handling in various connectivity scenarios
   - Verify background/foreground notification behavior
   - Test reconnection flows after network interruptions
   - Validate notification interactions across different device states

---

