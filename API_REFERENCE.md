# Solar Projects REST API Reference

## Overview

This is a comprehensive API reference for the Solar Projects Management REST API built with .NET 9.0. The API provides endpoints for managing solar installation projects, tasks, users, daily reports, and work requests with advanced features including caching, rate limiting, and HATEOAS support.

**Base URL**: `http://localhost:5002` (Local) | `https://solar-projects-api-dev.azurewebsites.net` (Production)  
**Version**: 1.0  
**Authentication**: JWT Bearer Token (except public endpoints)

### Key Features

🔐 **Secure Authentication**: JWT-based authentication with role-based access control  
📊 **Daily Report Management**: Complete workflow for field reporting with approval process  
🔧 **Work Request Tracking**: Change orders and additional work management  
📈 **Advanced Querying**: Complex filtering, sorting, and pagination on all collections  
🚀 **Performance Optimized**: Built-in caching and rate limiting  
🔗 **HATEOAS Support**: Hypermedia links for enhanced API discoverability  
📱 **Mobile Ready**: Image upload with GPS coordinates and device metadata  
🏥 **Health Monitoring**: Comprehensive health checks and system metrics  

### Quick Start

1. **Health Check**: `GET /health` - Verify API is running
2. **Authentication**: `POST /api/v1/auth/login` - Get access token
3. **Daily Reports**: `GET /api/v1/daily-reports` - Access core functionality
4. **Interactive Docs**: Visit `http://localhost:5002` for Swagger UI

---

## 📋 Table of Contents

