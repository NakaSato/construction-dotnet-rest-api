# 📅 Calendar Events & Scheduling API

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (view + create own events), All authenticated users (view)  
**📅 Version**: 1.0  
**🔄 Last Updated**: July 5, 2025  
**⚡ Real-Time Updates**: SignalR WebSocket support for live data synchronization

The Calendar Events API provides comprehensive scheduling and event management capabilities for solar installation projects with **real-time live updates**. This system serves as the central coordination hub for all project-related activities, deadlines, meetings, and resource scheduling across the entire solar project lifecycle, with instant data synchronization across all connected users.

## ⚡ Real-Time Live Updates & Instant Synchronization

**🔄 Live Data Broadcasting**: All calendar API endpoints support **real-time live updates** through SignalR/WebSocket connections. When any user creates, updates, or deletes calendar events, **all connected users receive instant notifications** and see changes immediately without page refresh.

### 🌐 Live Update Features
- **Instant Event Broadcasting**: All CRUD operations (Create, Read, Update, Delete) are broadcast live to all relevant users
- **Real-Time Conflict Detection**: Scheduling conflicts are detected and broadcast instantly to prevent double-booking
- **Live RSVP Updates**: Attendance responses and status changes are synchronized across all user sessions
- **Resource Availability Sync**: Equipment and personnel availability updates are pushed live to all users
- **Project Timeline Changes**: Master plan and task updates automatically refresh calendar views for all users
- **Collaborative Editing**: Multiple users can view and edit calendar events with real-time conflict prevention
- **Cross-Platform Sync**: Changes made on mobile, web, or API are instantly reflected across all platforms

### 📡 SignalR Connection Setup

**Connection Endpoint**: `/hubs/calendar`

**JavaScript Client Example**:
```javascript
// Establish SignalR connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/calendar", {
        accessTokenFactory: () => localStorage.getItem("jwtToken")
    })
    .withAutomaticReconnect()
    .build();

// Event listeners for real-time updates
connection.on("EventCreated", (eventData) => {
    console.log("New event created:", eventData);
    refreshCalendarView();
    showNotification(`New event: ${eventData.title}`, "info");
});

connection.on("EventUpdated", (eventData) => {
    console.log("Event updated:", eventData);
    updateEventInCalendar(eventData);
    showNotification(`Event updated: ${eventData.title}`, "success");
});

connection.on("EventDeleted", (eventId, deletedBy) => {
    console.log("Event deleted:", eventId);
    removeEventFromCalendar(eventId);
    showNotification(`Event deleted by ${deletedBy}`, "warning");
});

connection.on("RSVPUpdated", (eventId, attendeeData) => {
    console.log("RSVP updated:", attendeeData);
    updateAttendeeStatus(eventId, attendeeData);
});

connection.on("ConflictDetected", (conflictData) => {
    console.log("Scheduling conflict detected:", conflictData);
    showConflictAlert(conflictData);
});

connection.on("ResourceUpdated", (resourceData) => {
    console.log("Resource availability updated:", resourceData);
    refreshResourceAvailability(resourceData);
});

// Start connection
connection.start().then(() => {
    console.log("Calendar live updates connected");
    // Join user-specific and project-specific groups
    connection.invoke("JoinUserGroup", userId);
    connection.invoke("JoinProjectGroup", projectId);
}).catch(err => console.error("SignalR connection failed:", err));
```

### 📱 Live Update Event Types

| Event Type | Description | Triggered When | Recipients |
|------------|-------------|----------------|------------|
| `EventCreated` | New calendar event added | POST /api/v1/calendar/events | All project members + event attendees |
| `EventUpdated` | Event details modified | PUT /api/v1/calendar/events/{id} | All attendees + project managers |
| `EventDeleted` | Event removed | DELETE /api/v1/calendar/events/{id} | All attendees + project team |
| `RSVPUpdated` | Attendance status changed | POST /api/v1/calendar/events/{id}/rsvp | Event creator + all attendees |
| `AttendanceMarked` | Check-in/check-out recorded | POST /api/v1/calendar/events/{id}/attendance | Event attendees + managers |
| `ConflictDetected` | Scheduling conflict identified | Any scheduling operation | Affected users + managers |
| `ConflictResolved` | Scheduling conflict resolved | Automatic or manual resolution | Previously affected users |
| `ResourceUpdated` | Equipment/personnel availability changed | Resource booking/release | Users with resource access |
| `WeatherAlert` | Weather conditions affecting events | External weather service | Users with weather-dependent events |
| `DeadlineReminder` | Upcoming deadline notification | Scheduled reminder system | Event attendees + responsible parties |
| `ProjectTimelineUpdated` | Master plan changes affecting calendar | Project timeline modifications | All project team members |

### 🎯 Subscription Management

**Join Specific Update Groups**:
```javascript
// Subscribe to project-specific updates
connection.invoke("JoinProjectGroup", "project-123-id");

// Subscribe to user-specific updates
connection.invoke("JoinUserGroup", "user-456-id");

// Subscribe to resource-specific updates
connection.invoke("JoinResourceGroup", "crane-001");

// Subscribe to location-specific updates
connection.invoke("JoinLocationGroup", "austin-tx-site");
```

**Leave Update Groups**:
```javascript
// Unsubscribe from project updates
connection.invoke("LeaveProjectGroup", "project-123-id");

// Unsubscribe from all groups
connection.invoke("LeaveAllGroups");
```

### 🔔 Push Notifications & Mobile Apps

**Real-time notifications** are also sent as **push notifications** to mobile devices and browser notifications for:
- Event invitations and updates
- RSVP reminders
- Schedule conflicts requiring attention
- Weather alerts for outdoor events
- Deadline and milestone reminders
- Resource availability changes

### 🛡️ Security & Permission Filtering

**Live updates respect user permissions**:
- Users only receive updates for events they have permission to view
- Private events are filtered based on attendee status
- Admin/Manager users receive additional system-wide notifications
- Client/Viewer users only receive public project updates
- Real-time conflict detection considers user's scheduling authority

### 🌍 Multi-Timezone Live Updates

**Timezone-aware broadcasting**:
- All live updates include timezone information
- Client-side automatic conversion to user's local timezone
- Daylight saving time transitions handled automatically
- Global team coordination with real-time timezone display

### 📊 Live Analytics & Metrics

**Real-time dashboard updates**:
- Live project progress metrics
- Real-time resource utilization charts
- Instant conflict resolution statistics
- Live weather impact analysis
- Real-time team availability matrix

## 🏗️ Project-Calendar Integration Model

### 📋 Relationship Structure
- **Project** ↔ **Calendar Events**: One-to-many relationship for project scheduling
- **Tasks** ↔ **Calendar Events**: Direct linkage for task-specific scheduling
- **Master Plans** ↔ **Calendar Events**: Milestone and phase scheduling integration
- **Users** ↔ **Calendar Events**: Personal and team scheduling coordination
- **Resources** ↔ **Calendar Events**: Equipment and personnel allocation tracking

### 🔄 Event Inheritance & Context
- Events inherit project metadata (location, team, client info) from parent projects
- Automatic timezone conversion based on project location
- Real-time synchronization with project progress and task completion
- Cascading updates when project schedules change
- Integration with external calendar systems (Google Calendar, Outlook, iCal)

