# 📅 Calendar Events & Scheduling

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (view + create own events), All authenticated users (view)

The Calendar Events API provides comprehensive scheduling and event management capabilities for solar installation projects. Events can be linked to projects, tasks, milestones, and user schedules, providing a unified view of all project-related activities and deadlines.

## 🏗️ Project-Calendar Event Relationship

- **Project** can have multiple **Calendar Events** for milestones, deadlines, and project activities
- **Calendar Events** can be linked to **Tasks**, **Master Plans**, and **Project Milestones**
- Events inherit project context (location, team members, client information) from their parent Project
- Calendar integration supports both project-specific and personal scheduling
- All scheduling and deadline tracking is coordinated through the calendar system

## ⚡ Calendar Event Capabilities

### Admin & Manager
- ✅ Create events for any project and team member
- ✅ Schedule project milestones and deadlines
- ✅ Manage team schedules and resource allocation
- ✅ Create recurring events and event series
- ✅ Send meeting invitations and notifications
- ✅ Access comprehensive calendar analytics
- ✅ Configure calendar settings and permissions
- ✅ Delete and modify any events

### Project Managers
- ✅ Create and manage events for assigned projects
- ✅ Schedule project-specific activities and deadlines
- ✅ Coordinate team schedules within project scope
- ✅ Send team notifications and reminders
- ✅ Generate project timeline reports

### Users (Technicians & Field Staff)
- ✅ View project schedules and assigned events
- ✅ Create personal work events and reminders
- ✅ Receive event notifications and reminders
- ✅ Update attendance and event status
- ✅ Request schedule changes and time off
- ❌ Cannot modify project milestones or deadlines
- ❌ Cannot access other users' personal events

### Viewers
- ✅ View project events and milestones
- ✅ See public project schedules
- ❌ Cannot create or modify events

## 🎯 Calendar Event Features

### 📋 Comprehensive Event Management
- **Event Types**: Meetings, project milestones, deadlines, inspections, client visits, maintenance
- **Recurring Events**: Support for daily, weekly, monthly, and custom recurrence patterns
- **Event Categories**: Color-coded categorization with project and priority-based organization
- **Location Management**: GPS coordinates, site addresses, and location-based notifications
- **Attendee Management**: Invitation system with RSVP tracking and availability checking

### 🔄 Advanced Scheduling Features
- **Conflict Detection**: Automatic detection of scheduling conflicts and double-bookings
- **Resource Scheduling**: Integration with equipment and personnel availability
- **Time Zone Support**: Multi-timezone scheduling for distributed teams
- **Calendar Sync**: Integration with external calendar systems (Google, Outlook, iCal)
- **Mobile Optimization**: Full mobile calendar interface with offline access

### 📊 Analytics & Reporting
- **Schedule Analytics**: Team utilization, project timeline adherence, milestone tracking
- **Performance Metrics**: Meeting efficiency, deadline adherence, schedule optimization
- **Resource Utilization**: Equipment and personnel scheduling efficiency
- **Project Timeline Analysis**: Critical path analysis and schedule risk assessment

## 📋 Get All Calendar Events

**GET** `/api/v1/calendar/events`

Retrieve calendar events with comprehensive filtering and analytics options.