- [Authentication](#authentication)
- [Health Monitoring](#health-monitoring)
- [Calendar Management](#calendar-management)
- [Daily Reports Management](#daily-reports-management)
- [Work Request Management](#work-request-management)
- [Rate Limiting](#rate-limiting)
- [Caching and Performance](#caching-and-performance)
- [HATEOAS Implementation](#hateoas-implementation)
- [Advanced Querying](#advanced-querying)
- [Todo Management (Legacy)](#todo-management-legacy)
- [Debug Information](#debug-information)
- [User Management](#user-management)
- [Project Management](#project-management)
- [Task Management](#task-management)
- [Image Management](#image-management)
- [Error Responses](#error-responses)
- [Testing Examples](#testing-examples)

---

## 🔐 Authentication

### Login
**POST** `/api/v1/auth/login`

**Request Body**:
```json
{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "john.doe",
      "email": "john.doe@example.com",
      "fullName": "John Doe",
      "roleName": "Technician",
      "isActive": true
    }
  },
  "errors": []
}
```

**Response (401 Unauthorized)**:
```json
{
  "success": false,
  "message": "Invalid username or password",
  "data": null,
  "errors": ["Authentication failed"]
}
```

### Register
**POST** `/api/v1/auth/register`

**Request Body**:
```json
{
  "username": "jane.smith",
  "email": "jane.smith@example.com",
  "password": "SecurePassword123!",
  "fullName": "Jane Smith",
  "roleId": 2
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "User registered successfully",
  "data": {
    "userId": "456e7890-e89b-12d3-a456-426614174001",
    "username": "jane.smith",
    "email": "jane.smith@example.com",
    "fullName": "Jane Smith",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Refresh Token
**POST** `/api/v1/auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "refresh_token_here"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": "new_jwt_token_here",
  "errors": []
}
```

---

## ❤️ Health Monitoring

### Basic Health Check
**GET** `/health`

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-08T07:30:11.227702Z",
  "version": "1.0.0",
  "environment": "Development"
}
```

### Detailed Health Check
**GET** `/health/detailed`

**Response (200 OK)**:
```json
{
  "status": "Healthy",
  "timestamp": "2025-06-08T07:30:11.227702Z",
  "version": "1.0.0",
  "environment": "Development",
  "database": {
    "status": "Connected",
    "provider": "Npgsql.EntityFrameworkCore.PostgreSQL"
  },
  "memory": {
    "workingSet": 52428800,
    "gen0Collections": 12,
    "gen1Collections": 4,
    "gen2Collections": 1
  }
}
```

---

## 📅 Calendar Management

**🔒 Authentication Required**

The Calendar API provides comprehensive event planning and scheduling functionality for solar projects. It supports CRUD operations, advanced filtering, event associations with projects and tasks, conflict detection, and recurring events (future implementation). Each calendar event can be categorized by type, priority, and status with full audit trails.

### Event Types

| Type | Value | Description |
|------|-------|-------------|
| `Meeting` | 1 | Team meetings, client calls, standup meetings |
| `Deadline` | 2 | Project milestones, task due dates, deliverables |
| `Installation` | 3 | On-site installation work, system commissioning |
| `Maintenance` | 4 | Routine maintenance, inspections, repairs |
| `Training` | 5 | Team training sessions, certification courses |
| `Other` | 6 | General events not covered by other types |

### Event Status

| Status | Value | Description |
|--------|-------|-------------|
| `Scheduled` | 1 | Event is planned and confirmed |
| `InProgress` | 2 | Event is currently happening |
| `Completed` | 3 | Event has been finished |
| `Cancelled` | 4 | Event has been cancelled |
| `Postponed` | 5 | Event has been delayed to a future date |

### Event Priority

| Priority | Value | Description |
|----------|-------|-------------|
| `Low` | 1 | Optional or flexible events |
| `Medium` | 2 | Standard priority events |
| `High` | 3 | Important events requiring attention |
| `Critical` | 4 | Urgent events that cannot be missed |

### Get All Calendar Events
**GET** `/api/v1/calendar`

**Query Parameters**:
- `startDate` (DateTime): Filter events starting from this date
- `endDate` (DateTime): Filter events ending before this date
- `eventType` (EventType): Filter by event type
- `status` (EventStatus): Filter by event status
- `priority` (EventPriority): Filter by event priority
- `isAllDay` (bool): Filter all-day events
- `isRecurring` (bool): Filter recurring events
- `projectId` (Guid): Filter events for specific project
- `taskId` (Guid): Filter events for specific task
- `createdByUserId` (Guid): Filter events created by user
- `assignedToUserId` (Guid): Filter events assigned to user
- `pageNumber` (int): Page number for pagination (default: 1)
- `pageSize` (int): Number of items per page (default: 10, max: 100)

**Example Request**:
```
GET /api/v1/calendar?startDate=2025-06-01&endDate=2025-06-30&eventType=Meeting&pageSize=20
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "events": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "title": "Project Kickoff Meeting",
        "startDateTime": "2025-06-10T09:00:00Z",
        "endDateTime": "2025-06-10T10:30:00Z",
        "isAllDay": false,
        "eventType": 1,
        "eventTypeName": "Meeting",
        "status": 1,
        "statusName": "Scheduled",
        "priority": 3,
        "priorityName": "High",
        "location": "Conference Room A",
        "projectName": "Solar Installation Project Alpha",
        "taskName": null,
        "isRecurring": false
      }
    ],
    "totalCount": 25,
    "page": 1,
    "pageSize": 20,
    "totalPages": 2,
    "hasPreviousPage": false,
    "hasNextPage": true
  },
  "errors": [],
  "error": null
}
```

### Get Calendar Event by ID
**GET** `/api/v1/calendar/{eventId}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "title": "Project Kickoff Meeting",
    "description": "Initial planning meeting for Solar Installation Project Alpha",
    "startDateTime": "2025-06-10T09:00:00Z",
    "endDateTime": "2025-06-10T10:30:00Z",
    "isAllDay": false,
    "eventType": 1,
    "eventTypeName": "Meeting",
    "status": 1,
    "statusName": "Scheduled",
    "priority": 3,
    "priorityName": "High",
    "location": "Conference Room A",
    "projectId": "456e7890-e89b-12d3-a456-426614174000",
    "projectName": "Solar Installation Project Alpha",
    "taskId": null,
    "taskName": null,
    "createdByUserId": "789e0123-e89b-12d3-a456-426614174000",
    "createdByUserName": null,
    "assignedToUserId": "789e0123-e89b-12d3-a456-426614174000",
    "assignedToUserName": null,
    "isRecurring": false,
    "recurrencePattern": null,
    "recurrenceEndDate": null,
    "reminderMinutes": 15,
    "isPrivate": false,
    "meetingUrl": "https://teams.microsoft.com/l/meetup-join/...",
    "attendees": "john.doe@example.com, jane.smith@example.com",
    "notes": "Bring project specifications and timeline",
    "createdAt": "2025-06-08T14:30:00Z",
    "updatedAt": "2025-06-08T14:30:00Z"
  },
  "errors": [],
  "error": null
}
```

### Create Calendar Event
**POST** `/api/v1/calendar`

**Request Body**:
```json
{
  "title": "Installation Site Visit",
  "description": "On-site inspection and preparation for solar panel installation",
  "startDateTime": "2025-06-15T08:00:00Z",
  "endDateTime": "2025-06-15T12:00:00Z",
  "eventType": "Installation",
  "status": "Scheduled",
  "priority": "High",
  "location": "123 Solar Street, Sunnyville, CA",
  "isAllDay": false,
  "isRecurring": false,
  "notes": "Bring safety equipment and measurement tools",
  "reminderMinutes": 30,
  "projectId": "456e7890-e89b-12d3-a456-426614174000",
  "taskId": "789e0123-e89b-12d3-a456-426614174000",
  "assignedToUserId": "123e4567-e89b-12d3-a456-426614174000",
  "color": "#FF9800",
  "isPrivate": false,
  "attendees": "tech1@example.com, supervisor@example.com"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Operation completed successfully",
  "data": {
    "id": "456e7890-e89b-12d3-a456-426614174000",
    "title": "Installation Site Visit",
    "description": "On-site inspection and preparation for solar panel installation",
    "startDateTime": "2025-06-15T08:00:00Z",
    "endDateTime": "2025-06-15T12:00:00Z",
    "isAllDay": false,
    "eventType": 3,
    "eventTypeName": "Installation",
    "status": 1,
    "statusName": "Scheduled",
    "priority": 3,
    "priorityName": "High",
    "location": "123 Solar Street, Sunnyville, CA",
    "projectId": "456e7890-e89b-12d3-a456-426614174000",
    "projectName": null,
    "taskId": "789e0123-e89b-12d3-a456-426614174000",
    "taskName": null,
    "createdByUserId": "789e0123-e89b-12d3-a456-426614174000",
    "createdByUserName": null,
    "assignedToUserId": "123e4567-e89b-12d3-a456-426614174000",
    "assignedToUserName": null,
    "isRecurring": false,
    "recurrencePattern": null,
    "recurrenceEndDate": null,
    "reminderMinutes": 15,
    "isPrivate": false,
    "meetingUrl": null,
    "attendees": "tech1@example.com, supervisor@example.com",
    "notes": "Bring safety equipment and measurement tools",
    "createdAt": "2025-06-10T16:15:00Z",
    "updatedAt": "2025-06-10T16:15:00Z"
  },
  "errors": [],
  "error": null
}
```

### Update Calendar Event
**PUT** `/api/v1/calendar/{eventId}`

**Request Body**:
```json
{
  "title": "Installation Site Visit - Updated",
  "description": "Updated: On-site inspection and preparation for solar panel installation",
  "startDateTime": "2025-06-15T09:00:00Z",
  "endDateTime": "2025-06-15T13:00:00Z",
  "status": "InProgress",
  "priority": "Critical",
  "location": "123 Solar Street, Sunnyville, CA",
  "notes": "Updated: Bring safety equipment, measurement tools, and installation materials",
  "reminderMinutes": 15,
  "color": "#F44336"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Calendar event updated successfully",
  "data": {
    "eventId": "456e7890-e89b-12d3-a456-426614174000",
    "title": "Installation Site Visit - Updated",
    "description": "Updated: On-site inspection and preparation for solar panel installation",
    "startDateTime": "2025-06-15T09:00:00Z",
    "endDateTime": "2025-06-15T13:00:00Z",
    "eventType": "Installation",
    "status": "InProgress",
    "priority": "Critical",
    "location": "123 Solar Street, Sunnyville, CA",
    "isAllDay": false,
    "isRecurring": false,
    "projectId": "456e7890-e89b-12d3-a456-426614174000",
    "taskId": "789e0123-e89b-12d3-a456-426614174000",
    "createdByUserId": "789e0123-e89b-12d3-a456-426614174000",
    "assignedToUserId": "123e4567-e89b-12d3-a456-426614174000",
    "createdAt": "2025-06-10T16:15:00Z",
    "updatedAt": "2025-06-10T16:45:00Z",
    "color": "#F44336",
    "isPrivate": false,
    "attendees": "tech1@example.com, supervisor@example.com"
  },
  "errors": []
}
```

### Delete Calendar Event
**DELETE** `/api/v1/calendar/{eventId}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Calendar event deleted successfully",
  "data": null,
  "errors": []
}
```

### Get Events by Project
**GET** `/api/v1/calendar/project/{projectId}`

**Query Parameters**:
- `startDate` (DateTime): Filter events starting from this date
- `endDate` (DateTime): Filter events ending before this date
- `eventType` (EventType): Filter by event type
- `status` (EventStatus): Filter by event status

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Project calendar events retrieved successfully",
  "data": [
    {
      "eventId": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Project Kickoff Meeting",
      "startDateTime": "2025-06-10T09:00:00Z",
      "endDateTime": "2025-06-10T10:30:00Z",
      "eventType": "Meeting",
      "status": "Scheduled",
      "priority": "High"
    }
  ],
  "errors": []
}
```

### Get Events by Task
**GET** `/api/v1/calendar/task/{taskId}`

**Response**: Similar to project events endpoint

### Get Events by User
**GET** `/api/v1/calendar/user/{userId}`

**Response**: Similar to project events endpoint

### Get Upcoming Events
**GET** `/api/v1/calendar/upcoming`

**Query Parameters**:
- `days` (int): Number of days to look ahead (default: 7, max: 365)
- `userId` (Guid): Filter events for specific user

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Upcoming calendar events retrieved successfully",
  "data": [
    {
      "eventId": "123e4567-e89b-12d3-a456-426614174000",
      "title": "Project Kickoff Meeting",
      "startDateTime": "2025-06-10T09:00:00Z",
      "endDateTime": "2025-06-10T10:30:00Z",
      "eventType": "Meeting",
      "status": "Scheduled",
      "priority": "High",
      "location": "Conference Room A"
    }
  ],
  "errors": []
}
```

### Check Event Conflicts
**POST** `/api/v1/calendar/conflicts`

**Request Body**:
```json
{
  "startDateTime": "2025-06-15T09:00:00Z",
  "endDateTime": "2025-06-15T11:00:00Z",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "excludeEventId": "456e7890-e89b-12d3-a456-426614174000"
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Conflict check completed",
  "data": {
    "hasConflicts": true,
    "conflictingEvents": [
      {
        "eventId": "789e0123-e89b-12d3-a456-426614174000",
        "title": "Team Standup",
        "startDateTime": "2025-06-15T09:30:00Z",
        "endDateTime": "2025-06-15T10:00:00Z",
        "eventType": "Meeting",
        "status": "Scheduled"
      }
    ]
  },
  "errors": []
}
```

### Recurring Events (Future Implementation)

The following endpoints are placeholders for future recurring event functionality:

#### Get Recurring Events
**GET** `/api/v1/calendar/recurring`

#### Create Recurring Event Series
**POST** `/api/v1/calendar/recurring`

#### Update Recurring Event Series
**PUT** `/api/v1/calendar/recurring/{seriesId}`

#### Delete Recurring Event Series
**DELETE** `/api/v1/calendar/recurring/{seriesId}`

---

## 📊 Daily Reports Management

**🔒 Authentication Required**

The Daily Reports API manages comprehensive field reports for solar installation projects. Reports follow a structured workflow: **Draft** → **Submitted** → **Approved/Rejected**. This system supports HATEOAS (Hypermedia as the Engine of Application State) for enhanced API navigation and includes advanced caching for optimal performance.

### Report Status Workflow

| Status | Description | Next Actions |
|--------|-------------|--------------|
| `Draft` | Report created but not yet submitted | Submit, Update, Delete |
| `Submitted` | Report submitted for review | Approve, Reject |
| `Approved` | Report approved by supervisor | View only |
| `Rejected` | Report rejected and returned to technician | Update, Resubmit |

### Get All Daily Reports
**GET** `/api/v1/daily-reports`

**Cache Duration**: 5 minutes

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `projectId` (GUID, optional): Filter by project ID
- `technicianId` (GUID, optional): Filter by technician ID
- `status` (string, optional): Filter by status (Draft, Submitted, Approved, Rejected)
- `dateFrom` (datetime, optional): Filter reports from date
- `dateTo` (datetime, optional): Filter reports to date
- `sortBy` (string, optional): Sort field (reportDate, createdAt, updatedAt)
- `sortOrder` (string, optional): Sort direction (asc, desc)

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Daily reports retrieved successfully",
  "data": {
    "items": [
      {
        "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "technicianId": "234f5678-e89b-12d3-a456-426614174001",
        "reportDate": "2025-06-08T00:00:00Z",
        "status": "Submitted",
        "workStartTime": "07:30:00",
        "workEndTime": "16:00:00",
        "weatherConditions": "Sunny, 75°F, Light breeze",
        "overallNotes": "Good progress on panel installation. No major issues encountered.",
        "safetyNotes": "All safety protocols followed. Hard hats and harnesses used.",
        "delaysOrIssues": "Minor delay due to electrical inspection.",
        "photosCount": 8,
        "createdAt": "2025-06-08T16:30:00Z",
        "updatedAt": "2025-06-08T16:30:00Z",
        "project": {
          "projectId": "550e8400-e29b-41d4-a716-446655440000",
          "projectName": "Downtown Solar Installation",
          "address": "123 Main St, City, State 12345"
        },
        "technician": {
          "userId": "234f5678-e89b-12d3-a456-426614174001",
          "fullName": "Mike Technician",
          "email": "mike.tech@example.com"
        },
        "_links": {
          "self": {
            "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc"
          },
          "submit": {
            "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/submit",
            "method": "POST"
          },
          "work-progress": {
            "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/work-progress"
          },
          "personnel-logs": {
            "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/personnel-logs"
          }
        }
      }
    ],
    "totalCount": 45,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 5,
    "hasNextPage": true,
    "hasPreviousPage": false
  },
  "errors": []
}
```

### Get Daily Report by ID
**GET** `/api/v1/daily-reports/{id}`

**Cache Duration**: 5 minutes

**Parameters**:
- `id` (path, GUID): Daily report ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Daily report retrieved successfully",
  "data": {
    "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "technicianId": "234f5678-e89b-12d3-a456-426614174001",
    "reportDate": "2025-06-08T00:00:00Z",
    "status": "Submitted",
    "workStartTime": "07:30:00",
    "workEndTime": "16:00:00",
    "weatherConditions": "Sunny, 75°F, Light breeze",
    "overallNotes": "Good progress on panel installation. No major issues encountered.",
    "safetyNotes": "All safety protocols followed. Hard hats and harnesses used.",
    "delaysOrIssues": "Minor delay due to electrical inspection.",
    "photosCount": 8,
    "createdAt": "2025-06-08T16:30:00Z",
    "updatedAt": "2025-06-08T16:30:00Z",
    "project": {
      "projectId": "550e8400-e29b-41d4-a716-446655440000",
      "projectName": "Downtown Solar Installation",
      "address": "123 Main St, City, State 12345",
      "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)"
    },
    "technician": {
      "userId": "234f5678-e89b-12d3-a456-426614174001",
      "username": "tech.mike",
      "fullName": "Mike Technician",
      "email": "mike.tech@example.com",
      "roleName": "Technician"
    },
    "_links": {
      "self": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc"
      },
      "submit": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/submit",
        "method": "POST"
      },
      "work-progress": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/work-progress"
      },
      "personnel-logs": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/personnel-logs"
      },
      "material-usage": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/material-usage"
      },
      "equipment-logs": {
        "href": "/api/v1/daily-reports/aa0a1234-b5c6-7d8e-9f10-123456789abc/equipment-logs"
      }
    }
  },
  "errors": []
}
```

### Create Daily Report
**POST** `/api/v1/daily-reports`

**Required Role**: Technician, ProjectManager, Administrator

**Request Body**:
```json
{
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "reportDate": "2025-06-09T00:00:00Z",
  "workStartTime": "08:00:00",
  "workEndTime": "17:00:00",
  "weatherConditions": "Cloudy, 72°F, No wind",
  "overallNotes": "Continued panel installation on building section B.",
  "safetyNotes": "All team members wore required PPE. Safety meeting conducted at start.",
  "delaysOrIssues": "No significant issues today.",
  "photosCount": 12
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Daily report created successfully",
  "data": {
    "reportId": "bb1b5678-c9d0-1e2f-3a4b-567890123def",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "technicianId": "234f5678-e89b-12d3-a456-426614174001",
    "reportDate": "2025-06-09T00:00:00Z",
    "status": "Draft",
    "workStartTime": "08:00:00",
    "workEndTime": "17:00:00",
    "weatherConditions": "Cloudy, 72°F, No wind",
    "overallNotes": "Continued panel installation on building section B.",
    "safetyNotes": "All team members wore required PPE. Safety meeting conducted at start.",
    "delaysOrIssues": "No significant issues today.",
    "photosCount": 12,
    "createdAt": "2025-06-09T17:30:00Z",
    "updatedAt": "2025-06-09T17:30:00Z",
    "_links": {
      "self": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def"
      },
      "update": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def",
        "method": "PUT"
      },
      "submit": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def/submit",
        "method": "POST"
      }
    }
  },
  "errors": []
}
```

### Update Daily Report
**PUT** `/api/v1/daily-reports/{id}`

**Required Role**: Report creator, ProjectManager, Administrator  
**Note**: Only reports in "Draft" or "Rejected" status can be updated

**Request Body**:
```json
{
  "reportDate": "2025-06-09T00:00:00Z",
  "workStartTime": "08:00:00",
  "workEndTime": "16:30:00",
  "weatherConditions": "Cloudy, 72°F, Light rain in afternoon",
  "overallNotes": "Continued panel installation. Stopped work early due to rain.",
  "safetyNotes": "All team members wore required PPE. Extra caution due to wet conditions.",
  "delaysOrIssues": "2-hour delay due to rain in the afternoon.",
  "photosCount": 8
}
```

**Response (200 OK)**: Same structure as Create Daily Report

### Submit Daily Report
**POST** `/api/v1/daily-reports/{id}/submit`

**Required Role**: Report creator, ProjectManager, Administrator  
**Note**: Only reports in "Draft" status can be submitted

**Parameters**:
- `id` (path, GUID): Daily report ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Daily report submitted successfully",
  "data": {
    "reportId": "bb1b5678-c9d0-1e2f-3a4b-567890123def",
    "status": "Submitted",
    "submittedAt": "2025-06-09T18:00:00Z",
    "_links": {
      "self": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def"
      }
    }
  },
  "errors": []
}
```

### Approve Daily Report
**POST** `/api/v1/daily-reports/{id}/approve`

**Required Role**: ProjectManager, Administrator  
**Note**: Only reports in "Submitted" status can be approved

**Request Body** (optional):
```json
{
  "approverComments": "Report looks good. All required information provided."
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Daily report approved successfully",
  "data": {
    "reportId": "bb1b5678-c9d0-1e2f-3a4b-567890123def",
    "status": "Approved",
    "approvedAt": "2025-06-10T09:00:00Z",
    "approverComments": "Report looks good. All required information provided."
  },
  "errors": []
}
```

### Reject Daily Report
**POST** `/api/v1/daily-reports/{id}/reject`

**Required Role**: ProjectManager, Administrator  
**Note**: Only reports in "Submitted" status can be rejected

**Request Body**:
```json
{
  "rejectionReason": "Missing safety documentation and incomplete work progress details."
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Daily report rejected successfully",
  "data": {
    "reportId": "bb1b5678-c9d0-1e2f-3a4b-567890123def",
    "status": "Rejected",
    "rejectedAt": "2025-06-10T14:00:00Z",
    "rejectionReason": "Missing safety documentation and incomplete work progress details.",
    "_links": {
      "self": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def"
      },
      "update": {
        "href": "/api/v1/daily-reports/bb1b5678-c9d0-1e2f-3a4b-567890123def",
        "method": "PUT"
      }
    }
  },
  "errors": []
}
```

### Delete Daily Report
**DELETE** `/api/v1/daily-reports/{id}`

**Required Role**: Report creator, ProjectManager, Administrator  
**Note**: Only reports in "Draft" status can be deleted

**Response (204 No Content)**

### Work Progress Items

#### Get Work Progress Items
**GET** `/api/v1/daily-reports/{reportId}/work-progress`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Work progress items retrieved successfully",
  "data": [
    {
      "workProgressId": "cc2c6789-d0e1-2f3a-4b5c-678901234efg",
      "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
      "taskDescription": "Install solar panels on roof section A",
      "hoursWorked": 6.5,
      "percentageComplete": 75,
      "notes": "Completed 12 of 16 panels. Good progress despite morning fog.",
      "createdAt": "2025-06-08T16:30:00Z"
    }
  ],
  "errors": []
}
```

#### Add Work Progress Item
**POST** `/api/v1/daily-reports/{reportId}/work-progress`

**Request Body**:
```json
{
  "taskDescription": "Install electrical conduits",
  "hoursWorked": 4.0,
  "percentageComplete": 100,
  "notes": "All conduits installed and secured according to specifications."
}
```

**Response (201 Created)**: Same structure as Get Work Progress Items

#### Update Work Progress Item
**PUT** `/api/v1/daily-reports/{reportId}/work-progress/{workProgressId}`

**Request Body**:
```json
{
  "taskDescription": "Install electrical conduits",
  "hoursWorked": 4.5,
  "percentageComplete": 100,
  "notes": "All conduits installed and secured. Added extra support brackets."
}
```

#### Delete Work Progress Item
**DELETE** `/api/v1/daily-reports/{reportId}/work-progress/{workProgressId}`

**Response (204 No Content)**

### Personnel Logs

#### Get Personnel Logs
**GET** `/api/v1/daily-reports/{reportId}/personnel-logs`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Personnel logs retrieved successfully",
  "data": [
    {
      "personnelLogId": "dd3d7890-e1f2-3a4b-5c6d-789012345fgh",
      "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
      "personnelName": "Mike Johnson",
      "role": "Lead Technician",
      "hoursWorked": 8.0,
      "overtimeHours": 0.0,
      "notes": "Supervised panel installation and quality checks.",
      "createdAt": "2025-06-08T16:30:00Z"
    }
  ],
  "errors": []
}
```

#### Add Personnel Log
**POST** `/api/v1/daily-reports/{reportId}/personnel-logs`

**Request Body**:
```json
{
  "personnelName": "Sarah Williams",
  "role": "Technician",
  "hoursWorked": 7.5,
  "overtimeHours": 0.0,
  "notes": "Worked on electrical connections and system testing."
}
```

### Material Usage

#### Get Material Usage
**GET** `/api/v1/daily-reports/{reportId}/material-usage`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Material usage retrieved successfully",
  "data": [
    {
      "materialUsageId": "ee4e8901-f2g3-4b5c-6d7e-890123456ghi",
      "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
      "materialName": "Solar Panels (320W)",
      "quantityUsed": 12,
      "unit": "pieces",
      "notes": "High-efficiency monocrystalline panels installed on roof section A.",
      "createdAt": "2025-06-08T16:30:00Z"
    }
  ],
  "errors": []
}
```

#### Add Material Usage
**POST** `/api/v1/daily-reports/{reportId}/material-usage`

**Request Body**:
```json
{
  "materialName": "Electrical Conduit (1 inch)",
  "quantityUsed": 50,
  "unit": "feet",
  "notes": "Used for main electrical run from panels to inverter."
}
```

### Equipment Logs

#### Get Equipment Logs
**GET** `/api/v1/daily-reports/{reportId}/equipment-logs`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Equipment logs retrieved successfully",
  "data": [
    {
      "equipmentLogId": "ff5f9012-g3h4-5c6d-7e8f-901234567hij",
      "reportId": "aa0a1234-b5c6-7d8e-9f10-123456789abc",
      "equipmentName": "Scissor Lift",
      "usageHours": 6.0,
      "condition": "Good",
      "notes": "Used for panel installation. No issues encountered.",
      "createdAt": "2025-06-08T16:30:00Z"
    }
  ],
  "errors": []
}
```

#### Add Equipment Log
**POST** `/api/v1/daily-reports/{reportId}/equipment-logs`

**Request Body**:
```json
{
  "equipmentName": "Power Drill",
  "usageHours": 4.0,
  "condition": "Good",
  "notes": "Used for mounting bracket installation."
}
```

---

## 🔧 Work Request Management

**🔒 Authentication Required**

Work requests are used to track additional work, change orders, and special requirements for solar installation projects.

### Get All Work Requests
**GET** `/api/v1/work-requests`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `projectId` (GUID, optional): Filter by project ID
- `requestType` (string, optional): Filter by request type
- `status` (string, optional): Filter by status
- `priority` (string, optional): Filter by priority (Low, Medium, High, Critical)

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Work requests retrieved successfully",
  "data": {
    "items": [
      {
        "workRequestId": "gg6g0123-h4i5-6d7e-8f9g-012345678ijk",
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "requestType": "Change Order",
        "title": "Additional Panel Installation",
        "description": "Client requested additional 8 panels on the south-facing roof section.",
        "status": "Pending",
        "priority": "Medium",
        "estimatedCost": 3200.00,
        "estimatedHours": 16.0,
        "requestedBy": {
          "userId": "123e4567-e89b-12d3-a456-426614174000",
          "fullName": "Jane Project Manager",
          "email": "jane.pm@example.com"
        },
        "createdAt": "2025-06-07T10:00:00Z",
        "updatedAt": "2025-06-07T10:00:00Z",
        "project": {
          "projectId": "550e8400-e29b-41d4-a716-446655440000",
          "projectName": "Downtown Solar Installation",
          "address": "123 Main St, City, State 12345"
        }
      }
    ],
    "totalCount": 15,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2
  },
  "errors": []
}
```

### Get Work Request by ID
**GET** `/api/v1/work-requests/{id}`

**Parameters**:
- `id` (path, GUID): Work request ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Work request retrieved successfully",
  "data": {
    "workRequestId": "gg6g0123-h4i5-6d7e-8f9g-012345678ijk",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "requestType": "Change Order",
    "title": "Additional Panel Installation",
    "description": "Client requested additional 8 panels on the south-facing roof section.",
    "status": "Pending",
    "priority": "Medium",
    "estimatedCost": 3200.00,
    "estimatedHours": 16.0,
    "requestedBy": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "pm.jane",
      "fullName": "Jane Project Manager",
      "email": "jane.pm@example.com",
      "roleName": "ProjectManager"
    },
    "createdAt": "2025-06-07T10:00:00Z",
    "updatedAt": "2025-06-07T10:00:00Z",
    "project": {
      "projectId": "550e8400-e29b-41d4-a716-446655440000",
      "projectName": "Downtown Solar Installation",
      "address": "123 Main St, City, State 12345",
      "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)"
    }
  },
  "errors": []
}
```

### Create Work Request
**POST** `/api/v1/work-requests`

**Required Role**: ProjectManager, Administrator

**Request Body**:
```json
{
  "projectId": "550e8400-e29b-41d4-a716-446655440000",
  "requestType": "Change Order",
  "title": "Electrical Panel Upgrade",
  "description": "Upgrade main electrical panel to handle increased solar capacity.",
  "priority": "High",
  "estimatedCost": 1500.00,
  "estimatedHours": 8.0
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Work request created successfully",
  "data": {
    "workRequestId": "hh7h1234-i5j6-7e8f-9g0h-123456789jkl",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "requestType": "Change Order",
    "title": "Electrical Panel Upgrade",
    "description": "Upgrade main electrical panel to handle increased solar capacity.",
    "status": "Pending",
    "priority": "High",
    "estimatedCost": 1500.00,
    "estimatedHours": 8.0,
    "requestedBy": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "fullName": "Jane Project Manager",
      "email": "jane.pm@example.com"
    },
    "createdAt": "2025-06-09T14:00:00Z",
    "updatedAt": "2025-06-09T14:00:00Z"
  },
  "errors": []
}
```

### Update Work Request
**PUT** `/api/v1/work-requests/{id}`

**Required Role**: RequestCreator, ProjectManager, Administrator

**Request Body**:
```json
{
  "title": "Electrical Panel Upgrade - Updated",
  "description": "Upgrade main electrical panel to 200A service to handle increased solar capacity.",
  "priority": "High",
  "status": "Approved",
  "estimatedCost": 1800.00,
  "estimatedHours": 10.0
}
```

**Response (200 OK)**: Same structure as Create Work Request

### Delete Work Request
**DELETE** `/api/v1/work-requests/{id}`

**Required Role**: RequestCreator, ProjectManager, Administrator

**Response (204 No Content)**

---

## ✅ Todo Management (Legacy)

### Get All Todos
**GET** `/api/todo`

**Response (200 OK)**:
```json
[
  {
    "id": 1,
    "title": "Complete solar panel installation",
    "isCompleted": false,
    "dueDate": "2025-06-15T00:00:00Z"
  },
  {
    "id": 2,
    "title": "Submit project documentation",
    "isCompleted": true,
    "dueDate": "2025-06-10T00:00:00Z"
  }
]
```

### Get Todo by ID
**GET** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Response (200 OK)**:
```json
{
  "id": 1,
  "title": "Complete solar panel installation",
  "isCompleted": false,
  "dueDate": "2025-06-15T00:00:00Z"
}
```

**Response (404 Not Found)**:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "traceId": "00-4bfbd643cef305ad68bd0b16d14a6998-51d8e25c1be1cfd9-00"
}
```

### Create Todo
**POST** `/api/todo`

**Request Body**:
```json
{
  "title": "Test Todo Item",
  "description": "This is a test todo",
  "isCompleted": false,
  "dueDate": "2025-06-20T00:00:00Z"
}
```

**Response (201 Created)**:
```json
{
  "id": 3,
  "title": "Test Todo Item",
  "isCompleted": false,
  "dueDate": "2025-06-20T00:00:00Z"
}
```

### Update Todo
**PUT** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Request Body**:
```json
{
  "id": 1,
  "title": "Updated Todo Title",
  "description": "Updated description",
  "isCompleted": true,
  "dueDate": "2025-06-15T00:00:00Z"
}
```

**Response (204 No Content)**

### Delete Todo
**DELETE** `/api/todo/{id}`

**Parameters**:
- `id` (path, integer): Todo item ID

**Response (204 No Content)**

---

## 🔧 Debug Information

### Get Configuration
**GET** `/api/debug/config`

**Response (200 OK)**:
```json
{
  "environment": "Development",
  "connectionString": "Server=localhost;Database=SolarProjectsDb;...",
  "allConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SolarProjectsDb;..."
  }
}
```

---

## 👥 User Management

**🔒 Authentication Required**

### Get All Users
**GET** `/api/v1/users`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `role` (string, optional): Filter by role name

**Example Request**:
```
GET /api/v1/users?pageNumber=1&pageSize=20&role=Technician
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Users retrieved successfully",
  "data": {
    "items": [
      {
        "userId": "123e4567-e89b-12d3-a456-426614174000",
        "username": "john.doe",
        "email": "john.doe@example.com",
        "fullName": "John Doe",
        "roleName": "Technician",
        "isActive": true
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 20,
    "totalPages": 2
  },
  "errors": []
}
```

### Get User by ID
**GET** `/api/v1/users/{id}`

**Parameters**:
- `id` (path, GUID): User ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "User retrieved successfully",
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "username": "john.doe",
    "email": "john.doe@example.com",
    "fullName": "John Doe",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Create User
**POST** `/api/v1/users`  
**Required Role**: Administrator

**Request Body**:
```json
{
  "username": "new.user",
  "email": "new.user@example.com",
  "password": "SecurePassword123!",
  "fullName": "New User",
  "roleId": 2
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "User created successfully",
  "data": {
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "username": "new.user",
    "email": "new.user@example.com",
    "fullName": "New User",
    "roleName": "Technician",
    "isActive": true
  },
  "errors": []
}
```

### Update User
**PUT** `/api/v1/users/{id}`  
**Required Role**: Administrator

**Request Body**:
```json
{
  "username": "updated.user",
  "email": "updated.user@example.com",
  "fullName": "Updated User Name",
  "roleId": 3,
  "isActive": true
}
```

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "User updated successfully",
  "data": {
    "userId": "123e4567-e89b-12d3-a456-426614174000",
    "username": "updated.user",
    "email": "updated.user@example.com",
    "fullName": "Updated User Name",
    "roleName": "ProjectManager",
    "isActive": true
  },
  "errors": []
}
```

### Delete User
**DELETE** `/api/v1/users/{id}`  
**Required Role**: Administrator

**Response (204 No Content)**

---

## 🏗️ Project Management

**🔒 Authentication Required**

### Get All Projects
**GET** `/api/v1/projects`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10)
- `managerId` (GUID, optional): Filter by project manager ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "projectName": "Downtown Solar Installation",
        "address": "123 Main St, City, State 12345",
        "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)",
        "status": "In Progress",
        "startDate": "2025-05-01T00:00:00Z",
        "estimatedEndDate": "2025-07-15T00:00:00Z",
        "actualEndDate": null,
        "projectManager": {
          "userId": "123e4567-e89b-12d3-a456-426614174000",
          "username": "pm.jane",
          "email": "jane.pm@example.com",
          "fullName": "Jane Project Manager",
          "roleName": "ProjectManager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8
      }
    ],
    "totalCount": 12,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 2
  },
  "errors": []
}
```

### Get Project by ID
**GET** `/api/v1/projects/{id}`

**Parameters**:
- `id` (path, GUID): Project ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Project retrieved successfully",
  "data": {
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "projectName": "Downtown Solar Installation",
    "address": "123 Main St, City, State 12345",
    "clientInfo": "ABC Corp - Contact: John Smith (555-123-4567)",
    "status": "In Progress",
    "startDate": "2025-05-01T00:00:00Z",
    "estimatedEndDate": "2025-07-15T00:00:00Z",
    "actualEndDate": null,
    "projectManager": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "pm.jane",
      "email": "jane.pm@example.com",
      "fullName": "Jane Project Manager",
      "roleName": "ProjectManager",
      "isActive": true
    },
    "taskCount": 15,
    "completedTaskCount": 8
  },
  "errors": []
}
```

### Create Project
**POST** `/api/v1/projects`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "projectName": "New Solar Installation Project",
  "address": "456 Oak Ave, Another City, State 67890",
  "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-09-30T00:00:00Z",
  "projectManagerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "projectId": "660f8500-e29b-41d4-a716-446655440001",
    "projectName": "New Solar Installation Project",
    "address": "456 Oak Ave, Another City, State 67890",
    "clientInfo": "XYZ Corp - Contact: Sarah Johnson (555-987-6543)",
    "status": "Planning",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-09-30T00:00:00Z",
    "actualEndDate": null,
    "projectManager": {
      "userId": "123e4567-e89b-12d3-a456-426614174000",
      "username": "pm.jane",
      "email": "jane.pm@example.com",
      "fullName": "Jane Project Manager",
      "roleName": "ProjectManager",
      "isActive": true
    },
    "taskCount": 0,
    "completedTaskCount": 0
  },
  "errors": []
}
```

### Update Project
**PUT** `/api/v1/projects/{id}`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "projectName": "Updated Project Name",
  "address": "Updated Address",
  "clientInfo": "Updated Client Info",
  "status": "Completed",
  "startDate": "2025-05-01T00:00:00Z",
  "estimatedEndDate": "2025-07-15T00:00:00Z",
  "actualEndDate": "2025-07-10T00:00:00Z",
  "projectManagerId": "123e4567-e89b-12d3-a456-426614174000"
}
```

**Response (200 OK)**: Same structure as Create Project

### Delete Project
**DELETE** `/api/v1/projects/{id}`  
**Required Role**: Administrator

**Response (204 No Content)**

---

## 📋 Task Management

**🔒 Authentication Required**

### Get All Tasks
**GET** `/api/v1/tasks`

**Query Parameters**:
- `pageNumber` (integer, optional): Page number (default: 1)
- `pageSize` (integer, optional): Page size (default: 10, max: 100)
- `projectId` (GUID, optional): Filter by project ID
- `assigneeId` (GUID, optional): Filter by assignee ID

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Tasks retrieved successfully",
  "data": {
    "items": [
      {
        "taskId": "770g8600-e29b-41d4-a716-446655440002",
        "projectId": "550e8400-e29b-41d4-a716-446655440000",
        "projectName": "Downtown Solar Installation",
        "title": "Install solar panels on roof section A",
        "description": "Mount and wire solar panels on the eastern roof section",
        "status": "In Progress",
        "dueDate": "2025-06-20T00:00:00Z",
        "assignedTechnician": {
          "userId": "234f5678-e89b-12d3-a456-426614174001",
          "username": "tech.mike",
          "email": "mike.tech@example.com",
          "fullName": "Mike Technician",
          "roleName": "Technician",
          "isActive": true
        },
        "completionDate": null,
        "createdAt": "2025-05-15T00:00:00Z"
      }
    ],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3
  },
  "errors": []
}
```

### Get Task by ID
**GET** `/api/v1/tasks/{id}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Task retrieved successfully",
  "data": {
    "taskId": "770g8600-e29b-41d4-a716-446655440002",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "projectName": "Downtown Solar Installation",
    "title": "Install solar panels on roof section A",
    "description": "Mount and wire solar panels on the eastern roof section",
    "status": "In Progress",
    "dueDate": "2025-06-20T00:00:00Z",
    "assignedTechnician": {
      "userId": "234f5678-e89b-12d3-a456-426614174001",
      "username": "tech.mike",
      "email": "mike.tech@example.com",
      "fullName": "Mike Technician",
      "roleName": "Technician",
      "isActive": true
    },
    "completionDate": null,
    "createdAt": "2025-05-15T00:00:00Z"
  },
  "errors": []
}
```

### Create Task
**POST** `/api/v1/tasks`  
**Required Role**: Administrator, ProjectManager

**Request Body**:
```json
{
  "title": "Install inverter system",
  "description": "Install and configure the main inverter system",
  "dueDate": "2025-06-25T00:00:00Z",
  "assignedTechnicianId": "234f5678-e89b-12d3-a456-426614174001"
}
```

**Response (201 Created)**: Same structure as Get Task

### Update Task
**PUT** `/api/v1/tasks/{id}`

**Request Body**:
```json
{
  "title": "Updated task title",
  "description": "Updated task description",
  "status": "Completed",
  "dueDate": "2025-06-25T00:00:00Z",
  "assignedTechnicianId": "234f5678-e89b-12d3-a456-426614174001"
}
```

**Response (200 OK)**: Same structure as Get Task

### Delete Task
**DELETE** `/api/v1/tasks/{id}`  
**Required Role**: Administrator, ProjectManager

**Response (204 No Content)**

---

## 📸 Image Management

**🔒 Authentication Required**

### Upload Image
**POST** `/api/v1/images/upload`

**Content-Type**: `multipart/form-data`

**Form Parameters**:
- `file` (file, required): Image file to upload
- `projectId` (GUID, required): Project ID to associate with the image
- `taskId` (GUID, optional): Task ID to associate with the image
- `captureTimestamp` (datetime, optional): When the image was captured
- `gpsLatitude` (decimal, optional): GPS latitude coordinate
- `gpsLongitude` (decimal, optional): GPS longitude coordinate
- `deviceModel` (string, optional): Device model used to capture the image
- `exifData` (string, optional): EXIF metadata

**Example cURL**:
```bash
curl -X POST http://localhost:5002/api/v1/images/upload \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -F "file=@/path/to/image.jpg" \
  -F "projectId=550e8400-e29b-41d4-a716-446655440000" \
  -F "taskId=770g8600-e29b-41d4-a716-446655440002" \
  -F "captureTimestamp=2025-06-08T10:30:00Z" \
  -F "gpsLatitude=40.7128" \
  -F "gpsLongitude=-74.0060" \
  -F "deviceModel=iPhone 14 Pro"
```

**Response (201 Created)**:
```json
{
  "success": true,
  "message": "Image uploaded successfully",
  "data": {
    "imageId": "880h8700-e29b-41d4-a716-446655440003",
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "taskId": "770g8600-e29b-41d4-a716-446655440002",
    "originalFileName": "solar_panel_installation.jpg",
    "contentType": "image/jpeg",
    "fileSizeInBytes": 2048576,
    "uploadTimestamp": "2025-06-08T10:35:00Z",
    "captureTimestamp": "2025-06-08T10:30:00Z",
    "gpsLatitude": 40.7128,
    "gpsLongitude": -74.0060,
    "deviceModel": "iPhone 14 Pro",
    "imageUrl": "http://localhost:5002/files/images/880h8700-e29b-41d4-a716-446655440003.jpg",
    "uploadedBy": {
      "userId": "234f5678-e89b-12d3-a456-426614174001",
      "username": "tech.mike",
      "email": "mike.tech@example.com",
      "fullName": "Mike Technician",
      "roleName": "Technician",
      "isActive": true
    }
  },
  "errors": []
}
```

### Get Images by Project
**GET** `/api/v1/images/project/{projectId}`

**Response (200 OK)**:
```json
{
  "success": true,
  "message": "Images retrieved successfully",
  "data": [
    {
      "imageId": "880h8700-e29b-41d4-a716-446655440003",
      "projectId": "550e8400-e29b-41d4-a716-446655440000",
      "taskId": "770g8600-e29b-41d4-a716-446655440002",
      "originalFileName": "solar_panel_installation.jpg",
      "contentType": "image/jpeg",
      "fileSizeInBytes": 2048576,
      "uploadTimestamp": "2025-06-08T10:35:00Z",
      "captureTimestamp": "2025-06-08T10:30:00Z",
      "gpsLatitude": 40.7128,
      "gpsLongitude": -74.0060,
      "deviceModel": "iPhone 14 Pro",
      "imageUrl": "http://localhost:5002/files/images/880h8700-e29b-41d4-a716-446655440003.jpg",
      "uploadedBy": {
        "userId": "234f5678-e89b-12d3-a456-426614174001",
        "username": "tech.mike",
        "email": "mike.tech@example.com",
        "fullName": "Mike Technician",
        "roleName": "Technician",
        "isActive": true
      }
    }
  ],
  "errors": []
}
```

### Get Image by ID
**GET** `/api/v1/images/{id}`

**Response (200 OK)**: Same structure as single image from project list

### Delete Image
**DELETE** `/api/v1/images/{id}`

**Response (204 No Content)**

---

## ❌ Error Responses

### Common Error Formats

**400 Bad Request**:
```json
{
  "success": false,
  "message": "Invalid request data",
  "data": null,
  "errors": [
    "Username is required",
    "Password must be at least 8 characters"
  ]
}
```

**401 Unauthorized**:
```json
{
  "success": false,
  "message": "Authentication required",
  "data": null,
  "errors": ["Invalid or missing authorization token"]
}
```

**403 Forbidden**:
```json
{
  "success": false,
  "message": "Access denied",
  "data": null,
  "errors": ["Insufficient permissions for this operation"]
}
```

**404 Not Found**:
```json
{
  "success": false,
  "message": "Resource not found",
  "data": null,
  "errors": ["The requested resource does not exist"]
}
```

**500 Internal Server Error**:
```json
{
  "success": false,
  "message": "An internal server error occurred",
  "data": null,
  "errors": ["Please try again later or contact support"]
}
```

---

## 🛡️ Rate Limiting

The API implements rate limiting to ensure fair usage and protect against abuse.

### Rate Limit Tiers

| User Type | Requests per Minute | Burst Limit |
|-----------|-------------------|-------------|
| **Anonymous** | 60 | 10 |
| **Authenticated** | 300 | 50 |
| **Administrator** | 1000 | 100 |

### Rate Limit Headers

All responses include rate limiting information:

```http
X-RateLimit-Limit: 300
X-RateLimit-Remaining: 299
X-RateLimit-Reset: 1625567400
X-RateLimit-RetryAfter: 60
```

### Rate Limit Exceeded Response

**Response (429 Too Many Requests)**:
```json
{
  "success": false,
  "message": "Rate limit exceeded",
  "data": null,
  "errors": ["Too many requests. Please try again later."],
  "retryAfter": 60
}
```

### Best Practices

1. **Monitor Headers**: Check rate limit headers in responses
2. **Implement Backoff**: Use exponential backoff when rate limited
3. **Cache Responses**: Cache API responses to reduce request frequency
4. **Batch Operations**: Use bulk endpoints when available
5. **Use Websockets**: Consider real-time connections for frequent updates

---

## ⚡ Caching and Performance

The API implements intelligent caching to optimize performance for frequently accessed endpoints.

### Cached Endpoints

| Endpoint | Cache Duration | Cache Key Strategy |
|----------|----------------|-------------------|
| `GET /api/v1/daily-reports` | 5 minutes | Includes query parameters |
| `GET /api/v1/daily-reports/{id}` | 5 minutes | Based on report ID |
| `GET /api/v1/users` | 10 minutes | Includes pagination and filters |
| `GET /api/v1/projects` | 10 minutes | Includes pagination and filters |

### Cache Headers

Cached responses include standard HTTP cache headers:

```http
Cache-Control: public, max-age=300
ETag: "abc123def456"
Last-Modified: Mon, 09 Jun 2025 10:30:00 GMT
```

### Cache Invalidation

Cache is automatically invalidated when:
- Related data is modified (POST, PUT, DELETE operations)
- Cache duration expires
- Manual cache clear is triggered

### Performance Benefits

- **Reduced Database Load**: Frequently accessed data served from cache
- **Faster Response Times**: Sub-millisecond response for cached data
- **Improved Scalability**: Better handling of concurrent requests
- **Bandwidth Optimization**: ETags enable conditional requests

### Cache Status Metadata

Advanced query endpoints include cache status in response metadata:

```json
{
  "data": {
    "metadata": {
      "cacheStatus": "Hit|Miss|Expired",
      "executionTime": "00:00:00.1234567",
      "queryExecutedAt": "2025-06-09T10:30:00Z"
    }
  }
}
```

---

## 🔗 HATEOAS Implementation

**Hypermedia as the Engine of Application State (HATEOAS)** is implemented throughout the API to provide enhanced discoverability and navigation. Many responses include a `_links` section that provides relevant actions and related resources.

### HATEOAS in Daily Reports

Daily Reports include contextual links based on the current status:

#### Draft Status Links
```json
{
  "_links": {
    "self": { "href": "/api/v1/daily-reports/{id}" },
    "update": { "href": "/api/v1/daily-reports/{id}", "method": "PUT" },
    "submit": { "href": "/api/v1/daily-reports/{id}/submit", "method": "POST" },
    "delete": { "href": "/api/v1/daily-reports/{id}", "method": "DELETE" },
    "work-progress": { "href": "/api/v1/daily-reports/{id}/work-progress" },
    "personnel-logs": { "href": "/api/v1/daily-reports/{id}/personnel-logs" }
  }
}
```

#### Submitted Status Links
```json
{
  "_links": {
    "self": { "href": "/api/v1/daily-reports/{id}" },
    "approve": { "href": "/api/v1/daily-reports/{id}/approve", "method": "POST" },
    "reject": { "href": "/api/v1/daily-reports/{id}/reject", "method": "POST" },
    "work-progress": { "href": "/api/v1/daily-reports/{id}/work-progress" },
    "personnel-logs": { "href": "/api/v1/daily-reports/{id}/personnel-logs" }
  }
}
```

#### Approved/Rejected Status Links
```json
{
  "_links": {
    "self": { "href": "/api/v1/daily-reports/{id}" },
    "work-progress": { "href": "/api/v1/daily-reports/{id}/work-progress" },
    "personnel-logs": { "href": "/api/v1/daily-reports/{id}/personnel-logs" },
    "material-usage": { "href": "/api/v1/daily-reports/{id}/material-usage" },
    "equipment-logs": { "href": "/api/v1/daily-reports/{id}/equipment-logs" }
  }
}
```

### Benefits of HATEOAS

1. **Self-Documenting**: API responses indicate available actions
2. **Reduced Coupling**: Clients don't need to construct URLs
3. **Workflow Guidance**: Links guide users through business processes
4. **Version Resilience**: URL changes are automatically handled
5. **Enhanced UX**: Frontend applications can dynamically show available actions

---

## 🔍 Advanced Querying

All collection endpoints support advanced querying capabilities including filtering, sorting, and field selection. These features are available through dedicated "advanced" endpoints for enhanced flexibility and performance.

### Advanced Query Endpoints

Advanced querying is available on the following endpoints:
- **GET** `/api/v1/users/advanced` - Advanced user querying
- **GET** `/api/v1/projects/advanced` - Advanced project querying  
- **GET** `/api/v1/tasks/advanced` - Advanced task querying
- **GET** `/api/v1/images/project/{projectId}/advanced` - Advanced image querying

### Common Query Parameters

All advanced endpoints support these base parameters:

| Parameter | Type | Description | Default |
|-----------|------|-------------|---------|
| `pageNumber` | integer | Page number (1-based) | 1 |
| `pageSize` | integer | Items per page (1-100) | 10 |
| `sortBy` | string | Field name to sort by | - |
| `sortOrder` | string | Sort direction: "asc" or "desc" | "asc" |
| `fields` | string | Comma-separated field list for sparse fieldsets | - |

### Filtering

#### Method 1: Query String Filters
Use the format `filter.{field}.{operator}={value}`:

```
GET /api/v1/users/advanced?filter.fullName.contains=John&filter.isActive.eq=true
```

#### Method 2: Entity-Specific Parameters
Each entity has specific filter parameters:

**Users**: `username`, `email`, `fullName`, `role`, `isActive`
**Projects**: `projectName`, `status`, `clientInfo`, `address`, `managerId`, `startDateAfter`, `startDateBefore`
**Tasks**: `title`, `status`, `projectId`, `assigneeId`, `dueDateAfter`, `dueDateBefore`
**Images**: `taskId`, `uploadedById`, `contentType`, `deviceModel`, `minFileSize`, `maxFileSize`

#### Supported Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal to | `filter.status.eq=Active` |
| `ne` | Not equal to | `filter.status.ne=Completed` |
| `gt` | Greater than | `filter.startDate.gt=2024-01-01` |
| `gte` | Greater than or equal | `filter.startDate.gte=2024-01-01` |
| `lt` | Less than | `filter.endDate.lt=2024-12-31` |
| `lte` | Less than or equal | `filter.endDate.lte=2024-12-31` |
| `contains` | String contains | `filter.name.contains=Solar` |
| `startswith` | String starts with | `filter.name.startswith=Project` |
| `endswith` | String ends with | `filter.name.endswith=2024` |
| `in` | Value in list | `filter.status.in=Active,Planning` |

### Sorting

Sort results by any field using `sortBy` and `sortOrder`:

```
GET /api/v1/projects/advanced?sortBy=createdAt&sortOrder=desc
```

### Field Selection (Sparse Fieldsets)

Request only specific fields to reduce payload size:

```
GET /api/v1/users/advanced?fields=userId,username,email
```

### Example Advanced Queries

#### Complex Project Query
```
GET /api/v1/projects/advanced?
    filter.status.eq=InProgress&
    filter.startDate.gte=2024-01-01&
    sortBy=createdAt&
    sortOrder=desc&
    fields=projectId,projectName,status,startDate&
    pageSize=20
```

#### User Search with Pagination
```
GET /api/v1/users/advanced?
    filter.fullName.contains=John&
    filter.isActive.eq=true&
    sortBy=fullName&
    pageNumber=2&
    pageSize=10
```

### Enhanced Response Format

Advanced queries return enhanced results with metadata:

```json
{
  "success": true,
  "data": {
    "items": [...],
    "totalCount": 25,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 3,
    "hasNextPage": true,
    "hasPreviousPage": false,
    "sortBy": "createdAt",
    "sortOrder": "desc",
    "requestedFields": ["projectId", "projectName", "status"],
    "metadata": {
      "executionTime": "00:00:00.1234567",
      "filtersApplied": 2,
      "queryComplexity": "Medium",
      "queryExecutedAt": "2024-06-08T10:30:00Z",
      "cacheStatus": "Miss"
    }
  }
}
```

### Performance Tips

1. **Use field selection** to request only needed fields
2. **Add indexes** for frequently filtered/sorted fields
3. **Limit page size** to reasonable values (≤50 for complex queries)
4. **Use specific filters** rather than broad text searches when possible
5. **Consider caching** for repeated identical queries

---

## 🧪 Testing Examples

### Using cURL

**Login and get token**:
```bash
TOKEN=$(curl -s -X POST http://localhost:5002/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john.doe","password":"SecurePassword123!"}' \
  | jq -r '.data.token')
```

**Get projects with authentication**:
```bash
curl -X GET http://localhost:5002/api/v1/projects \
  -H "Authorization: Bearer $TOKEN"
```

**Create a new daily report**:
```bash
curl -X POST http://localhost:5002/api/v1/daily-reports \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "projectId": "550e8400-e29b-41d4-a716-446655440000",
    "reportDate": "2025-06-09T00:00:00Z",
    "workStartTime": "08:00:00",
    "workEndTime": "17:00:00",
    "weatherConditions": "Sunny, 75°F",
    "overallNotes": "Good progress on installation",
    "safetyNotes": "All safety protocols followed",
    "photosCount": 5
  }'
```

**Get daily reports with filtering**:
```bash
curl -X GET "http://localhost:5002/api/v1/daily-reports?status=Submitted&pageSize=20" \
  -H "Authorization: Bearer $TOKEN"
```

**Create a new todo (legacy)**:
```bash
curl -X POST http://localhost:5002/api/todo \
  -H "Content-Type: application/json" \
  -d '{"title":"New Todo","isCompleted":false,"dueDate":"2025-06-30T00:00:00Z"}'
```

### Using PowerShell

**Get health status**:
```powershell
Invoke-RestMethod -Uri "http://localhost:5002/health" -Method GET
```

**Login and store token**:
```powershell
$loginData = @{
    username = "john.doe"
    password = "SecurePassword123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5002/api/v1/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $response.data.token
```

**Get daily reports with token**:
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "http://localhost:5002/api/v1/daily-reports" -Method GET -Headers $headers
```

**Get projects with token**:
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "http://localhost:5002/api/v1/projects" -Method GET -Headers $headers
```

---

## 📝 API Features Summary

✅ **Authentication**: JWT-based login/register/refresh with role-based access control  
✅ **Daily Reports**: Comprehensive field reporting with workflow management (Draft → Submitted → Approved/Rejected)  
✅ **Work Requests**: Change orders and additional work tracking with priority management  
✅ **Work Progress Tracking**: Detailed progress items, personnel logs, material usage, and equipment logs  
✅ **HATEOAS Support**: Hypermedia links for enhanced API navigation and discoverability  
✅ **Advanced Caching**: 5-minute cache duration on frequently accessed endpoints for optimal performance  
✅ **Versioning**: API v1.0 with URL-based versioning strategy  
✅ **Authorization**: Role-based access control (Administrator, ProjectManager, Technician)  
✅ **Advanced Pagination**: Enhanced pagination with metadata and performance tracking  
✅ **Advanced Querying**: Complex filtering, sorting, and field selection on collection endpoints  
✅ **Error Handling**: Consistent error response format with detailed validation messages  
✅ **Health Monitoring**: Basic and detailed health checks with system metrics  
✅ **File Upload**: Multipart form data support for images with metadata and GPS coordinates  
✅ **Rate Limiting**: Built-in rate limiting for API protection  
✅ **CORS**: Cross-origin requests enabled for web application integration  
✅ **Swagger Documentation**: Interactive API explorer with comprehensive endpoint documentation  
✅ **Database**: PostgreSQL with Entity Framework Core and automated migrations  
✅ **Structured Logging**: Multi-level logging with correlation IDs and performance tracking  

---

**📚 Interactive Documentation**: Visit http://localhost:5002 for live Swagger UI  
**🚀 Application Status**: http://localhost:5002/health  
**🔧 Production URL**: https://solar-projects-api-dev.azurewebsites.net (after deployment)