### 📊 Calendar Intelligence Features
- **Smart Scheduling**: AI-powered optimal time slot suggestions
- **Conflict Prevention**: Real-time detection and resolution of scheduling conflicts
- **Weather Integration**: Weather-aware scheduling with automatic alerts
- **Resource Optimization**: Intelligent resource allocation and availability tracking
- **Progress Synchronization**: Automatic calendar updates based on project progress
- **⚡ Live Data Updates**: Real-time synchronization across all users and devices
- **🔄 Instant Notifications**: WebSocket-powered immediate event updates
- **📱 Multi-Device Sync**: Seamless synchronization across web, mobile, and desktop
- **🌐 Collaborative Editing**: Real-time collaborative calendar management

## ⚡ Calendar Event Capabilities & Permissions

### 👑 Admin & Manager (Full Control)
- ✅ Create, modify, and delete events for any project and team member
- ✅ Schedule project milestones, deadlines, and critical path activities
- ✅ Manage team schedules and resource allocation across all projects
- ✅ Create recurring events and complex event series
- ✅ Send meeting invitations and automated notifications
- ✅ Access comprehensive calendar analytics and reporting
- ✅ Configure calendar settings, permissions, and integrations
- ✅ Override scheduling conflicts and resource constraints
- ✅ Manage external calendar integrations and sync settings
- ✅ Export calendar data and generate scheduling reports

### 🔧 Project Managers (Project-Specific Control)
- ✅ Create and manage events for assigned projects
- ✅ Schedule project-specific activities, milestones, and deadlines
- ✅ Coordinate team schedules within project scope
- ✅ Send team notifications, reminders, and updates
- ✅ Generate project timeline and resource utilization reports
- ✅ Manage client meeting schedules and inspections
- ✅ Create project-specific recurring events
- ❌ Cannot access other projects' confidential events
- ❌ Cannot modify system-wide calendar settings

### 👷 Users (Technicians & Field Staff)
- ✅ View project schedules and assigned events
- ✅ Create personal work events and reminders
- ✅ Receive event notifications and automated reminders
- ✅ Update attendance status and event completion
- ✅ Request schedule changes and time off
- ✅ Access mobile calendar with offline synchronization
- ✅ View weather forecasts for scheduled outdoor work
- ❌ Cannot modify project milestones or critical deadlines
- ❌ Cannot access other users' personal events
- ❌ Cannot change resource allocations

### 👀 Viewers (Clients & Stakeholders)
- ✅ View project events and milestones (public only)
- ✅ See public project schedules and progress updates
- ✅ Receive project milestone notifications
- ❌ Cannot create or modify any events
- ❌ Cannot access internal team scheduling

## 🎯 Calendar Event Features & Capabilities

### 📋 Comprehensive Event Management
- **Event Types**: 
  - 🏗️ Project Milestones (Critical Path Events)
  - 📅 Meetings (Team, Client, Stakeholder)
  - ⏰ Deadlines (Hard and Soft Deadlines)
  - 🔍 Inspections (Safety, Quality, Compliance)
  - 👥 Client Visits (Site Tours, Progress Reviews)
  - 🔧 Maintenance (Scheduled, Preventive, Emergency)
  - 📚 Training (Safety, Technical, Certification)
  - 🚚 Logistics (Material Delivery, Equipment Transport)

- **Recurring Events**: 
  - Daily, weekly, monthly, quarterly, and custom patterns
  - Advanced recurrence rules with exceptions
  - Series management with bulk updates
  - Automatic adaptation to project schedule changes

- **Event Categories**: 
  - Color-coded categorization system
  - Project and priority-based organization
  - Custom categories for different teams
  - Visual calendar organization and filtering

- **Location Management**: 
  - GPS coordinates with mapping integration
  - Site addresses with automatic geocoding
  - Location-based notifications and check-ins
  - Weather monitoring for outdoor events
  - Travel time calculation and optimization

- **Attendee Management**: 
  - Advanced invitation system with RSVP tracking
  - Availability checking across all attendees
  - Automatic conflict detection and suggestions
  - Guest attendee support (external clients/vendors)
  - Role-based attendee permissions

### 🔄 Advanced Scheduling Features
- **AI-Powered Conflict Detection**: 
  - Real-time scheduling conflict identification
  - Smart resolution suggestions
  - Resource double-booking prevention
  - Critical path impact analysis

- **Resource Scheduling**: 
  - Equipment availability integration
  - Personnel scheduling with skill matching
  - Tool and material coordination
  - Vehicle and transportation scheduling

- **Multi-Timezone Support**: 
  - Automatic timezone conversion
  - Global team coordination
  - Client timezone preferences
  - Daylight saving time handling

- **External Calendar Sync**: 
  - Google Calendar bidirectional sync
  - Microsoft Outlook integration
  - iCal export/import capabilities
  - Real-time synchronization
  - Conflict resolution across platforms

- **Mobile Optimization**: 
  - Full-featured mobile calendar interface
  - Offline access and synchronization
  - Push notifications and alerts
  - GPS-based check-in and attendance
  - Photo and document attachments

### 📊 Analytics & Advanced Reporting
- **Schedule Analytics**: 
  - Team utilization metrics and trends
  - Project timeline adherence tracking
  - Milestone completion rates
  - Resource efficiency analysis

- **Performance Metrics**: 
  - Meeting efficiency scores
  - Deadline adherence statistics
  - Schedule optimization recommendations
  - Time-to-completion analysis

- **Resource Utilization**: 
  - Equipment usage patterns
  - Personnel scheduling efficiency
  - Optimal resource allocation suggestions
  - Cost-benefit analysis of scheduling decisions

- **Project Timeline Analysis**: 
  - Critical path analysis and visualization
  - Schedule risk assessment
  - Dependency impact analysis
  - Predictive scheduling insights

### 🌦️ Weather & Environmental Integration
- **Weather-Aware Scheduling**: 
  - Real-time weather monitoring
  - Automatic weather alerts for outdoor events
  - Rescheduling suggestions based on forecasts
  - Historical weather data for planning

- **Environmental Factors**: 
  - Daylight hours optimization
  - Seasonal scheduling considerations
  - Safety condition monitoring
  - Equipment performance factors

## 📋 Get All Calendar Events

**GET** `/api/v1/calendar/events`

**⚡ Real-Time Updates**: This endpoint supports live data updates. All changes to calendar events are instantly broadcast to connected users via WebSocket.

Retrieve calendar events with comprehensive filtering, analytics, and smart scheduling options. All returned data is real-time synchronized across all connected users.

### 🔍 Query Parameters

#### Basic Filtering
- `projectId` (Guid): Filter by specific project
- `userId` (Guid): Filter by event creator or attendee
- `eventType` (string): Filter by event type
  - Options: `Meeting`, `ProjectMilestone`, `Deadline`, `Inspection`, `ClientVisit`, `Maintenance`, `Training`, `Logistics`
- `startDate` (DateTime): Filter events from date (ISO format: `2025-07-01T00:00:00Z`)
- `endDate` (DateTime): Filter events to date (ISO format: `2025-07-31T23:59:59Z`)
- `category` (string): Filter by event category
- `status` (string): Filter by event status
  - Options: `Scheduled`, `InProgress`, `Completed`, `Cancelled`, `Postponed`, `Rescheduled`

