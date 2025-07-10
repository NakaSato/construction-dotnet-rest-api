# 🌞 Solar Projects REST API - Complete Documentation

## 📋 Table of Contents

1. [Overview](#overview)
2. [Quick Start](#quick-start)
3. [Authentication](#authentication)
4. [API Endpoints](#api-endpoints)
   - [Authentication & Users](#authentication--users)
   - [Projects](#projects)
   - [Tasks](#tasks)
   - [Master Plans](#master-plans)
   - [WBS (Work Breakdown Structure)](#wbs-work-breakdown-structure)
   - [Daily Reports](#daily-reports)
   - [Work Requests](#work-requests)
   - [Weekly Reports](#weekly-reports)
   - [Calendar](#calendar)
   - [Documents & Resources](#documents--resources)
   - [Images](#images)
   - [Notifications](#notifications)
   - [Dashboard](#dashboard)
   - [Health & Debug](#health--debug)
5. [Data Models](#data-models)
6. [Error Handling](#error-handling)
7. [Rate Limiting](#rate-limiting)
8. [Real-time Features (SignalR)](#real-time-features-signalr)

---

## Overview

The Solar Projects REST API is a comprehensive .NET 9.0 API designed for managing solar installation projects, field operations, and team collaboration. Built with Clean Architecture principles, it provides robust functionality for mobile and web applications.

### Key Features
- 🔐 JWT-based authentication with role-based access control
- 📊 Complete project lifecycle management
- 🔧 Work breakdown structure (WBS) support
- 📱 Mobile-optimized endpoints with image upload
- ⚡ Real-time notifications via SignalR
- 🚀 Built-in caching and rate limiting
- 📈 Advanced filtering and pagination

### Base Information
- **Framework**: .NET 9.0
- **API Version**: v1.0
- **Base URL**: `https://your-api-domain.com/api/v1`
- **Authentication**: JWT Bearer Token
- **Content-Type**: `application/json`
- **Date Format**: ISO 8601

---

## Quick Start

### 1. Health Check
```http
GET /health
```

### 2. Authenticate
```http
POST /api/v1/auth/login
Content-Type: application/json

{
  "username": "your_username",
  "password": "your_password"
}
```

### 3. Use Bearer Token
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

---

## Authentication

### Login
**POST** `/api/v1/auth/login`

Login with username or email address.

**Request Body:**
```json
{
  "username": "admin_user",  // Can be username or email
  "password": "SecurePass123!"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "refresh_token_here",
    "user": {
      "userId": "f1be98ee-7cd9-4ebe-b32e-73b4c67ba144",
      "username": "admin_user",
      "email": "admin@company.com",
      "fullName": "Admin User",
      "roleName": "Admin",
      "isActive": true
    }
  },
  "errors": []
}
```

### Register
**POST** `/api/v1/auth/register`

**Request Body:**
```json
{
  "username": "new_user",
  "email": "newuser@company.com",
  "password": "SecurePass123!",
  "fullName": "New User",
  "roleId": 3
}
```

**Role Hierarchy:**
| ID | Role | Access Level | Description |
|---|---|---|---|
| 1 | Admin | Full Access | Complete CRUD operations |
| 2 | Manager | Limited Admin | Create/Update projects, view all |
| 3 | User | Standard | View projects, submit reports |
| 4 | Viewer | Read-Only | View projects and reports only |

---

## API Endpoints

## Authentication & Users

### Get All Users
**GET** `/api/v1/users`
- **Authorization**: Admin, Manager
- **Cache**: 15 minutes

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `role` (string): Filter by role name

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "userId": "guid",
        "username": "user1",
        "email": "user1@company.com",
        "fullName": "User One",
        "roleName": "User",
        "isActive": true
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

### Get User by ID
**GET** `/api/v1/users/{id}`
- **Authorization**: Admin, Manager, or own profile

### Create User
**POST** `/api/v1/users`
- **Authorization**: Admin, Manager

**Request Body:**
```json
{
  "username": "new_user",
  "email": "newuser@company.com",
  "password": "SecurePass123!",
  "fullName": "New User",
  "roleId": 3
}
```

### Update User
**PUT** `/api/v1/users/{id}`
- **Authorization**: Admin, Manager, or own profile

### Deactivate User
**DELETE** `/api/v1/users/{id}`
- **Authorization**: Admin only

---

## Projects

### Get All Projects
**GET** `/api/v1/projects`
- **Authorization**: All authenticated users
- **Cache**: 15 minutes

**Advanced Query Parameters:**
- `pageNumber`, `pageSize`: Pagination
- `sortBy`: Field to sort by
- `sortOrder`: "asc" or "desc"
- `fields`: Comma-separated field selection
- `filter`: Dynamic filter string
- `search`: Search term
- `status`: Filter by status
- `managerId`: Filter by manager

**Example with filters:**
```http
GET /api/v1/projects?pageNumber=1&pageSize=20&sortBy=projectName&sortOrder=asc&status=Active&search=Solar
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "projectId": "guid",
        "projectName": "Solar Installation - Site A",
        "address": "123 Main St, City, State",
        "clientInfo": "ABC Company",
        "status": "Active",
        "startDate": "2024-01-15T00:00:00Z",
        "estimatedEndDate": "2024-06-15T00:00:00Z",
        "actualEndDate": null,
        "managerName": "Project Manager",
        "totalTasks": 25,
        "completedTasks": 10,
        "progressPercentage": 40.0
      }
    ],
    "pageNumber": 1,
    "pageSize": 20,
    "totalCount": 97,
    "totalPages": 5
  }
}
```

### Get Project by ID
**GET** `/api/v1/projects/{id}`

### Create Project
**POST** `/api/v1/projects`
- **Authorization**: Admin, Manager

**Request Body:**
```json
{
  "projectName": "New Solar Project",
  "address": "456 Oak Ave, City, State",
  "clientInfo": "XYZ Corporation",
  "status": "Planning",
  "startDate": "2024-07-01T00:00:00Z",
  "estimatedEndDate": "2024-12-01T00:00:00Z",
  "managerId": "manager-guid"
}
```

### Update Project
**PUT** `/api/v1/projects/{id}`
- **Authorization**: Admin, Manager

### Delete Project
**DELETE** `/api/v1/projects/{id}`
- **Authorization**: Admin only

---

## Tasks

### Get All Tasks
**GET** `/api/v1/tasks`
- **Authorization**: All authenticated users
- **Cache**: 5 minutes

**Query Parameters:**
- `pageNumber`, `pageSize`: Pagination
- `projectId`: Filter by project
- `assigneeId`: Filter by assignee
- `status`: Filter by status
- `priority`: Filter by priority

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "taskId": "guid",
        "taskName": "Install Solar Panels",
        "description": "Install 20 solar panels on roof",
        "projectId": "project-guid",
        "projectName": "Solar Project A",
        "assigneeId": "user-guid",
        "assigneeName": "John Technician",
        "status": "In Progress",
        "priority": "High",
        "startDate": "2024-07-01T00:00:00Z",
        "dueDate": "2024-07-15T00:00:00Z",
        "completedDate": null,
        "progressPercentage": 60.0
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 150
  }
}
```

### Get Task by ID
**GET** `/api/v1/tasks/{id}`

### Create Task
**POST** `/api/v1/tasks`
- **Authorization**: Admin, Manager

**Request Body:**
```json
{
  "taskName": "New Installation Task",
  "description": "Detailed task description",
  "projectId": "project-guid",
  "assigneeId": "user-guid",
  "status": "Not Started",
  "priority": "Medium",
  "startDate": "2024-07-20T00:00:00Z",
  "dueDate": "2024-07-25T00:00:00Z"
}
```

### Update Task
**PUT** `/api/v1/tasks/{id}`

### Update Task Progress
**PATCH** `/api/v1/tasks/{id}/progress`

**Request Body:**
```json
{
  "progressPercentage": 75.0,
  "notes": "Progress update notes"
}
```

### Delete Task
**DELETE** `/api/v1/tasks/{id}`
- **Authorization**: Admin, Manager

---

## Master Plans

### Get All Master Plans
**GET** `/api/v1/masterplans`

**Query Parameters:**
- `projectId`: Filter by project
- `status`: Filter by status
- `phaseId`: Filter by phase

### Get Master Plan by ID
**GET** `/api/v1/masterplans/{id}`

### Create Master Plan
**POST** `/api/v1/masterplans`

**Request Body:**
```json
{
  "projectId": "project-guid",
  "planName": "Q3 Implementation Plan",
  "description": "Quarterly implementation plan",
  "startDate": "2024-07-01T00:00:00Z",
  "endDate": "2024-09-30T00:00:00Z",
  "status": "Active",
  "phases": [
    {
      "phaseName": "Phase 1",
      "description": "Initial setup",
      "startDate": "2024-07-01T00:00:00Z",
      "endDate": "2024-07-31T00:00:00Z",
      "budget": 50000.00
    }
  ]
}
```

### Update Master Plan
**PUT** `/api/v1/masterplans/{id}`

### Update Phase Progress
**PATCH** `/api/v1/masterplans/{id}/phases/{phaseId}/progress`

---

## WBS (Work Breakdown Structure)

### Get WBS for Project
**GET** `/api/v1/wbs`

**Query Parameters:**
- `projectId` (required): Project ID
- `level`: WBS level depth

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "wbsId": "guid",
      "wbsCode": "1.1.1",
      "name": "Site Preparation",
      "description": "Prepare installation site",
      "level": 3,
      "parentWbsId": "parent-guid",
      "projectId": "project-guid",
      "startDate": "2024-07-01T00:00:00Z",
      "endDate": "2024-07-05T00:00:00Z",
      "budget": 15000.00,
      "actualCost": 12000.00,
      "status": "Completed",
      "children": []
    }
  ]
}
```

### Create WBS Item
**POST** `/api/v1/wbs`

**Request Body:**
```json
{
  "projectId": "project-guid",
  "parentWbsId": "parent-guid",
  "wbsCode": "1.2.1",
  "name": "Equipment Installation",
  "description": "Install solar equipment",
  "startDate": "2024-07-10T00:00:00Z",
  "endDate": "2024-07-20T00:00:00Z",
  "budget": 25000.00
}
```

### Update WBS Item
**PUT** `/api/v1/wbs/{id}`

### Delete WBS Item
**DELETE** `/api/v1/wbs/{id}`

---

## Daily Reports

### Get All Daily Reports
**GET** `/api/v1/dailyreports`

**Query Parameters:**
- `projectId`: Filter by project
- `reportDate`: Filter by date
- `reportedById`: Filter by reporter

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "reportId": "guid",
        "projectId": "project-guid",
        "projectName": "Solar Project A",
        "reportDate": "2024-07-15T00:00:00Z",
        "reportedById": "user-guid",
        "reportedByName": "John Technician",
        "workDescription": "Installed 10 solar panels",
        "hoursWorked": 8.0,
        "issuesEncountered": "Minor weather delay",
        "safetyNotes": "All safety protocols followed",
        "weatherConditions": "Partly cloudy",
        "equipmentUsed": "Drill, mounting hardware",
        "materialUsed": "Solar panels, wiring",
        "images": [
          {
            "imageId": "image-guid",
            "fileName": "progress_photo.jpg",
            "description": "Installation progress"
          }
        ]
      }
    ]
  }
}
```

### Create Daily Report
**POST** `/api/v1/dailyreports`

**Request Body:**
```json
{
  "projectId": "project-guid",
  "reportDate": "2024-07-15T00:00:00Z",
  "workDescription": "Continued panel installation",
  "hoursWorked": 8.0,
  "issuesEncountered": "None",
  "safetyNotes": "All safety measures observed",
  "weatherConditions": "Sunny",
  "equipmentUsed": "Standard installation tools",
  "materialUsed": "Solar panels, mounting hardware"
}
```

### Update Daily Report
**PUT** `/api/v1/dailyreports/{id}`

### Delete Daily Report
**DELETE** `/api/v1/dailyreports/{id}`

---

## Work Requests

### Get All Work Requests
**GET** `/api/v1/workrequests`

**Query Parameters:**
- `projectId`: Filter by project
- `status`: Filter by status
- `priority`: Filter by priority
- `requestedById`: Filter by requester

### Create Work Request
**POST** `/api/v1/workrequests`

**Request Body:**
```json
{
  "projectId": "project-guid",
  "title": "Additional Wiring Required",
  "description": "Need additional electrical wiring for panel array",
  "priority": "High",
  "requestedDate": "2024-07-15T00:00:00Z",
  "requiredDate": "2024-07-20T00:00:00Z",
  "estimatedHours": 16.0,
  "estimatedCost": 2500.00
}
```

### Update Work Request Status
**PATCH** `/api/v1/workrequests/{id}/status`

**Request Body:**
```json
{
  "status": "Approved",
  "notes": "Approved for implementation"
}
```

---

## Weekly Reports

## Weekly Reports

### Get All Weekly Reports
**GET** `/api/v1/weeklyreports`
- **Authorization**: All authenticated users
- **Cache**: 10 minutes

**Query Parameters:**
- `pageNumber`, `pageSize`: Pagination
- `projectId`: Filter by project
- `status`: Filter by status (Draft, Submitted, Approved, Rejected)
- `weekStartAfter`: Filter reports after date
- `weekStartBefore`: Filter reports before date
- `submittedById`: Filter by submitter
- `minActualHours`, `maxActualHours`: Filter by hours worked
- `minCompletionPercentage`, `maxCompletionPercentage`: Filter by completion

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "weeklyReportId": "guid",
        "projectId": "project-guid",
        "projectName": "Solar Installation - Site A",
        "weekStartDate": "2024-07-01T00:00:00Z",
        "status": "Approved",
        "summaryOfProgress": "Completed installation of 25 solar panels on Building A roof. Weather conditions were favorable throughout the week.",
        "aggregatedMetrics": {
          "totalManHours": 40,
          "panelsInstalled": 25,
          "safetyIncidents": 0,
          "delaysReported": 1
        },
        "majorAccomplishments": [
          "Successfully installed 25 solar panels",
          "Completed electrical connections for Building A",
          "Passed safety inspection"
        ],
        "majorIssues": [
          {
            "issueId": "issue-guid",
            "description": "Unexpected roof structural concerns",
            "impact": "Minor delay in installation schedule",
            "resolution": "Structural engineer consulted, modifications approved",
            "resolvedAt": "2024-07-03T14:00:00Z"
          }
        ],
        "lookahead": "Next week focus on Building B installation and system testing",
        "submittedBy": {
          "userId": "user-guid",
          "username": "john_tech",
          "fullName": "John Technician"
        },
        "approvedBy": {
          "userId": "manager-guid",
          "username": "manager_1",
          "fullName": "Site Manager"
        },
        "approvedAt": "2024-07-05T10:00:00Z",
        "createdAt": "2024-07-05T08:00:00Z",
        "updatedAt": "2024-07-05T10:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 15,
    "totalPages": 2
  }
}
```

### Get Weekly Report by ID
**GET** `/api/v1/weeklyreports/{id}`

### Create Weekly Report
**POST** `/api/v1/weeklyreports`
- **Authorization**: All authenticated users

**Request Body:**
```json
{
  "projectId": "project-guid",
  "weekStartDate": "2024-07-08T00:00:00Z",
  "summaryOfProgress": "Installation progress for week of July 8th. Focused on Building C solar panel installation.",
  "aggregatedMetrics": {
    "totalManHours": 35,
    "panelsInstalled": 20,
    "safetyIncidents": 0,
    "delaysReported": 0
  },
  "majorAccomplishments": [
    "Completed Building C panel installation",
    "Finished inverter connections",
    "Conducted system performance tests"
  ],
  "majorIssues": [
    {
      "description": "Weather delays due to heavy rain",
      "impact": "2-hour work stoppage on Tuesday",
      "resolution": "Resumed work after weather cleared"
    }
  ],
  "lookahead": "Complete final system integration and documentation",
  "submittedById": "user-guid",
  "completionPercentage": 85,
  "totalManHours": 35,
  "panelsInstalled": 20,
  "safetyIncidents": 0,
  "delaysReported": 0
}
```

### Update Weekly Report
**PUT** `/api/v1/weeklyreports/{id}`
- **Authorization**: Report submitter or Admin/Manager

### Update Weekly Report Status
**PATCH** `/api/v1/weeklyreports/{id}/status`
- **Authorization**: Admin, Manager

**Request Body:**
```json
{
  "status": "Approved",
  "comments": "Report approved with commendation for safety compliance"
}
```

### Delete Weekly Report
**DELETE** `/api/v1/weeklyreports/{id}`
- **Authorization**: Admin, Manager, or report submitter (if not approved)

### Get Project Weekly Reports
**GET** `/api/v1/projects/{projectId}/weeklyreports`

---

## Weekly Work Requests

### Get All Weekly Work Requests
**GET** `/api/v1/weeklyworkrequests`
- **Authorization**: All authenticated users
- **Cache**: 5 minutes

**Query Parameters:**
- `pageNumber`, `pageSize`: Pagination
- `projectId`: Filter by project
- `status`: Filter by status (Draft, Submitted, Approved, Rejected, In Progress, Completed)
- `weekStartAfter`: Filter requests after date
- `weekStartBefore`: Filter requests before date
- `requestedById`: Filter by requester
- `minEstimatedHours`, `maxEstimatedHours`: Filter by estimated hours
- `priority`: Filter by priority (Low, Medium, High, Critical)
- `type`: Filter by request type

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "weeklyRequestId": "guid",
        "projectId": "project-guid",
        "projectName": "Solar Installation - Site A",
        "weekStartDate": "2024-07-08T00:00:00Z",
        "status": "Approved",
        "overallGoals": "Complete solar panel installation for Building D and conduct system integration testing",
        "keyTasks": [
          "Install 30 solar panels on Building D roof",
          "Connect DC wiring from panels to inverters",
          "Conduct system performance and safety tests",
          "Complete documentation and handover preparation"
        ],
        "resourceForecast": {
          "personnel": "3 electricians, 1 safety supervisor, 1 project coordinator",
          "majorEquipment": "Crane, safety harnesses, electrical testing equipment",
          "criticalMaterials": "Solar panels (30 units), DC cables, mounting hardware, safety equipment"
        },
        "requestedBy": {
          "userId": "user-guid",
          "username": "site_manager",
          "fullName": "Site Manager"
        },
        "approvedBy": {
          "userId": "admin-guid",
          "username": "project_admin",
          "fullName": "Project Administrator"
        },
        "approvedAt": "2024-07-06T15:00:00Z",
        "createdAt": "2024-07-05T14:00:00Z",
        "updatedAt": "2024-07-06T15:00:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 8,
    "totalPages": 1
  }
}
```

### Get Weekly Work Request by ID
**GET** `/api/v1/weeklyworkrequests/{id}`

### Create Weekly Work Request
**POST** `/api/v1/weeklyworkrequests`
- **Authorization**: All authenticated users

**Request Body:**
```json
{
  "projectId": "project-guid",
  "weekStartDate": "2024-07-15T00:00:00Z",
  "overallGoals": "Complete final phase of solar installation project including system commissioning and documentation",
  "keyTasks": [
    "Finalize electrical connections and testing",
    "Conduct comprehensive system commissioning",
    "Complete project documentation",
    "Prepare for final inspection"
  ],
  "personnelForecast": "2 senior electricians, 1 commissioning engineer, 1 documentation specialist",
  "majorEquipment": "Advanced testing equipment, commissioning tools, safety gear",
  "criticalMaterials": "Final connection hardware, testing cables, documentation materials",
  "requestedById": "user-guid",
  "estimatedHours": 32,
  "priority": "High",
  "type": "Final Phase",
  "resourceForecast": {
    "personnel": "2 senior electricians, 1 commissioning engineer, 1 documentation specialist",
    "majorEquipment": "Advanced testing equipment, commissioning tools, safety gear",
    "criticalMaterials": "Final connection hardware, testing cables, documentation materials"
  }
}
```

### Update Weekly Work Request
**PUT** `/api/v1/weeklyworkrequests/{id}`
- **Authorization**: Request creator or Admin/Manager

### Update Weekly Work Request Status
**PATCH** `/api/v1/weeklyworkrequests/{id}/status`
- **Authorization**: Admin, Manager

**Request Body:**
```json
{
  "status": "Approved",
  "comments": "Request approved. Ensure safety protocols are followed for final phase work."
}
```

### Delete Weekly Work Request
**DELETE** `/api/v1/weeklyworkrequests/{id}`
- **Authorization**: Admin, Manager, or request creator (if not approved)

### Get Project Weekly Work Requests
**GET** `/api/v1/projects/{projectId}/weeklyworkrequests`

### Weekly Work Request Status Flow
```
Draft → Submitted → Approved/Rejected → In Progress → Completed
```

**Status Descriptions:**
- **Draft**: Work request being prepared
- **Submitted**: Submitted for approval
- **Approved**: Approved and ready for execution
- **Rejected**: Not approved, needs revision
- **In Progress**: Work is actively being performed
- **Completed**: All tasks completed successfully

### Weekly Report Status Flow
```
Draft → Submitted → Approved/Rejected
```

**Status Descriptions:**
- **Draft**: Report being prepared
- **Submitted**: Submitted for review
- **Approved**: Report accepted and approved
- **Rejected**: Report needs revision or additional information

---

## Calendar

### Get Calendar Events
**GET** `/api/v1/calendar/events`

**Query Parameters:**
- `startDate`: Start date filter
- `endDate`: End date filter
- `projectId`: Filter by project

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "eventId": "guid",
      "title": "Site Inspection",
      "description": "Monthly site inspection",
      "startDate": "2024-07-20T09:00:00Z",
      "endDate": "2024-07-20T11:00:00Z",
      "projectId": "project-guid",
      "projectName": "Solar Project A",
      "eventType": "Inspection",
      "isAllDay": false
    }
  ]
}
```

### Create Calendar Event
**POST** `/api/v1/calendar/events`

---

## Documents & Resources

### Get Project Documents
**GET** `/api/v1/documents`

**Query Parameters:**
- `projectId`: Filter by project
- `documentType`: Filter by type

### Upload Document
**POST** `/api/v1/documents/upload`

**Form Data:**
- `file`: Document file
- `projectId`: Associated project
- `documentType`: Document category
- `description`: Document description

---

## Images

### Upload Image
**POST** `/api/v1/images/upload`

**Form Data:**
- `file`: Image file
- `description`: Image description
- `projectId`: Associated project (optional)
- `taskId`: Associated task (optional)
- `reportId`: Associated report (optional)
- `latitude`: GPS latitude (optional)
- `longitude`: GPS longitude (optional)

**Response:**
```json
{
  "success": true,
  "data": {
    "imageId": "guid",
    "fileName": "image_20240715_143022.jpg",
    "originalFileName": "site_photo.jpg",
    "filePath": "/uploads/images/2024/07/image_20240715_143022.jpg",
    "fileSize": 2048576,
    "contentType": "image/jpeg",
    "description": "Site progress photo",
    "uploadedAt": "2024-07-15T14:30:22Z",
    "metadata": {
      "latitude": 40.7128,
      "longitude": -74.0060,
      "deviceInfo": "iPhone 14 Pro",
      "appVersion": "1.0.0"
    }
  }
}
```

### Get Image
**GET** `/api/v1/images/{id}`

### Delete Image
**DELETE** `/api/v1/images/{id}`

---

## Notifications

### Get User Notifications
**GET** `/api/v1/notifications`
- **Authorization**: All authenticated users
- **Cache**: No cache (real-time data)

**Query Parameters:**
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `isRead` (boolean): Filter by read status
- `type` (string): Filter by notification type
- `priority` (string): Filter by priority (Low, Medium, High, Critical)
- `dateFrom` (datetime): Filter notifications from date
- `dateTo` (datetime): Filter notifications to date

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "notificationId": "guid",
        "type": "ProjectStatusChanged",
        "title": "Project Status Updated",
        "message": "Project 'Solar Installation Site A' status changed to Completed",
        "priority": "High",
        "data": {
          "projectId": "project-guid",
          "oldStatus": "In Progress",
          "newStatus": "Completed",
          "completionPercentage": 100
        },
        "isRead": false,
        "createdAt": "2024-07-07T14:30:00Z",
        "readAt": null,
        "expiresAt": "2024-07-14T14:30:00Z"
      }
    ],
    "pageNumber": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3,
    "unreadCount": 8
  }
}
```

### Get Notification by ID
**GET** `/api/v1/notifications/{id}`
- **Authorization**: Own notifications only

**Response:**
```json
{
  "success": true,
  "data": {
    "notificationId": "guid",
    "type": "WeeklyReportCreated",
    "title": "Weekly Report Submitted",
    "message": "Weekly report for Project Alpha has been submitted by John Technician",
    "priority": "Medium",
    "data": {
      "weeklyReportId": "report-guid",
      "projectId": "project-guid",
      "submittedBy": "John Technician",
      "weekStartDate": "2024-07-01T00:00:00Z"
    },
    "isRead": false,
    "createdAt": "2024-07-07T15:00:00Z"
  }
}
```

### Mark Notification as Read
**PATCH** `/api/v1/notifications/{id}/read`
- **Authorization**: Own notifications only

**Response:**
```json
{
  "success": true,
  "message": "Notification marked as read",
  "data": {
    "notificationId": "guid",
    "isRead": true,
    "readAt": "2024-07-07T15:30:00Z"
  }
}
```

### Mark All Notifications as Read
**PATCH** `/api/v1/notifications/read-all`
- **Authorization**: All authenticated users

**Response:**
```json
{
  "success": true,
  "message": "All notifications marked as read",
  "data": {
    "markedAsReadCount": 12,
    "timestamp": "2024-07-07T15:30:00Z"
  }
}
```

### Get Notification Count
**GET** `/api/v1/notifications/count`
- **Authorization**: All authenticated users

**Response:**
```json
{
  "success": true,
  "data": {
    "totalCount": 25,
    "unreadCount": 8,
    "priorityCounts": {
      "critical": 1,
      "high": 3,
      "medium": 12,
      "low": 9
    },
    "typeCounts": {
      "ProjectStatusChanged": 5,
      "TaskAssigned": 3,
      "WeeklyReportCreated": 4,
      "WorkRequestCreated": 2,
      "DailyReportSubmitted": 11
    }
  }
}
```

### Delete Notification
**DELETE** `/api/v1/notifications/{id}`
- **Authorization**: Own notifications only

### Bulk Mark as Read
**PATCH** `/api/v1/notifications/bulk-read`
- **Authorization**: All authenticated users

**Request Body:**
```json
{
  "notificationIds": [
    "guid1",
    "guid2",
    "guid3"
  ]
}
```

### Get Notification Settings
**GET** `/api/v1/notifications/settings`
- **Authorization**: All authenticated users

**Response:**
```json
{
  "success": true,
  "data": {
    "emailNotifications": true,
    "pushNotifications": true,
    "smsNotifications": false,
    "preferences": {
      "projectUpdates": true,
      "taskAssignments": true,
      "reportSubmissions": true,
      "workRequests": true,
      "systemAnnouncements": true,
      "weeklyReports": true,
      "approvalRequests": true
    },
    "quietHours": {
      "enabled": true,
      "startTime": "22:00",
      "endTime": "07:00",
      "timezone": "Asia/Bangkok"
    }
  }
}
```

### Update Notification Settings
**PUT** `/api/v1/notifications/settings`
- **Authorization**: All authenticated users

**Request Body:**
```json
{
  "emailNotifications": true,
  "pushNotifications": true,
  "smsNotifications": false,
  "preferences": {
    "projectUpdates": true,
    "taskAssignments": true,
    "reportSubmissions": false,
    "workRequests": true,
    "systemAnnouncements": true,
    "weeklyReports": true,
    "approvalRequests": true
  },
  "quietHours": {
    "enabled": true,
    "startTime": "23:00",
    "endTime": "06:00",
    "timezone": "Asia/Bangkok"
  }
}
```

## Notification Types and Real-time Events

### Project Notifications
- **ProjectCreated**: New project created
- **ProjectUpdated**: Project information updated
- **ProjectDeleted**: Project removed
- **ProjectStatusChanged**: Project status transition
- **ProjectLocationUpdated**: GPS coordinates or address changed

### Task Notifications
- **TaskCreated**: New task assigned
- **TaskUpdated**: Task information modified
- **TaskDeleted**: Task removed
- **TaskStatusChanged**: Task status updated
- **TaskAssigned**: Task assigned to user
- **TaskCompleted**: Task marked as completed

### Report Notifications
- **DailyReportCreated**: Daily report submitted
- **DailyReportUpdated**: Daily report modified
- **DailyReportDeleted**: Daily report removed
- **DailyReportApprovalChanged**: Approval status updated
- **WeeklyReportCreated**: Weekly report submitted
- **WeeklyReportUpdated**: Weekly report modified
- **WeeklyReportDeleted**: Weekly report removed

### Work Request Notifications
- **WorkRequestCreated**: New work request submitted
- **WorkRequestUpdated**: Work request modified
- **WorkRequestDeleted**: Work request cancelled
- **WorkRequestAssigned**: Work request assigned
- **WorkRequestCompleted**: Work request finished
- **ApprovalRequired**: Work request needs approval
- **WeeklyWorkRequestCreated**: Weekly work request submitted
- **WeeklyWorkRequestUpdated**: Weekly work request modified

### User and System Notifications
- **UserCreated**: New user account created
- **UserUpdated**: User profile updated
- **UserRoleChanged**: User permissions modified
- **SystemAnnouncement**: Important system message
- **MaintenanceScheduled**: System maintenance notice

### Calendar and Schedule Notifications
- **CalendarEventCreated**: New event scheduled
- **CalendarEventUpdated**: Event details changed
- **CalendarEventReminder**: Upcoming event reminder
- **EventConflict**: Schedule conflict detected

### Resource and Document Notifications
- **DocumentUploaded**: New document added
- **DocumentDeleted**: Document removed
- **ResourceCreated**: New resource available
- **ResourceUpdated**: Resource information changed

### Master Plan and WBS Notifications
- **MasterPlanCreated**: New master plan created
- **MasterPlanUpdated**: Master plan modified
- **WbsTaskCreated**: WBS task added
- **WbsTaskUpdated**: WBS task modified
- **PhaseCompleted**: Project phase finished

## Real-time Notification Integration

### SignalR Events for Notifications
```javascript
// Listen for real-time notifications
connection.on("ReceiveNotification", (notification) => {
    displayNotification(notification);
    updateNotificationCount();
    playNotificationSound(notification.priority);
});

// Listen for notification count updates
connection.on("NotificationCountUpdated", (count) => {
    updateNotificationBadge(count.unreadCount);
});

// Listen for system announcements
connection.on("SystemAnnouncement", (announcement) => {
    displaySystemAnnouncement(announcement);
});

// Listen for weekly report notifications
connection.on("WeeklyReportCreated", (reportData) => {
    if (userRole === "Manager" || userRole === "Admin") {
        showWeeklyReportNotification(reportData);
    }
});

// Listen for work request notifications
connection.on("WeeklyWorkRequestCreated", (requestData) => {
    if (userRole === "Manager" || userRole === "Admin") {
        showWorkRequestNotification(requestData);
    }
});
```

### Weekly Planning Notifications

#### Weekly Work Request Events
```javascript
connection.on("WeeklyWorkRequestCreated", (data) => {
    // {
    //   weeklyRequestId: "guid",
    //   projectName: "Solar Project A",
    //   weekStartDate: "2024-07-08T00:00:00Z",
    //   requestedBy: "John Technician",
    //   overallGoals: "Complete panel installation",
    //   estimatedHours: 40
    // }
    showNotification("New Weekly Work Request", `${data.requestedBy} submitted weekly work request for ${data.projectName}`);
});

connection.on("WeeklyWorkRequestStatusChanged", (data) => {
    // {
    //   weeklyRequestId: "guid",
    //   projectName: "Solar Project A",
    //   oldStatus: "Pending",
    //   newStatus: "Approved",
    //   approvedBy: "Manager Name"
    // }
    showNotification("Work Request Status Update", `Weekly work request for ${data.projectName} has been ${data.newStatus.toLowerCase()}`);
});
```

#### Weekly Report Events
```javascript
connection.on("WeeklyReportSubmitted", (data) => {
    // {
    //   weeklyReportId: "guid",
    //   projectName: "Solar Project A",
    //   weekStartDate: "2024-07-01T00:00:00Z",
    //   submittedBy: "John Technician",
    //   summaryOfProgress: "Installation progress summary",
    //   aggregatedMetrics: {
    //     totalManHours: 40,
    //     panelsInstalled: 25,
    //     safetyIncidents: 0
    //   }
    // }
    showWeeklyReportNotification(data);
});
```

### Notification Priority Levels
- **Critical**: System failures, security alerts, safety incidents
- **High**: Project status changes, task deadlines, approval requests
- **Medium**: Task assignments, report submissions, document uploads
- **Low**: Informational updates, reminders, general announcements

### Mobile Push Notifications
For mobile applications, notifications support:
- **Rich Media**: Images, project photos, progress charts
- **Action Buttons**: Approve/Reject, Mark as Read, View Details
- **Deep Linking**: Direct navigation to relevant screens
- **Offline Queueing**: Notifications delivered when app comes online
- **Geographic Targeting**: Location-based project notifications

---

## Dashboard

### Get Dashboard Data
**GET** `/api/v1/dashboard`

**Response:**
```json
{
  "success": true,
  "data": {
    "totalProjects": 97,
    "activeProjects": 45,
    "completedProjects": 30,
    "totalTasks": 1250,
    "completedTasks": 680,
    "overdueTasks": 25,
    "recentActivity": [
      {
        "activityType": "TaskCompleted",
        "description": "Task 'Install Panel Array' completed",
        "timestamp": "2024-07-15T14:30:00Z",
        "projectName": "Solar Project A"
      }
    ],
    "projectProgress": [
      {
        "projectId": "guid",
        "projectName": "Solar Project A",
        "progressPercentage": 75.0,
        "status": "On Track"
      }
    ]
  }
}
```

---

## Health & Debug

### Health Check
**GET** `/health`

**Response:**
```json
{
  "status": "Healthy",
  "timestamp": "2024-07-15T14:30:00Z",
  "version": "1.0.0",
  "environment": "Production",
  "dependencies": {
    "database": "Healthy",
    "redis": "Healthy"
  }
}
```

### Debug Information
**GET** `/api/v1/debug/info`
- **Authorization**: Admin only

---

## Data Models

### Standard API Response
```json
{
  "success": boolean,
  "message": string,
  "data": object | null,
  "errors": string[]
}
```

### Paginated Response
```json
{
  "items": array,
  "pageNumber": number,
  "pageSize": number,
  "totalCount": number,
  "totalPages": number,
  "hasPreviousPage": boolean,
  "hasNextPage": boolean
}
```

### Project Model
```json
{
  "projectId": "guid",
  "projectName": "string",
  "address": "string",
  "clientInfo": "string",
  "status": "Planning|Active|On Hold|Completed|Cancelled",
  "startDate": "datetime",
  "estimatedEndDate": "datetime",
  "actualEndDate": "datetime",
  "managerId": "guid",
  "managerName": "string",
  "budget": "decimal",
  "actualCost": "decimal",
  "progressPercentage": "decimal"
}
```

### Task Model
```json
{
  "taskId": "guid",
  "taskName": "string",
  "description": "string",
  "projectId": "guid",
  "assigneeId": "guid",
  "status": "Not Started|In Progress|Completed|On Hold|Cancelled",
  "priority": "Low|Medium|High|Critical",
  "startDate": "datetime",
  "dueDate": "datetime",
  "completedDate": "datetime",
  "progressPercentage": "decimal",
  "estimatedHours": "decimal",
  "actualHours": "decimal"
}
```

---

## Error Handling

### HTTP Status Codes
- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `409` - Conflict
- `422` - Validation Error
- `429` - Rate Limit Exceeded
- `500` - Internal Server Error

### Error Response Format
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": ["Detailed error messages"],
  "error": {
    "code": "ERROR_CODE",
    "details": "Additional error information"
  }
}
```

### Validation Error Example
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    "Project name is required",
    "Start date must be in the future"
  ],
  "error": {
    "validationErrors": {
      "projectName": ["Project name is required"],
      "startDate": ["Start date must be in the future"]
    }
  }
}
```

---

## Rate Limiting

### Default Limits
- **General API**: 50 requests per minute per user
- **Image Upload**: 10 requests per minute per user
- **Authentication**: 5 requests per minute per IP

### Rate Limit Headers
```
X-RateLimit-Limit: 50
X-RateLimit-Remaining: 45
X-RateLimit-Reset: 2024-07-15T14:31:00Z
```

### Rate Limit Exceeded Response
```json
{
  "success": false,
  "message": "Rate limit exceeded",
  "data": null,
  "errors": ["Too many requests"],
  "error": {
    "rateLimit": {
      "limit": 50,
      "resetTime": "2024-07-15T14:31:00Z",
      "retryAfter": "00:01:00"
    }
  }
}
```

---

## Real-time Features (SignalR)

### Connection Setup
**Hub URL**: `/notificationHub`
**Authentication**: JWT Bearer Token required
**Auto-Connection**: Users automatically join personal and role groups on connect

### JavaScript/TypeScript Client Setup
```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub", {
        accessTokenFactory: () => localStorage.getItem("jwt_token")
    })
    .build();

// Start connection
await connection.start();

// Connection confirmation
connection.on("Connected", (data) => {
    console.log("Connected to SignalR:", data);
});
```

### Core Features

#### 1. Project Management Groups

**Join Project Group**
```javascript
await connection.invoke("JoinProjectGroup", projectId);

// Listen for join confirmation
connection.on("JoinedProjectGroup", (projectId) => {
    console.log(`Joined project ${projectId}`);
});

// Listen for other users joining
connection.on("UserJoinedProject", (data) => {
    console.log(`${data.UserName} joined project ${data.ProjectId}`);
});
```

**Leave Project Group**
```javascript
await connection.invoke("LeaveProjectGroup", projectId);

connection.on("LeftProjectGroup", (projectId) => {
    console.log(`Left project ${projectId}`);
});
```

**Project Status Updates**
```javascript
connection.on("ProjectStatusChanged", (data) => {
    console.log("Project status updated:", data);
    // Update UI with new status and completion percentage
});

// Send status update (Admin/Manager only)
await connection.invoke("UpdateProjectStatus", projectId, "Completed", 100.0);
```

#### 2. Real-time Daily Report Collaboration

**Join Report Editing Session**
```javascript
await connection.invoke("JoinDailyReportSession", reportId);

connection.on("JoinedReportSession", (reportId) => {
    console.log(`Joined report session ${reportId}`);
});

connection.on("UserJoinedReportSession", (data) => {
    console.log(`${data.UserName} is now editing this report`);
});
```

**Real-time Field Updates**
```javascript
// Send field updates
await connection.invoke("UpdateReportField", reportId, "workDescription", "Updated description");

// Receive field updates from other editors
connection.on("ReportFieldUpdated", (data) => {
    console.log(`${data.UpdatedByName} updated ${data.FieldName}:`, data.Value);
    // Update the field in real-time
    document.getElementById(data.FieldName).value = data.Value;
});
```

**Typing Indicators**
```javascript
// Send typing indicator
await connection.invoke("SendTypingIndicator", reportId, "workDescription", true);

// Receive typing indicators
connection.on("UserTyping", (data) => {
    if (data.IsTyping) {
        showTypingIndicator(`${data.UserName} is typing in ${data.FieldName}...`);
    } else {
        hideTypingIndicator(data.UserName);
    }
});
```

#### 3. Geographic and Facility Groups

**Region-based Updates**
```javascript
// Join region group for regional project updates
await connection.invoke("JoinRegionGroup", "northern");
await connection.invoke("JoinRegionGroup", "western");

connection.on("JoinedRegionGroup", (region) => {
    console.log(`Subscribed to ${region} region updates`);
});
```

**Facility Type Groups**
```javascript
// Join facility-specific groups
await connection.invoke("JoinFacilityGroup", "solar_installation");
await connection.invoke("JoinFacilityGroup", "water_treatment");
```

**Map Integration**
```javascript
// Join map viewers for location updates
await connection.invoke("JoinMapViewersGroup");

// Receive real-time location updates
connection.on("ProjectLocationUpdated", (data) => {
    updateMapMarker(data.ProjectId, data.Coordinates, data.Address);
});

// Send location updates (from field workers)
await connection.invoke("UpdateProjectLocation", 
    projectId, 
    40.7128,    // latitude
    -74.0060,   // longitude
    "123 Main St, New York, NY"
);
```

#### 4. Role-based Notifications

**Automatic Role Groups**
Users automatically join role groups on connection:
- `role_admin` - Administrators
- `role_manager` - Managers  
- `role_user` - Standard users
- `role_viewer` - View-only users

**User Groups**
```javascript
// Automatically joined on connection
connection.on("JoinedUserGroup", (userId) => {
    console.log(`Joined personal notification group`);
});

connection.on("JoinedRoleGroup", (role) => {
    console.log(`Joined ${role} role group`);
});
```

#### 5. Project Communication

**Real-time Messaging**
```javascript
// Send project message
await connection.invoke("SendProjectMessage", projectId, "Site inspection completed successfully");

// Receive project messages
connection.on("ProjectMessage", (data) => {
    displayMessage({
        sender: data.SenderName,
        message: data.Message,
        timestamp: data.Timestamp,
        projectId: data.ProjectId
    });
});
```

#### 6. Notification Management

**Mark Notifications as Read**
```javascript
await connection.invoke("MarkNotificationRead", notificationId);

connection.on("NotificationMarkedRead", (notificationId) => {
    removeNotificationFromUI(notificationId);
});
```

**Get Notification Count**
```javascript
await connection.invoke("GetNotificationCount");

connection.on("NotificationCountUpdated", (count) => {
    updateNotificationBadge(count);
});
```

### Event Types and Data Structures

#### Connection Events
```javascript
connection.on("Connected", (data) => {
    // { UserId, UserName, Role, ConnectionId, Timestamp }
});
```

#### Project Events
```javascript
connection.on("ProjectStatusChanged", (data) => {
    // { ProjectId, NewStatus, CompletionPercentage, UpdatedBy, Timestamp }
});

connection.on("UserJoinedProject", (data) => {
    // { UserId, UserName, ProjectId, Timestamp }
});
```

#### Report Collaboration Events
```javascript
connection.on("ReportFieldUpdated", (data) => {
    // { ReportId, FieldName, Value, UpdatedBy, UpdatedByName, Timestamp }
});

connection.on("UserTyping", (data) => {
    // { ReportId, FieldName, UserId, UserName, IsTyping, Timestamp }
});
```

#### Location Events
```javascript
connection.on("ProjectLocationUpdated", (data) => {
    // { ProjectId, Coordinates: { Latitude, Longitude }, Address, UpdatedBy, Timestamp }
});
```

### Mobile Implementation Example (React Native)

```javascript
import { HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

class SignalRService {
    constructor() {
        this.connection = null;
        this.isConnected = false;
    }

    async connect(jwtToken) {
        this.connection = new HubConnectionBuilder()
            .withUrl(`${API_BASE_URL}/notificationHub`, {
                accessTokenFactory: () => jwtToken,
                skipNegotiation: true,
                transport: HttpTransportType.WebSockets
            })
            .withAutomaticReconnect()
            .configureLogging(LogLevel.Information)
            .build();

        await this.connection.start();
        this.isConnected = true;
        
        this.setupEventHandlers();
    }

    setupEventHandlers() {
        // Project updates
        this.connection.on("ProjectStatusChanged", (data) => {
            NotificationService.showProjectUpdate(data);
        });

        // Real-time collaboration
        this.connection.on("ReportFieldUpdated", (data) => {
            ReportStore.updateField(data.ReportId, data.FieldName, data.Value);
        });

        // Location updates for map
        this.connection.on("ProjectLocationUpdated", (data) => {
            MapStore.updateProjectLocation(data);
        });
    }

    async joinProject(projectId) {
        if (this.isConnected) {
            await this.connection.invoke("JoinProjectGroup", projectId);
        }
    }

    async sendLocationUpdate(projectId, latitude, longitude, address) {
        if (this.isConnected) {
            await this.connection.invoke("UpdateProjectLocation", 
                projectId, latitude, longitude, address);
        }
    }
}
```

### Error Handling

```javascript
connection.onclose((error) => {
    console.error("SignalR connection closed:", error);
    // Implement reconnection logic
});

connection.onreconnecting((error) => {
    console.warn("SignalR reconnecting:", error);
    showReconnectingIndicator();
});

connection.onreconnected((connectionId) => {
    console.log("SignalR reconnected:", connectionId);
    hideReconnectingIndicator();
    // Rejoin groups
    rejoinAllGroups();
});
```

### Performance Considerations

- **Automatic Reconnection**: Built-in reconnection on network issues
- **Group Management**: Efficient group-based message delivery
- **Authorization**: JWT-based security for all connections
- **Scalability**: Supports multiple server instances with Redis backplane
- **Bandwidth Optimization**: Structured data formats, minimal payload sizes

### Security Features

- **JWT Authentication**: Required for all connections
- **Role-based Access**: Automatic role group assignment
- **User Isolation**: Personal notification groups
- **Project-level Security**: Users only receive updates for authorized projects
- **Connection Logging**: Comprehensive audit trail

This SignalR implementation provides comprehensive real-time functionality for collaborative daily reports, live project updates, location tracking, and instant notifications across your solar project management platform.

---

## Development Notes

### Caching Strategy
- **Short Cache (5 min)**: Dynamic data (tasks, reports)
- **Medium Cache (15 min)**: Semi-static data (projects, users)
- **Long Cache (1 hour)**: Static data (roles, settings)

### Performance Considerations
- Use pagination for large datasets
- Implement field selection with `fields` parameter
- Leverage caching headers for client-side caching
- Use compression for large responses

### Security Features
- JWT token authentication
- Role-based authorization
- Rate limiting per user/endpoint
- Input validation and sanitization
- CORS policy configuration

### Mobile Optimization
- Optimized response sizes
- Image compression and resizing
- GPS metadata support
- Offline-friendly data structures
- Progressive data loading

---

This documentation covers all major endpoints and features of the Solar Projects REST API. For implementation examples and additional details, refer to the specific endpoint documentation files in the `/docs/api/` directory.