**Query Parameters**:
- `projectId` (Guid): Filter by specific project
- `userId` (Guid): Filter by event creator or attendee
- `eventType` (string): Filter by event type (Meeting, ProjectMilestone, Deadline, Inspection, ClientVisit)
- `startDate` (DateTime): Filter events from date (ISO format)
- `endDate` (DateTime): Filter events to date (ISO format)
- `category` (string): Filter by event category
- `status` (string): Filter by event status (Scheduled, InProgress, Completed, Cancelled)
- `priority` (string): Filter by priority (Low, Medium, High, Critical)
- `location` (string): Filter by location or site
- `hasConflicts` (bool): Filter events with scheduling conflicts
- `isRecurring` (bool): Filter recurring events
- `attendeeId` (Guid): Filter by specific attendee
- `includePrivate` (bool): Include private events (Admin/Manager only)
- `sortBy` (string): Sort field (startDateTime, priority, eventType, createdAt)
- `sortDirection` (string): Sort direction (asc, desc)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar events retrieved successfully",
  "data": {
    "events": [
      {
        "eventId": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Solar Panel Installation - Phase 2",
        "description": "Installation of 24 solar panels on east-facing roof section",
        "startDateTime": "2025-07-15T08:00:00Z",
        "endDateTime": "2025-07-15T16:00:00Z",
        "eventType": "ProjectMilestone",
        "status": "Scheduled",
        "priority": "High",
        "location": "123 Solar Street, Austin, TX",
        "gpsCoordinates": {
          "latitude": 30.2672,
          "longitude": -97.7431
        },
        "isAllDay": false,
        "isRecurring": false,
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "taskId": "task-123-id",
        "taskName": "Phase 2 Installation",
        "createdByUserId": "789e0123-e89b-12d3-a456-426614174002",
        "createdByName": "Sarah Manager",
        "assignedToUserId": "user1-id",
        "assignedToName": "John Technician",
        "attendees": [
          {
            "userId": "user1-id",
            "name": "John Technician",
            "email": "john@company.com",
            "rsvpStatus": "Accepted",
            "isRequired": true
          },
          {
            "userId": "user2-id",
            "name": "Mike Electrician",
            "email": "mike@company.com",
            "rsvpStatus": "Pending",
            "isRequired": true
          }
        ],
        "reminderMinutes": 60,
        "color": "#3788d8",
        "isPrivate": false,
        "hasConflicts": false,
        "conflictReason": null,
        "notes": "Weather backup date: July 16th",
        "attachments": [
          {
            "id": "att1-id",
            "fileName": "installation_plan.pdf",
            "fileType": "application/pdf",
            "uploadedAt": "2025-07-10T14:30:00Z"
          }
        ],
        "createdAt": "2025-07-10T14:30:00Z",
        "updatedAt": "2025-07-12T09:15:00Z"
      }
    ],
    "pagination": {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 3,
      "hasPreviousPage": false,
      "hasNextPage": true
    },
    "summary": {
      "totalEvents": 45,
      "upcomingEvents": 12,
      "overdue": 2,
      "inProgress": 3,
      "completed": 28,
      "conflicts": 1,
      "highPriority": 8
    }
  },
  "errors": []
}
```

## 🔍 Get Calendar Event by ID

**GET** `/api/v1/calendar/events/{id}`

Retrieve detailed information about a specific calendar event.

**Path Parameters**:
- `id` (Guid): Calendar event ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Calendar event retrieved successfully",
  "data": {
    "eventId": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Solar Panel Installation - Phase 2",
    "description": "Installation of 24 solar panels on east-facing roof section with quality inspections",
    "startDateTime": "2025-07-15T08:00:00Z",
    "endDateTime": "2025-07-15T16:00:00Z",
    "eventType": "ProjectMilestone",
    "status": "Scheduled",
    "priority": "High",
    "location": "123 Solar Street, Austin, TX",
    "gpsCoordinates": {
      "latitude": 30.2672,
      "longitude": -97.7431
    },
    "isAllDay": false,
    "isRecurring": false,
    "recurrencePattern": null,
    "recurrenceEndDate": null,
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "taskId": "task-123-id",
    "taskName": "Phase 2 Installation",
    "milestoneId": "milestone-456-id",
    "milestoneName": "East Section Complete",
    "createdByUserId": "789e0123-e89b-12d3-a456-426614174002",
    "createdByName": "Sarah Manager",
    "assignedToUserId": "user1-id",
    "assignedToName": "John Technician",
    "attendees": [
      {
        "userId": "user1-id",
        "name": "John Technician",
        "email": "john@company.com",
        "role": "Lead Installer",
        "rsvpStatus": "Accepted",
        "isRequired": true,
        "respondedAt": "2025-07-11T10:30:00Z"
      }
    ],
    "reminderMinutes": 60,
    "color": "#3788d8",
    "isPrivate": false,
    "hasConflicts": false,
    "conflictDetails": [],
    "notes": "Weather backup date: July 16th. Ensure all safety equipment is available.",
    "meetingUrl": null,
    "requirements": [
      "Safety equipment mandatory",
      "Installation materials pre-positioned",
      "Quality inspector on-site at 2 PM"
    ],
    "weatherForecast": {
      "condition": "Sunny",
      "temperature": 85,
      "windSpeed": 8,
      "precipitation": 0
    },
    "attachments": [
      {
        "id": "att1-id",
        "fileName": "installation_plan.pdf",
        "fileType": "application/pdf",
        "fileSize": 2048576,
        "uploadedAt": "2025-07-10T14:30:00Z",
        "uploadedBy": "Sarah Manager"
      }
    ],
    "relatedEvents": [
      {
        "eventId": "related-event-id",
        "title": "Material Delivery",
        "startDateTime": "2025-07-14T14:00:00Z",
        "relationship": "Prerequisite"
      }
    ],
    "createdAt": "2025-07-10T14:30:00Z",
    "updatedAt": "2025-07-12T09:15:00Z"
  },
  "errors": []
}
```