#### Advanced Filtering
- `priority` (string): Filter by priority
  - Options: `Low`, `Medium`, `High`, `Critical`, `Urgent`
- `location` (string): Filter by location or site
- `hasConflicts` (bool): Filter events with scheduling conflicts
- `isRecurring` (bool): Filter recurring events
- `attendeeId` (Guid): Filter by specific attendee
- `includePrivate` (bool): Include private events (Admin/Manager only)
- `weatherDependent` (bool): Filter weather-dependent events
- `requiresApproval` (bool): Filter events requiring approval

#### Sorting & Pagination
- `sortBy` (string): Sort field
  - Options: `startDateTime`, `priority`, `eventType`, `createdAt`, `updatedAt`, `attendeeCount`
- `sortDirection` (string): Sort direction (`asc`, `desc`)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)

#### Analytics & Reporting
- `includeAnalytics` (bool): Include analytics summary
- `includeConflicts` (bool): Include conflict analysis
- `includeWeather` (bool): Include weather information
- `timeZone` (string): Response timezone (default: UTC)

### ✅ Success Response (200)
```json
{
  "success": true,
  "message": "Calendar events retrieved successfully",
  "data": {
    "events": [
      {
        "eventId": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Solar Panel Installation - Phase 2",
        "description": "Installation of 24 solar panels on east-facing roof section with safety protocols",
        "startDateTime": "2025-07-15T08:00:00Z",
        "endDateTime": "2025-07-15T16:00:00Z",
        "eventType": "ProjectMilestone",
        "status": "Scheduled",
        "priority": "High",
        "category": "Installation",
        "location": "123 Solar Street, Austin, TX 78701",
        "gpsCoordinates": {
          "latitude": 30.2672,
          "longitude": -97.7431
        },
        "isAllDay": false,
        "isRecurring": false,
        "weatherDependent": true,
        "requiresApproval": false,
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "taskId": "task-123-id",
        "taskName": "Phase 2 Installation",
        "milestoneId": "milestone-456-id",
        "milestoneName": "East Section Complete",
        "createdByUserId": "789e0123-e89b-12d3-a456-426614174002",
        "createdByName": "Sarah Manager",
        "createdByRole": "ProjectManager",
        "assignedToUserId": "user1-id",
        "assignedToName": "John Technician",
        "attendees": [
          {
            "userId": "user1-id",
            "name": "John Technician",
            "email": "john.tech@company.com",
            "role": "Lead Installer",
            "rsvpStatus": "Accepted",
            "isRequired": true,
            "respondedAt": "2025-07-11T10:30:00Z"
          },
          {
            "userId": "user2-id",
            "name": "Mike Electrician",
            "email": "mike.elec@company.com",
            "role": "Electrical Specialist",
            "rsvpStatus": "Pending",
            "isRequired": true,
            "respondedAt": null
          }
        ],
        "resources": [
          {
            "resourceId": "crane-001",
            "resourceName": "Mobile Crane - 15T",
            "resourceType": "Equipment",
            "isAvailable": true,
            "bookingStatus": "Reserved"
          }
        ],
        "reminderMinutes": 60,
        "color": "#3788d8",
        "isPrivate": false,
        "hasConflicts": false,
        "conflictSeverity": null,
        "notes": "Weather backup date: July 16th. Safety briefing at 7:45 AM.",
        "weatherForecast": {
          "condition": "Partly Cloudy",
          "temperature": 82,
          "humidity": 65,
          "windSpeed": 8,
          "precipitation": 10,
          "uvIndex": 7,
          "recommendation": "Suitable for outdoor work"
        },
        "attachments": [
          {
            "id": "att1-id",
            "fileName": "installation_plan_v2.pdf",
            "fileType": "application/pdf",
            "fileSize": 2048576,
            "uploadedAt": "2025-07-10T14:30:00Z",
            "uploadedBy": "Sarah Manager"
          }
        ],
        "estimatedDuration": "8h 0m",
        "actualDuration": null,
        "completionPercentage": 0,
        "criticalPath": true,
        "dependencies": ["material-delivery-event-id"],
        "tags": ["installation", "phase2", "rooftop", "safety-critical"],
        "createdAt": "2025-07-10T14:30:00Z",
        "updatedAt": "2025-07-12T09:15:00Z",
        "lastSyncedAt": "2025-07-12T09:15:00Z"
      }
    ],
    "pagination": {
      "totalCount": 145,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 8,
      "hasPreviousPage": false,
      "hasNextPage": true
    },
    "analytics": {
      "totalEvents": 145,
      "upcomingEvents": 32,
      "overdue": 3,
      "inProgress": 8,
      "completed": 102,
      "conflicts": 2,
      "highPriority": 18,
      "criticalPath": 12,
      "weatherDependent": 28,
      "averageAttendeeCount": 3.2,
      "resourceUtilization": 78.5,
      "onTimeCompletion": 94.2
    },
    "conflicts": {
      "totalConflicts": 2,
      "criticalConflicts": 1,
      "resolutionRequired": 1,
      "autoResolvable": 1
    },
    "timeZone": "UTC",
    "generatedAt": "2025-07-05T15:30:00Z",
    "realTimeSync": {
      "isLiveDataEnabled": true,
      "lastSyncTimestamp": "2025-07-05T15:30:00Z",
      "connectedUsers": 15,
      "syncStatus": "Active"
    }
  },
  "errors": []
}
```

## 🔍 Get Calendar Event by ID

**GET** `/api/v1/calendar/events/{id}`

**⚡ Real-Time Updates**: This endpoint supports live data updates. Changes to this event are instantly broadcast to all attendees and relevant users.

Retrieve comprehensive information about a specific calendar event including detailed analytics, dependencies, and real-time status updates.

### 📝 Path Parameters
- `id` (Guid): Calendar event ID

### 🔍 Query Parameters
- `includeAnalytics` (bool): Include performance analytics (default: false)
- `includeWeather` (bool): Include weather forecast data (default: false)
- `includeDependencies` (bool): Include event dependencies and relationships (default: false)
- `timeZone` (string): Response timezone preference (default: UTC)

### ✅ Success Response (200)
```json
{
  "success": true,
  "message": "Calendar event retrieved successfully",
  "data": {
    "eventId": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Panel Installation - Phase 2",
    "description": "Installation of 24 solar panels on east-facing roof section with comprehensive quality inspections and safety protocols",
    "startDateTime": "2025-07-15T08:00:00Z",
    "endDateTime": "2025-07-15T16:00:00Z",
    "eventType": "ProjectMilestone",
    "status": "Scheduled",
    "priority": "High",
    "category": "Installation",
    "location": "123 Solar Street, Austin, TX 78701",
    "gpsCoordinates": {
      "latitude": 30.2672,
      "longitude": -97.7431
    },
    "isAllDay": false,
    "isRecurring": false,
    "recurrencePattern": null,
    "recurrenceEndDate": null,
    "weatherDependent": true,
    "requiresApproval": false,
    "approvalStatus": null,
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "projectStatus": "InProgress",
    "taskId": "task-123-id",
    "taskName": "Phase 2 Installation",
    "taskProgress": 65,
    "milestoneId": "milestone-456-id",
    "milestoneName": "East Section Complete",
    "milestoneProgress": 45,
    "createdByUserId": "789e0123-e89b-12d3-a456-426614174002",
    "createdByName": "Sarah Manager",
    "createdByRole": "ProjectManager",
    "assignedToUserId": "user1-id",
    "assignedToName": "John Technician",
    "assignedToRole": "Lead Installer",
    "attendees": [
      {
        "userId": "user1-id",
        "name": "John Technician",
        "email": "john.tech@company.com",
        "phone": "+1-555-0123",
        "role": "Lead Installer",
        "rsvpStatus": "Accepted",
        "isRequired": true,
        "respondedAt": "2025-07-11T10:30:00Z",
        "lastActivity": "2025-07-12T14:20:00Z",
        "availability": "Available",
        "skills": ["Solar Installation", "Electrical Work", "Safety Management"]
      },
      {
        "userId": "user2-id",
        "name": "Mike Electrician",
        "email": "mike.elec@company.com",
        "phone": "+1-555-0124",
        "role": "Electrical Specialist",
        "rsvpStatus": "Pending",
        "isRequired": true,
        "respondedAt": null,
        "lastActivity": "2025-07-10T16:45:00Z",
        "availability": "Busy",
        "skills": ["Electrical Systems", "Grid Connection", "Code Compliance"]
      }
    ],
    "resources": [
      {
        "resourceId": "crane-001",
        "resourceName": "Mobile Crane - 15T",
        "resourceType": "Equipment",
        "isAvailable": true,
        "bookingStatus": "Reserved",
        "location": "Equipment Yard A",
        "operator": "Tom Crane Operator",
        "hourlyRate": 125.00,
        "notes": "Recently serviced, ready for operation"
      },
      {
        "resourceId": "truck-005",
        "resourceName": "Material Transport Truck",
        "resourceType": "Vehicle",
        "isAvailable": true,
        "bookingStatus": "Confirmed",
        "location": "Site Parking",
        "operator": "Lisa Driver",
        "hourlyRate": 75.00,
        "notes": "Loaded with installation materials"
      }
    ],
    "reminderMinutes": 60,
    "color": "#3788d8",
    "isPrivate": false,
    "hasConflicts": false,
    "conflictDetails": [],
    "conflictSeverity": null,
    "notes": "Weather backup date: July 16th. Safety briefing mandatory at 7:45 AM. All PPE required.",
    "meetingUrl": null,
    "dialInNumber": null,
    "requirements": [
      "All safety equipment mandatory and inspected",
      "Installation materials pre-positioned and verified",
      "Quality inspector on-site at 2:00 PM",
      "Weather conditions must be suitable (no rain, wind < 25 mph)",
      "Electrical safety lockout/tagout procedures in place"
    ],
    "checklist": [
      {
        "id": "check-001",
        "item": "Safety equipment inspection",
        "isCompleted": false,
        "assignedTo": "John Technician",
        "dueTime": "2025-07-15T07:45:00Z"
      },
      {
        "id": "check-002",
        "item": "Material inventory verification",
        "isCompleted": false,
        "assignedTo": "Mike Electrician",
        "dueTime": "2025-07-15T08:00:00Z"
      }
    ],
    "weatherForecast": {
      "condition": "Partly Cloudy",
      "temperature": 82,
      "humidity": 65,
      "windSpeed": 8,
      "windDirection": "SW",
      "precipitation": 10,
      "uvIndex": 7,
      "visibility": 10,
      "recommendation": "Suitable for outdoor work",
      "alerts": [],
      "hourlyForecast": [
        {
          "hour": "08:00",
          "temperature": 78,
          "condition": "Clear",
          "windSpeed": 6
        }
      ]
    },
    "attachments": [
      {
        "id": "att1-id",
        "fileName": "installation_plan_v2.pdf",
        "fileType": "application/pdf",
        "fileSize": 2048576,
        "description": "Detailed installation procedures and safety protocols",
        "uploadedAt": "2025-07-10T14:30:00Z",
        "uploadedBy": "Sarah Manager",
        "downloadUrl": "/api/v1/files/att1-id/download",
        "thumbnailUrl": "/api/v1/files/att1-id/thumbnail"
      },
      {
        "id": "att2-id",
        "fileName": "site_survey_photos.zip",
        "fileType": "application/zip",
        "fileSize": 15728640,
        "description": "Site survey photographs and measurements",
        "uploadedAt": "2025-07-09T11:15:00Z",
        "uploadedBy": "John Technician",
        "downloadUrl": "/api/v1/files/att2-id/download",
        "thumbnailUrl": null
      }
    ],
    "relatedEvents": [
      {
        "eventId": "related-event-001",
        "title": "Material Delivery - Phase 2",
        "startDateTime": "2025-07-14T14:00:00Z",
        "endDateTime": "2025-07-14T16:00:00Z",
        "relationship": "Prerequisite",
        "status": "Completed"
      },
      {
        "eventId": "related-event-002",
        "title": "Quality Inspection - Phase 2",
        "startDateTime": "2025-07-15T14:00:00Z",
        "endDateTime": "2025-07-15T15:00:00Z",
        "relationship": "Dependent",
        "status": "Scheduled"
      }
    ],
    "dependencies": [
      {
        "type": "Prerequisite",
        "eventId": "material-delivery-event",
        "eventTitle": "Material Delivery",
        "status": "Completed",
        "criticalPath": true
      },
      {
        "type": "Resource",
        "resourceId": "crane-001",
        "resourceName": "Mobile Crane",
        "availability": "Reserved",
        "criticalPath": false
      }
    ],
    "analytics": {
      "estimatedDuration": "8h 0m",
      "actualDuration": null,
      "completionPercentage": 0,
      "onSchedule": true,
      "riskLevel": "Low",
      "criticalPath": true,
      "bufferTime": "2h 0m",
      "costEstimate": 2450.00,
      "actualCost": null,
      "productivity": {
        "plannedOutput": "24 panels installed",
        "actualOutput": null,
        "efficiency": null
      }
    },
    "permissions": {
      "canEdit": true,
      "canDelete": true,
      "canInviteAttendees": true,
      "canViewPrivateNotes": true,
      "canManageResources": true
    },
    "auditTrail": [
      {
        "action": "Created",
        "userId": "789e0123-e89b-12d3-a456-426614174002",
        "userName": "Sarah Manager",
        "timestamp": "2025-07-10T14:30:00Z",
        "details": "Event created with initial scheduling"
      },
      {
        "action": "Updated",
        "userId": "789e0123-e89b-12d3-a456-426614174002",
        "userName": "Sarah Manager",
        "timestamp": "2025-07-12T09:15:00Z",
        "details": "Added weather dependency and updated requirements"
      }
    ],
    "tags": ["installation", "phase2", "rooftop", "safety-critical", "weather-dependent"],
    "customFields": {
      "permitNumber": "SOLAR-2025-0156",
      "inspectorId": "INS-001",
      "contractorLicense": "ELEC-TX-12345"
    },
    "createdAt": "2025-07-10T14:30:00Z",
    "updatedAt": "2025-07-12T09:15:00Z",
    "lastSyncedAt": "2025-07-12T09:15:00Z",
    "timeZone": "UTC"
  },
  "errors": []
}
```