## 📝 Create Calendar Event

**POST** `/api/v1/calendar/events`

Create a new calendar event with comprehensive scheduling options.

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
  "errors": []
}
```

## 🔄 Update Calendar Event

**PUT** `/api/v1/calendar/events/{id}`

**🔒 Requires**: Admin, Manager, or event creator

Update an existing calendar event with validation and conflict checking.

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
    "notificationsSent": 2
  },
  "errors": []
}
```

## 🗑️ Delete Calendar Event

**DELETE** `/api/v1/calendar/events/{id}`

**🔒 Required Roles**: Admin, Manager, or event creator

Delete a calendar event with optional cancellation notifications.

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
  "errors": []
}
```

## 📅 Calendar Views & Analytics

### Get Calendar View

**GET** `/api/v1/calendar/view`

Get calendar events in various view formats (day, week, month, year).

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

Update RSVP status for a calendar event.

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
  "errors": []
}
```

### Mark Attendance

**POST** `/api/v1/calendar/events/{id}/attendance`

**🔒 Requires**: Event attendee or higher authority

Mark attendance for a calendar event.

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

## 🎯 Summary: Calendar as Project Coordination Hub

The Calendar Events system serves as the **central coordination and scheduling hub** for solar installation projects, providing:

### 📊 Unified Project Scheduling
- **Master Timeline**: Single source of truth for all project dates, milestones, and deadlines
- **Resource Coordination**: Personnel and equipment scheduling with conflict detection
- **Client Coordination**: Professional scheduling interface for client meetings and inspections
- **Milestone Tracking**: Integration with master plans for automatic milestone scheduling

### 🔄 Advanced Scheduling Intelligence
- **Conflict Detection**: Automatic identification and resolution of scheduling conflicts
- **Smart Scheduling**: AI-powered suggestions for optimal scheduling based on availability and preferences
- **Weather Integration**: Weather-aware scheduling with automatic notifications and rescheduling suggestions
- **Mobile Synchronization**: Real-time calendar sync across all devices and platforms

### 📈 Project Management Integration
- **Task Synchronization**: Automatic calendar updates based on task progress and completion
- **Progress Tracking**: Visual timeline representation of project progress and upcoming activities
- **Team Coordination**: Centralized team scheduling with role-based access and notifications
- **Client Communication**: Professional client-facing calendar interface with approval workflows

The Calendar Events system transforms basic scheduling into a comprehensive project coordination platform that ensures optimal resource utilization, client satisfaction, and successful project delivery.

## ❌ Calendar Event Error Codes

### Core Operations
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **CAL001** | Event not found | Verify event ID exists and user has access |
| **CAL002** | Insufficient permissions | Check user role and event access rights |
| **CAL003** | Invalid event data | Check request body for required fields |
| **CAL004** | Scheduling conflict detected | Resolve conflicts or override with proper authorization |
| **CAL005** | Invalid date/time format | Use ISO 8601 format (YYYY-MM-DDTHH:MM:SSZ) |

### Business Logic & Validation
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **CAL006** | Event in the past | Cannot create events for past dates |
| **CAL007** | End time before start time | Verify start and end times are logical |
| **CAL008** | Attendee not found | Verify attendee user IDs and email addresses |
| **CAL009** | Location not accessible | Verify location exists and is accessible |
| **CAL010** | Recurrence pattern invalid | Check recurrence settings and end date |

### Resource & Project Integration
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **CAL011** | Project not found | Verify project ID and user access |
| **CAL012** | Task not found | Verify task ID and project assignment |
| **CAL013** | Resource unavailable | Check resource availability and schedule conflicts |
| **CAL014** | Calendar sync failed | Check external calendar integration settings |
| **CAL015** | Notification delivery failed | Verify attendee email addresses and notification settings |

---
*Last Updated: July 4, 2025*