## 📝 Create Calendar Event

**POST** `/api/v1/calendar/events`

**⚡ Real-Time Updates**: When a calendar event is created, all relevant users receive instant notifications via WebSocket. The new event is immediately visible to all connected users without page refresh.

Create a new calendar event with comprehensive scheduling options. The created event will be instantly broadcast to all attendees, project team members, and relevant stakeholders.

**Request Body**:
```json
{
  "title": "Client Final Inspection",
  "description": "Final walkthrough and inspection with client before project completion",
  "startDateTime": "2025-07-25T10:00:00Z",
  "endDateTime": "2025-07-25T12:00:00Z",
  "eventType": "ClientVisit",
  "priority": "High",
  "location": "456 Green Energy Lane, Austin, TX",
  "gpsCoordinates": {
    "latitude": 30.2672,
    "longitude": -97.7431
  },
  "isAllDay": false,
  "isRecurring": false,
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "taskId": "task-final-inspection-id",
  "assignedToUserId": "user1-id",
  "attendees": [
    {
      "userId": "user1-id",
      "isRequired": true
    },
    {
      "email": "client@example.com",
      "name": "John Client",
      "isRequired": true
    }
  ],
  "reminderMinutes": 120,
  "color": "#ff6b35",
  "isPrivate": false,
  "notes": "Bring completed documentation and warranty information",
  "requirements": [
    "All installation work completed",
    "Documentation prepared",
    "System operational test completed"
  ]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Calendar event created successfully",
  "data": {
    "eventId": "new-event-id",
    "title": "Client Final Inspection",
    "startDateTime": "2025-07-25T10:00:00Z",
    "endDateTime": "2025-07-25T12:00:00Z",
    "eventType": "ClientVisit",
    "status": "Scheduled",
    "projectName": "Solar Installation Project Alpha",
    "assignedToName": "John Technician",
    "attendeeCount": 2,
    "hasConflicts": false,
    "createdAt": "2025-07-15T16:45:00Z"
  },
  "realTimeUpdate": {
    "broadcastSent": true,
    "affectedUsers": ["user1-id", "user2-id", "admin-id"],
    "notificationsSent": 2,
    "updateType": "CalendarEventCreated"
  },
  "errors": []
}
```

**📡 Real-Time Broadcast**: This endpoint automatically sends live updates to:
- All project team members
- Event attendees
- Project managers and administrators
- Users subscribed to the project calendar

## 🔄 Update Calendar Event

**PUT** `/api/v1/calendar/events/{id}`

**🔒 Requires**: Admin, Manager, or event creator
**⚡ Real-Time Updates**: When an event is updated, all attendees and relevant users receive instant notifications and see changes immediately via WebSocket.

Update an existing calendar event with validation and conflict checking. All changes are instantly broadcast to relevant users.

**Path Parameters**:
- `id` (Guid): Calendar event ID

**Request Body**:
```json
{
  "title": "Client Final Inspection (Updated)",
  "description": "Final walkthrough, inspection, and system demonstration with client",
  "startDateTime": "2025-07-25T09:00:00Z",
  "endDateTime": "2025-07-25T11:30:00Z",
  "priority": "Critical",
  "notes": "Bring completed documentation, warranty information, and system monitoring setup instructions"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar event updated successfully",
  "data": {
    "eventId": "new-event-id",
    "title": "Client Final Inspection (Updated)",
    "startDateTime": "2025-07-25T09:00:00Z",
    "endDateTime": "2025-07-25T11:30:00Z",
    "hasConflicts": false,
    "updatedAt": "2025-07-15T17:30:00Z",
    "notificationsSent": 2,
    "version": 2
  },
  "realTimeUpdate": {
    "broadcastSent": true,
    "affectedUsers": ["user1-id", "user2-id", "client@example.com"],
    "conflictsResolved": 0,
    "updateType": "CalendarEventUpdated",
    "changedFields": ["title", "startDateTime", "endDateTime"]
  },
  "errors": []
}
```

**📡 Real-Time Broadcast**: This endpoint automatically sends live updates to:
- All event attendees with immediate calendar refresh
- Project team members with conflict checks
- Resource managers for availability updates
- Mobile apps with push notifications

## 🗑️ Delete Calendar Event

**DELETE** `/api/v1/calendar/events/{id}`

**🔒 Required Roles**: Admin, Manager, or event creator
**⚡ Real-Time Updates**: When an event is deleted, all attendees and relevant users receive instant cancellation notifications via WebSocket.

Delete a calendar event with optional cancellation notifications. The deletion is immediately broadcast to all relevant users.

**Path Parameters**:
- `id` (Guid): Calendar event ID

**Query Parameters**:
- `sendCancellation` (bool): Send cancellation notifications to attendees (default: true)
- `reason` (string): Cancellation reason for notifications

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar event deleted successfully",
  "data": {
    "eventId": "new-event-id",
    "cancellationsSent": 2,
    "deletedAt": "2025-07-15T18:00:00Z"
  },
  "realTimeUpdate": {
    "broadcastSent": true,
    "affectedUsers": ["user1-id", "user2-id", "client@example.com"],
    "cancellationNotificationsSent": 2,
    "updateType": "CalendarEventDeleted",
    "relatedEventsUpdated": 1
  },
  "errors": []
}
```

**📡 Real-Time Broadcast**: This endpoint automatically sends live updates to:
- All event attendees with cancellation notifications
- Project team members with calendar removal
- Resource managers for availability release
- Dependent events with automatic rescheduling alerts

## 📅 Calendar Views & Analytics

### Get Calendar View

**GET** `/api/v1/calendar/view`

**⚡ Real-Time Updates**: Calendar views automatically refresh with live data as events are created, updated, or deleted by other users.

Get calendar events in various view formats (day, week, month, year) with real-time synchronization.

**Query Parameters**:
- `viewType` (string): View type (day, week, month, year)
- `date` (DateTime): Focus date for the view
- `projectId` (Guid): Filter by project (optional)
- `userId` (Guid): Filter by user (optional)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar view retrieved successfully",
  "data": {
    "viewType": "week",
    "startDate": "2025-07-14T00:00:00Z",
    "endDate": "2025-07-20T23:59:59Z",
    "events": [
      {
        "eventId": "event1-id",
        "title": "Material Delivery",
        "startDateTime": "2025-07-15T08:00:00Z",
        "endDateTime": "2025-07-15T10:00:00Z",
        "eventType": "Logistics",
        "priority": "Medium",
        "color": "#4caf50"
      }
    ],
    "conflicts": [
      {
        "date": "2025-07-15",
        "conflictCount": 1,
        "details": "John Technician has overlapping events"
      }
    ],
    "summary": {
      "totalEvents": 8,
      "workDays": 5,
      "busyHours": 32,
      "availableHours": 8
    }
  },
  "errors": []
}
```

### Get Event Conflicts

**GET** `/api/v1/calendar/conflicts`

**🔒 Required Roles**: Admin, Manager

Identify and analyze scheduling conflicts across the organization.

**Query Parameters**:
- `startDate` (DateTime): Analysis start date
- `endDate` (DateTime): Analysis end date
- `projectId` (Guid): Filter by project (optional)
- `severity` (string): Filter by conflict severity (Low, Medium, High, Critical)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar conflicts analyzed successfully",
  "data": {
    "analysisperiod": {
      "startDate": "2025-07-01T00:00:00Z",
      "endDate": "2025-07-31T23:59:59Z"
    },
    "conflicts": [
      {
        "conflictId": "conflict1-id",
        "date": "2025-07-15T14:00:00Z",
        "severity": "High",
        "type": "Resource Conflict",
        "description": "John Technician scheduled for two overlapping events",
        "affectedEvents": [
          {
            "eventId": "event1-id",
            "title": "Solar Installation",
            "startDateTime": "2025-07-15T13:00:00Z",
            "endDateTime": "2025-07-15T17:00:00Z"
          },
          {
            "eventId": "event2-id",
            "title": "Client Meeting",
            "startDateTime": "2025-07-15T14:00:00Z",
            "endDateTime": "2025-07-15T15:00:00Z"
          }
        ],
        "resolution": "Reschedule client meeting to July 16th",
        "priority": "High"
      }
    ],
    "summary": {
      "totalConflicts": 3,
      "criticalConflicts": 1,
      "highPriorityConflicts": 2,
      "resolutionRequired": 2,
      "autoResolvable": 1
    }
  },
  "errors": []
}
```

## 🔄 RSVP & Attendance Management

### Update RSVP Status

**POST** `/api/v1/calendar/events/{id}/rsvp`

**🔒 Requires**: Event attendee or higher authority
**⚡ Real-Time Updates**: RSVP status changes are instantly broadcast to the event creator and all other attendees.

Update RSVP status for a calendar event. All attendees will see the updated attendance status immediately.

**Path Parameters**:
- `id` (Guid): Calendar event ID

**Request Body**:
```json
{
  "rsvpStatus": "Accepted",
  "comments": "Looking forward to the final inspection!"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "RSVP updated successfully",
  "data": {
    "eventId": "event-id",
    "userId": "user-id",
    "rsvpStatus": "Accepted",
    "respondedAt": "2025-07-15T19:30:00Z"
  },
  "realTimeUpdate": {
    "broadcastSent": true,
    "affectedUsers": ["event-creator-id", "other-attendees"],
    "updateType": "RsvpStatusUpdated",
    "attendeeCountUpdated": true
  },
  "errors": []
}
```

**📡 Real-Time Broadcast**: RSVP changes instantly update:
- Event organizer dashboard
- Attendee lists for all participants
- Resource planning calculations
- Meeting room capacity indicators

### Mark Attendance

**POST** `/api/v1/calendar/events/{id}/attendance`

**🔒 Requires**: Event attendee or higher authority
**⚡ Real-Time Updates**: Attendance marking is instantly broadcast to event organizers and team managers for real-time tracking.

Mark attendance for a calendar event. Attendance status is immediately visible to organizers and managers.

**Path Parameters**:
- `id` (Guid): Calendar event ID

**Request Body**:
```json
{
  "attendanceStatus": "Present",
  "arrivalTime": "2025-07-25T09:05:00Z",
  "notes": "Arrived slightly late due to traffic"
}
```

## ⚡ Real-Time Live Updates & WebSocket Integration

### 🌐 WebSocket Connection Setup

**WebSocket Endpoint**: `wss://api.solarprojects.com/hubs/calendar`

All calendar API endpoints support real-time live updates through SignalR WebSocket connections. When any user updates calendar data, all connected users receive instant notifications and data updates.

#### Connection Authentication
```javascript
// JavaScript/TypeScript WebSocket Connection
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hubs/calendar", {
        accessTokenFactory: () => getJwtToken()
    })
    .withAutomaticReconnect()
    .build();

await connection.start();
```

#### Connection Groups & Subscriptions
Users are automatically subscribed to relevant update groups based on their permissions:
- **Project-specific groups**: `project_${projectId}`
- **User-specific groups**: `user_${userId}`
- **Role-based groups**: `role_admin`, `role_manager`, `role_user`
- **Global calendar group**: `calendar_all` (admin/manager only)

### 📡 Real-Time Event Types

#### Calendar Event Updates
```javascript
// Listen for calendar event updates
connection.on("CalendarEventUpdated", (data) => {
    console.log("Event updated:", data);
    // Update UI immediately
    updateEventInCalendar(data.eventId, data.updatedData);
});

connection.on("CalendarEventCreated", (data) => {
    console.log("New event created:", data);
    // Add new event to calendar UI
    addEventToCalendar(data.event);
});

connection.on("CalendarEventDeleted", (data) => {
    console.log("Event deleted:", data);
    // Remove event from calendar UI
    removeEventFromCalendar(data.eventId);
});
```

#### Resource & Scheduling Updates
```javascript
// Resource availability changes
connection.on("ResourceAvailabilityChanged", (data) => {
    console.log("Resource availability updated:", data);
    updateResourceStatus(data.resourceId, data.availability);
});

// Scheduling conflict notifications
connection.on("SchedulingConflictDetected", (data) => {
    console.log("Scheduling conflict detected:", data);
    showConflictNotification(data.conflictDetails);
});

// RSVP status changes
connection.on("RsvpStatusUpdated", (data) => {
    console.log("RSVP status changed:", data);
    updateAttendeeStatus(data.eventId, data.userId, data.rsvpStatus);
});
```

#### Project & Task Integration Updates
```javascript
// Project milestone updates
connection.on("ProjectMilestoneUpdated", (data) => {
    console.log("Project milestone updated:", data);
    updateMilestoneInCalendar(data.milestoneId, data.progress);
});

// Task completion notifications
connection.on("TaskCompleted", (data) => {
    console.log("Task completed:", data);
    updateRelatedEvents(data.taskId, "completed");
});

// Weather alerts for outdoor events
connection.on("WeatherAlertTriggered", (data) => {
    console.log("Weather alert:", data);
    showWeatherAlert(data.eventId, data.weatherData);
});
```

### 🔄 Live Update Response Format

All real-time updates follow a consistent format:

```json
{
  "updateType": "CalendarEventUpdated",
  "timestamp": "2025-07-05T15:30:00Z",
  "userId": "user-who-made-change-id",
  "userName": "Sarah Manager",
  "projectId": "project-id",
  "eventId": "event-id",
  "changeType": "Modified",
  "updatedFields": ["startDateTime", "attendees", "priority"],
  "data": {
    "eventId": "event-id",
    "title": "Updated Event Title",
    "startDateTime": "2025-07-15T09:00:00Z",
    "endDateTime": "2025-07-15T11:30:00Z",
    "updatedAt": "2025-07-05T15:30:00Z",
    "version": 3
  },
  "metadata": {
    "affectedUsers": ["user1-id", "user2-id"],
    "conflictsResolved": 1,
    "notificationsSent": 2
  }
}
```

### 🎯 Subscription Management

#### Subscribe to Specific Events
```javascript
// Subscribe to specific project calendar updates
await connection.invoke("JoinProjectGroup", projectId);

// Subscribe to specific event updates
await connection.invoke("JoinEventGroup", eventId);

// Subscribe to resource updates
await connection.invoke("JoinResourceGroup", resourceId);
```

#### Unsubscribe from Updates
```javascript
// Unsubscribe from project updates
await connection.invoke("LeaveProjectGroup", projectId);

// Unsubscribe from event updates
await connection.invoke("LeaveEventGroup", eventId);
```

### 📱 Mobile & Offline Support

#### Offline Queue Management
```javascript
// Queue updates when offline
connection.onreconnected(() => {
    // Request missed updates since last connection
    connection.invoke("GetMissedUpdates", lastUpdateTimestamp);
});

// Handle missed updates
connection.on("MissedUpdatesReceived", (updates) => {
    updates.forEach(update => {
        applyUpdateToLocalCalendar(update);
    });
});
```

#### Background Sync
```javascript
// Background synchronization for mobile apps
connection.on("BackgroundSyncRequired", (data) => {
    // Trigger background sync process
    syncCalendarData(data.lastSyncTimestamp);
});
```

### 🔔 Push Notifications Integration

#### Real-Time Notification Events
```javascript
// Push notification triggers
connection.on("PushNotificationTriggered", (data) => {
    if (data.notificationType === "EventReminder") {
        showEventReminder(data.eventId, data.reminderTime);
    } else if (data.notificationType === "ConflictAlert") {
        showConflictAlert(data.conflictDetails);
    } else if (data.notificationType === "WeatherWarning") {
        showWeatherWarning(data.eventId, data.weatherAlert);
    }
});
```

### 🛡️ Security & Permission Filtering

Real-time updates are automatically filtered based on user permissions:

```json
{
  "securityContext": {
    "userId": "current-user-id",
    "userRole": "ProjectManager",
    "projectAccess": ["project1-id", "project2-id"],
    "personalEventsOnly": false,
    "canViewPrivateEvents": false
  },
  "filteredUpdate": {
    "visibleFields": ["title", "startDateTime", "endDateTime", "status"],
    "hiddenFields": ["privateNotes", "internalComments"],
    "accessLevel": "ProjectSpecific"
  }
}
```

### 📊 Live Analytics Updates

```javascript
// Real-time analytics updates
connection.on("CalendarAnalyticsUpdated", (data) => {
    updateDashboardMetrics({
        totalEvents: data.totalEvents,
        upcomingEvents: data.upcomingEvents,
        conflicts: data.conflicts,
        resourceUtilization: data.resourceUtilization
    });
});

// Real-time performance metrics
connection.on("PerformanceMetricsUpdated", (data) => {
    updatePerformanceCharts(data.metrics);
});
```

### 🔧 Error Handling & Reconnection

```javascript
// Connection error handling
connection.onclose((error) => {
    console.log("Connection closed:", error);
    // Implement exponential backoff retry
    setTimeout(() => connection.start(), 5000);
});

// Reconnection handling
connection.onreconnecting((error) => {
    console.log("Reconnecting:", error);
    showReconnectingIndicator();
});

connection.onreconnected((connectionId) => {
    console.log("Reconnected:", connectionId);
    hideReconnectingIndicator();
    // Sync any missed updates
    syncMissedUpdates();
});
```

### 🎮 Interactive Collaborative Features

#### Real-Time Collaborative Editing
```javascript
// Multiple users editing the same event
connection.on("EventBeingEdited", (data) => {
    showUserEditingIndicator(data.eventId, data.userId, data.userName);
});

connection.on("EventEditingCompleted", (data) => {
    hideUserEditingIndicator(data.eventId, data.userId);
});

// Live cursor positions in form fields
connection.on("FieldFocusChanged", (data) => {
    showUserFieldFocus(data.eventId, data.fieldName, data.userId);
});
```

#### Live Attendance Tracking
```javascript
// Real-time attendance updates
connection.on("AttendanceMarked", (data) => {
    updateAttendanceStatus(data.eventId, data.userId, data.status);
    showAttendanceNotification(data.userName, data.status);
});

// Live event status updates
connection.on("EventStatusChanged", (data) => {
    updateEventStatus(data.eventId, data.newStatus);
    if (data.newStatus === "InProgress") {
        startLiveEventTracking(data.eventId);
    }
});
```

### 📈 Implementation Best Practices

#### Client-Side Implementation
```javascript
// Efficient update handling
const updateQueue = new Map();
const batchUpdateInterval = 500; // 500ms batching

connection.on("CalendarEventUpdated", (data) => {
    // Batch updates to prevent UI flooding
    updateQueue.set(data.eventId, data);
    
    setTimeout(() => {
        if (updateQueue.has(data.eventId)) {
            const latestUpdate = updateQueue.get(data.eventId);
            applyUpdateToUI(latestUpdate);
            updateQueue.delete(data.eventId);
        }
    }, batchUpdateInterval);
});
```

#### Conflict Resolution
```javascript
// Handle concurrent modifications
connection.on("ConcurrentModificationDetected", (data) => {
    showConflictResolutionDialog({
        localChanges: data.localVersion,
        serverChanges: data.serverVersion,
        eventId: data.eventId
    });
});

// Auto-merge non-conflicting changes
connection.on("AutoMergeCompleted", (data) => {
    applyMergedChanges(data.eventId, data.mergedData);
    showMergeNotification("Changes automatically merged");
});
```

---

## 🌐 Comprehensive Real-Time Live Update Support

**✨ Every Calendar API Endpoint Supports Live Data Updates**

All calendar endpoints (`GET`, `POST`, `PUT`, `DELETE`) are designed with **real-time live updates** as a core feature:

### � Instant Data Synchronization
- **Creating Events**: `POST /api/v1/calendar/events` → All project members see new events immediately
- **Updating Events**: `PUT /api/v1/calendar/events/{id}` → Changes appear instantly across all user sessions
- **Deleting Events**: `DELETE /api/v1/calendar/events/{id}` → Removal is broadcast immediately with notifications
- **RSVP Updates**: `POST /api/v1/calendar/events/{id}/rsvp` → Attendance status syncs live to all attendees
- **Calendar Views**: `GET /api/v1/calendar/view` → Views refresh automatically as data changes
- **Conflict Detection**: Real-time alerts prevent scheduling conflicts across the entire organization

### 🎯 Live Update Benefits for Users
1. **No Page Refresh Required**: All changes appear automatically without manual refresh
2. **Instant Collaboration**: Multiple users can work on schedules simultaneously with conflict prevention
3. **Real-Time Notifications**: Push notifications and browser alerts for important schedule changes
4. **Cross-Device Sync**: Changes on mobile instantly appear on desktop and vice versa
5. **Live Status Updates**: RSVP changes, attendance marking, and resource availability update instantly
6. **Automatic Conflict Resolution**: Real-time conflict detection prevents double-booking issues

### 🔄 Technical Implementation
- **SignalR WebSocket**: Persistent connections for instant data push
- **Permission-Based Broadcasting**: Users only receive updates for events they can access
- **Efficient Data Sync**: Only changed data is transmitted to minimize bandwidth
- **Automatic Reconnection**: Seamless reconnection handling for mobile and unstable connections
- **Cross-Platform Support**: Works seamlessly across all web browsers, mobile apps, and desktop applications

*�🔄 All calendar API endpoints automatically broadcast real-time updates to subscribed users. No additional configuration required for live data synchronization.*

---

## 🎯 Summary: Calendar as Central Project Coordination Hub

The Calendar Events system serves as the **intelligent scheduling and coordination backbone** for solar installation projects with **real-time live data synchronization**, providing:

### 📊 Unified Project Intelligence with Live Updates
- **Master Timeline Coordination**: Single source of truth for all project dates with instant updates across all users
- **AI-Powered Resource Optimization**: Intelligent scheduling with real-time conflict detection and automatic resolution
- **Client Experience Management**: Professional scheduling interface with live updates and automated communications
- **Milestone & Progress Integration**: Seamless synchronization with instant progress updates and milestone notifications

### 🔄 Advanced Real-Time Scheduling Intelligence
- **Live Conflict Detection**: Instant scheduling conflict prevention with real-time resolution suggestions
- **Weather-Aware Scheduling**: Real-time weather monitoring with automatic alerts and rescheduling recommendations
- **Smart Resource Allocation**: Live resource distribution with instant availability updates and skill-based matching
- **Cross-Platform Synchronization**: Seamless real-time updates across web, mobile, desktop, and external calendars

### 📈 Comprehensive Live Project Management Integration
- **Dynamic Task Synchronization**: Instant calendar updates based on real-time task progress and completion
- **Live Progress Tracking**: Interactive timeline visualization with real-time project progress and milestone updates
- **Advanced Team Coordination**: Live team scheduling with instant availability updates and collaborative editing
- **Professional Client Interface**: Real-time client portal with live progress updates and instant approval workflows

### 🌟 Next-Generation Real-Time Features
- **Live Collaborative Editing**: Multiple users can edit events simultaneously with real-time conflict resolution
- **Instant Push Notifications**: Real-time alerts for schedule changes, conflicts, and important updates
- **Live Analytics Dashboard**: Real-time metrics and insights with instant data visualization updates
- **Smart Notification System**: Context-aware real-time notifications with intelligent filtering and prioritization

### ⚡ Real-Time Technology Stack
- **SignalR WebSocket Hub**: Low-latency bidirectional communication for instant updates
- **Automatic Subscription Management**: Smart group-based subscriptions based on user permissions and project access
- **Offline-First Architecture**: Seamless synchronization when reconnecting with missed update recovery
- **Multi-Device Synchronization**: Instant updates across all user devices and platforms

### 🔄 Live Data Flow Architecture
1. **User Action Trigger**: Any calendar modification by any user
2. **Instant Validation**: Real-time conflict checking and permission validation
3. **Database Update**: Atomic transaction with versioning and audit logging
4. **Live Broadcast**: Immediate WebSocket notification to all affected users
5. **UI Synchronization**: Instant calendar refresh and conflict resolution across all connected clients
6. **Push Notifications**: Real-time mobile and desktop notifications for critical updates

The Calendar Events system transforms traditional scheduling into an **intelligent, real-time collaborative platform** that ensures optimal resource utilization, exceptional client satisfaction, and successful project delivery through **instant data synchronization** and **live collaborative features**.

## ❌ Calendar Event Error Codes & Troubleshooting

### 🔴 Core Operations & Authentication
| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| **CAL001** | 404 | Event not found | Verify event ID exists and user has access permissions |
| **CAL002** | 403 | Insufficient permissions | Check user role and event access rights; contact administrator |
| **CAL003** | 400 | Invalid event data | Validate request body for required fields and proper formatting |
| **CAL004** | 409 | Scheduling conflict detected | Resolve conflicts manually or use conflict resolution API |
| **CAL005** | 400 | Invalid date/time format | Use ISO 8601 format: `YYYY-MM-DDTHH:MM:SSZ` |

### 🟡 Business Logic & Validation Errors
| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| **CAL006** | 400 | Event scheduled in the past | Cannot create events for past dates; use current or future dates |
| **CAL007** | 400 | End time before start time | Verify start and end times are in logical order |
| **CAL008** | 404 | Attendee not found | Verify attendee user IDs and email addresses are valid |
| **CAL009** | 400 | Location not accessible | Verify location exists and GPS coordinates are valid |
| **CAL010** | 400 | Invalid recurrence pattern | Check recurrence settings, frequency, and end date |
| **CAL011** | 400 | Duration exceeds maximum limit | Event duration cannot exceed 24 hours for single events |
| **CAL012** | 400 | Too many attendees | Maximum 50 attendees per event; create multiple events if needed |

### 🟠 Resource & Integration Errors
| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| **CAL013** | 404 | Project not found | Verify project ID exists and user has access |
| **CAL014** | 404 | Task not found | Verify task ID exists and is associated with the project |
| **CAL015** | 409 | Resource unavailable | Check resource availability and schedule conflicts |
| **CAL016** | 503 | Calendar sync failed | Check external calendar integration settings and retry |
| **CAL017** | 503 | Notification delivery failed | Verify attendee email addresses and notification settings |
| **CAL018** | 400 | Weather service unavailable | Weather data temporarily unavailable; manual weather check required |
| **CAL019** | 409 | Critical path conflict | Event affects critical project path; manager approval required |
| **CAL020** | 400 | Resource booking limit exceeded | Maximum resource bookings per day exceeded |

### 🔵 System & Performance Errors
| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| **CAL021** | 429 | Rate limit exceeded | Too many requests; wait before retrying (max 100 requests/minute) |
| **CAL022** | 413 | Attachment too large | File size exceeds 10MB limit; compress or split files |
| **CAL023** | 500 | Database connection failed | Temporary database issue; retry in a few minutes |
| **CAL024** | 503 | External service timeout | Third-party service timeout; retry operation |
| **CAL025** | 507 | Storage quota exceeded | Calendar storage limit reached; archive old events |

### 🆘 Emergency & Critical Errors
| Error Code | HTTP Status | Description | Resolution |
|------------|-------------|-------------|------------|
| **CAL999** | 500 | Critical system failure | Contact system administrator immediately |
| **CAL998** | 503 | Maintenance mode active | System under maintenance; check status page for updates |
| **CAL997** | 401 | Authentication service down | Login service temporarily unavailable |

### 🔧 Troubleshooting Guidelines

#### Common Issues & Solutions

**Authentication Problems:**
1. Verify JWT token is valid and not expired
2. Check user permissions for the specific calendar/project
3. Ensure proper role-based access (Admin/Manager/User)

**Date/Time Issues:**
1. Always use UTC timezone in ISO 8601 format
2. Verify start time is before end time
3. Check for daylight saving time conflicts
4. Ensure dates are not in the past (unless historical events)

**Conflict Resolution:**
1. Use conflict detection API to identify overlaps
2. Check resource availability before scheduling
3. Consider alternative time slots or resources
4. Use automatic conflict resolution suggestions

**Performance Optimization:**
1. Use pagination for large date ranges
2. Limit attendee lists to essential participants
3. Cache frequently accessed calendar data
4. Use appropriate query filters to reduce response size

**Integration Issues:**
1. Verify external calendar authentication
2. Check internet connectivity for sync operations
3. Validate webhook URLs for real-time updates
4. Test API endpoints in isolation before integration

#### Support Resources
- **Documentation**: `/docs/api/calendar`
- **Status Page**: `status.solarprojects.com`
- **Support Portal**: `support@solarprojects.com`
- **Emergency Contact**: `+1-800-SOLAR-HELP`

---
*Last Updated: July 5, 2025 | API Version: 1.0 | Documentation Version: 2.1*
