# 📊 Daily Reports

**⚡ Real-Time Live Updates**: Daily reports support real-time collaborative editing with live field updates, typing indicators, and instant approval notifications.

**🔒 Authentication Required: JWT Bearer Token**  
**📡 Live Updates**: SignalR WebSocket broadcasting for collaborative editing  
**🎯 Role Requirements**: 
- **Admin, Manager**: Full access to all operations
- **ProjectManager**: Project-specific management access
- **User**: Create and manage own reports
- **All authenticated users**: View approved reports

Daily reports provide comprehensive tracking of daily work activities, progress, safety compliance, and resource utilization within the context of a specific **Project**. Each Daily Report belongs to a Project and serves as a detailed work log, progress tracker, and compliance record for solar installation projects. Daily reports support advanced workflow management, approval processes, real-time collaborative editing, and AI-powered insights for project optimization.

## 🔐 Authorization & Access Control

The Daily Reports API implements JWT-based authentication with role-based access control:

### 🔑 **Login Required**
All endpoints require a valid JWT token obtained from the authentication endpoint.

You need to:
1. Log in with admin account credentials
2. Use username or email flexibility options
3. Include the token in the Authorization header for all API requests

*Code example removed - see implementation guide*

**Default Test Accounts:**
- **Admin**: `admin@example.com` / `Admin123!` (Full system access)
- **Manager**: `manager@example.com` / `Manager123!` (Management access)
- **User**: `user@example.com` / `User123!` (Limited access)

**Login Flexibility**: The system accepts either username or email in the same login field.

### 👥 **Role-Based Permissions**
- **🔐 [Authorize]**: All endpoints require authentication
- **🔐 [Authorize(Roles = "Administrator,Manager")]**: Admin/Manager only
- **🔐 [Authorize(Roles = "Administrator,Manager,ProjectManager")]**: Management roles
- **🔐 [Authorize(Roles = "Administrator,ProjectManager")]**: Admin/Project Manager only

## 🏗️ Project-Daily Report Relationship

- **Project** is the primary business entity containing high-level project information and constraints
- **Daily Report** belongs to a Project (many:1 relationship) and captures comprehensive daily work activities
- A Project can have multiple Daily Reports (typically one per team member per day, or one consolidated per day)
- Daily Reports integrate with the project's **Master Plan** for progress tracking and schedule validation
- Daily Reports inherit project context (location, team, objectives, safety requirements) from their parent Project
- All reporting, analytics, and compliance tracking is organized by Project for comprehensive oversight
- Daily Reports contribute to project-level progress calculations and milestone tracking

## ⚡ Daily Report Capabilities

### 🔐 Administrator & Manager (Full System Access)
- ✅ **Full CRUD Operations**: Create, read, update, delete any daily report
- ✅ **Cross-Project Access**: View and manage reports across all projects
- ✅ **Advanced Analytics**: Access comprehensive analytics and AI insights
- ✅ **Workflow Management**: Approve/reject reports with bulk operations
- ✅ **Export & Reporting**: Generate reports in multiple formats (PDF, Excel, CSV)
- ✅ **Template Management**: Configure report templates and validation rules
- ✅ **User Management**: Manage team assignments and permissions
- ✅ **Audit Operations**: Full access to approval history and audit trails

### 🏗️ Project Manager (Project-Scoped Access)
- ✅ **Project-Specific Management**: Full control within assigned projects
- ✅ **Team Report Oversight**: View and approve team member reports
- ✅ **Project Analytics**: Access project-specific insights and trends
- ✅ **Approval Authority**: Approve/reject reports for managed projects
- ✅ **Export Project Data**: Generate project-specific reports and analytics
- ✅ **Template Configuration**: Customize templates for managed projects
- ❌ **Cross-Project Access**: Limited to assigned projects only

### 👷 User/Technician (Individual Access)
- ✅ **Personal Reports**: Create and manage own daily reports
- ✅ **Photo Attachments**: Add progress photos and documentation
- ✅ **Status Tracking**: Submit reports for approval workflow
- ✅ **View Own History**: Access personal report history and status
- ✅ **Template Guidance**: Use project-specific report templates
- ❌ **Others' Reports**: Cannot view or modify other users' reports
- ❌ **Management Functions**: No approval or administrative capabilities
- ❌ **Cross-Project Data**: Limited to assigned project reports

### 👀 Viewer (Read-Only Access)
- ✅ **View Approved Reports**: Read-only access to approved daily reports
- ✅ **Basic Analytics**: View project progress summaries and charts
- ✅ **Public Information**: Access general project status and milestones
- ❌ **Report Creation**: Cannot create or modify reports
- ❌ **Workflow Actions**: No approval or administrative capabilities
- ❌ **Sensitive Data**: Limited access to detailed operational information

## 🎯 Daily Report Features & Capabilities

### 📋 Comprehensive Data Capture
- **Work Progress Tracking**: Detailed activity logging with time allocation and completion percentages
- **Safety & Quality Metrics**: Standardized scoring systems with incident reporting
- **Weather Impact Assessment**: Weather condition logging with impact analysis on work progress
- **Resource Utilization**: Personnel, equipment, and material usage tracking with cost implications
- **Photo Documentation**: Progress photos with metadata, GPS coordinates, and categorization
- **Issue & Risk Management**: Incident reporting with severity classification and resolution tracking

### 🔄 Advanced Workflow Management
- **Approval Workflows**: Configurable multi-level approval processes with role-based permissions
- **Template-Based Creation**: Project-specific templates ensuring consistent data collection
- **Validation Engine**: Real-time data validation with AI-powered suggestions and corrections
- **Bulk Operations**: Mass approval, rejection, and export capabilities for efficiency

### 📊 Analytics & Insights
- **AI-Powered Analysis**: Automated insights generation with performance recommendations
- **Progress Analytics**: Real-time progress tracking with trend analysis and forecasting
- **Safety Analytics**: Safety performance monitoring with risk assessment and compliance tracking
- **Resource Optimization**: Equipment and personnel utilization analysis with cost optimization suggestions

### 🔗 Integration Capabilities
- **Master Plan Integration**: Direct linkage to project schedules and milestone tracking
- **Task Progress Sync**: Automatic task completion updates based on daily report submissions
- **Calendar Integration**: Schedule coordination with project timelines and resource allocation
- **Notification System**: Automated alerts for approvals, issues, and milestone achievements

## 🚀 API Overview

The Daily Reports API provides comprehensive endpoints for managing daily work reports with JWT authentication and role-based authorization. All endpoints require a valid Bearer token.

### � **Base URL & Authentication**
```
Base URL: http://localhost:5001/api/v1/daily-reports
Authentication: Bearer JWT Token
Content-Type: application/json
```

### 📖 **Core CRUD Operations**
| Endpoint | Method | Authorization | Description |
|----------|--------|---------------|-------------|
| `/` | GET | All Users | Get all daily reports with filtering |
| `/{id}` | GET | All Users | Get specific daily report by ID |
| `/` | POST | All Users | Create new daily report |
| `/enhanced` | POST | All Users | Create enhanced daily report with validation |
| `/{id}` | PUT | Owner/Admin/Manager | Update existing daily report |
| `/{id}` | DELETE | Admin/Manager | Delete daily report |

### 🔄 **Workflow Management**
| Endpoint | Method | Authorization | Description |
|----------|--------|---------------|-------------|
| `/{id}/submit` | POST | Owner/Admin/Manager | Submit report for approval |
| `/{id}/approve` | POST | Admin/ProjectManager | Approve submitted report |
| `/{id}/reject` | POST | Admin/ProjectManager | Reject submitted report |
| `/pending-approval` | GET | Admin/Manager/ProjectManager | Get pending approvals |
| `/{reportId}/approval-history` | GET | Admin/Manager/ProjectManager | Get approval history |

### 📊 **Analytics & Insights**
| Endpoint | Method | Authorization | Description |
|----------|--------|---------------|-------------|
| `/projects/{projectId}/analytics` | GET | Admin/Manager/ProjectManager | Get project analytics |
| `/projects/{projectId}/weekly-report` | GET | Admin/Manager/ProjectManager | Generate weekly report |
| `/projects/{projectId}/insights` | GET | Admin/Manager/ProjectManager | Get AI-powered insights |
| `/weekly-summary` | GET | Admin/Manager | Get weekly summary |

### 🔧 **Bulk Operations**
| Endpoint | Method | Authorization | Description |
|----------|--------|---------------|-------------|
| `/bulk-approve` | POST | Admin/Manager/ProjectManager | Bulk approve reports |
| `/bulk-reject` | POST | Admin/Manager/ProjectManager | Bulk reject reports |
| `/export` | GET | Admin/Manager | Export reports (basic) |
| `/export-enhanced` | POST | Admin/Manager/ProjectManager | Export with analytics |

### 📎 **File Management**
| Endpoint | Method | Authorization | Description |
|----------|--------|---------------|-------------|
| `/{id}/attachments` | POST | Owner/Admin/Manager | Add file attachment |
| `/validate` | POST | All Users | Validate report data |
| `/projects/{projectId}/templates` | GET | All Users | Get project templates |

## 📋 Get All Daily Reports

**GET** `/api/v1/daily-reports`  
**🔒 Authorization**: Bearer JWT Token (All authenticated users)

Retrieve daily reports with advanced filtering options.

**Headers**:
*Authentication headers removed - requires Bearer token and application/json content type*

**Query Parameters**:
- `projectId` (Guid): Filter by specific project
- `userId` (Guid): Filter by reporting user
- `startDate` (DateTime): Filter from date (YYYY-MM-DD or ISO format)
- `endDate` (DateTime): Filter to date (YYYY-MM-DD or ISO format)
- `approvalStatus` (string): Filter by approval status (Draft, Submitted, Approved, Rejected, RevisionRequired)
- `minSafetyScore` (int): Minimum safety score filter (1-10)
- `minQualityScore` (int): Minimum quality score filter (1-10)
- `weatherCondition` (string): Filter by weather condition
- `hasCriticalIssues` (bool): Filter reports with critical issues
- `sortBy` (string): Sort field (reportDate, safetyScore, qualityScore, hoursWorked, createdAt)
- `sortDirection` (string): Sort direction (asc, desc)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)

**Success Response (200)**:

**Response Structure:**
- `success`: Boolean indicating success status
- `message`: Response message
- `data`: Object containing daily reports information
  - `reports`: Array of daily report objects
    - `id`: Unique report identifier
    - `projectId`: Associated project ID
    - `projectName`: Name of the project
    - `userId`: Report creator user ID
    - `userName`: Name of the report creator
    - `reportDate`: Date of the report
    - `approvalStatus`: Current approval status (e.g., "Approved", "Submitted", "Rejected")
    - `hoursWorked`: Number of hours worked
    - `personnelOnSite`: Number of personnel on site
    - `weatherConditions`: Weather condition description
    - `temperature`: Temperature value
    - `humidity`: Humidity percentage
    - `summary`: Report summary text
    - `issues`: Issues encountered
    - `safetyScore`: Safety score (1-10)
    - `qualityScore`: Quality score (1-10)
    - `dailyProgressContribution`: Progress contribution percentage
    - `hasCriticalIssues`: Boolean indicating presence of critical issues
    - `tasksCompleted`: Array of completed tasks
    - `createdAt`: Report creation timestamp
    - `hasAttachments`: Boolean indicating attachment presence

*JSON response example removed - see implementation guide*
      {
        "id": "234f6789-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "890f1234-e89b-12d3-a456-426614174002",
        "userName": "Sarah Electrician",
        "reportDate": "2025-06-14",
        "approvalStatus": "Submitted",
        "hoursWorked": 7.0,
        "personnelOnSite": 3,
        "weatherConditions": "Sunny, 85°F",
        "temperature": 85.0,
        "humidity": 45,
        "summary": "Completed electrical wiring for inverter connections",
        "issues": "Permit inspector delayed until tomorrow",
        "safetyScore": 9,
        "qualityScore": 8,
        "dailyProgressContribution": 8.5,
        "hasCriticalIssues": false,
        "tasksCompleted": ["Wire inverters", "Test connections"],
        "createdAt": "2025-06-14T16:45:00Z",
        "hasAttachments": false
      }
    ],
    "pagination": {
      "totalCount": 45,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 5,
      "hasPreviousPage": false,
      "hasNextPage": true
    },
    "summary": {
      "totalReports": 45,
      "averageSafetyScore": 9.2,
      "averageQualityScore": 8.8,
      "totalHoursLogged": 382.5,
      "totalProgressContribution": 95.5,
      "criticalIssuesCount": 2,
      "pendingApprovals": 8
    }
  },
  "errors": []
}
```

**Error Responses**:

**401 Unauthorized**:
- `success`: false
- `message`: "Unauthorized"
- `errors`: Array containing error messages

**403 Forbidden**:
- `success`: false
- `message`: "Insufficient permissions"
- `errors`: Array containing detailed error messages

*JSON error examples removed - see implementation guide*

## 🔍 Get Daily Report by ID

**GET** `/api/v1/daily-reports/{id}`  
**🔒 Authorization**: Bearer JWT Token (All authenticated users)

Retrieve details of a specific daily report.

**Headers**:
*Authentication headers removed - requires Bearer token and application/json content type*

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
**Response Structure**:
- `success`: Boolean indicating success
- `message`: Response message
- `data`: Daily report object containing:
  - Report identification details (id, projectId, userName)
  - Report content (summary, issues, tasks completed)
  - Approval information (status, approver, timestamp)
  - Weather conditions and site information
  - Performance metrics (safety score, quality score)

*JSON response example removed - see implementation guide*
    "hoursWorked": 8.5,
    "personnelOnSite": 4,
    "weatherConditions": "Sunny, 85°F",
    "temperature": 85.0,
    "humidity": 45,
    "windSpeed": 8.5,
    "weatherImpact": "None - ideal conditions",
    "summary": "Completed installation of 24 solar panels on south-facing roof section",
    "workAccomplished": "Successfully installed mounting rails and 24 high-efficiency solar panels",
    "workPlannedNextDay": "Begin electrical connections and inverter setup",
    "issues": "None",
    "safetyScore": 10,
    "qualityScore": 9,
    "dailyProgressContribution": 12.5,
    "hasCriticalIssues": false,
    "requiresManagerAttention": false,
    "tasksCompleted": [
      {
        "taskId": "task1-id",
        "title": "Install mounting rails",
        "completionPercentage": 100
      },
      {
        "taskId": "task2-id",
        "title": "Install panels in section A",
        "completionPercentage": 100
      }
    ],
    "materialsUsed": [
      {
        "name": "Mounting rails",
        "quantity": 12,
        "unit": "pieces"
      },
      {
        "name": "Solar panels",
        "quantity": 24,
        "unit": "panels"
      }
    ],
    "attachments": [
      {
        "id": "att1-id",
        "fileName": "completed_installation.jpg",
        "fileType": "image/jpeg",
        "fileSize": 1540000,
        "uploadedAt": "2025-06-14T17:25:00Z",
        "thumbnailUrl": "/api/v1/attachments/att1-id/thumbnail"
      }
    ],
    "createdAt": "2025-06-14T17:30:00Z",
    "updatedAt": null
  },
  "errors": []
}
```

## 📝 Create Daily Report

**POST** `/api/v1/daily-reports`  
**🔒 Authorization**: Bearer JWT Token (All authenticated users)

Create a new daily report for work completed.

**Headers**:
*Authentication headers removed - requires Bearer token and application/json content type*

**Request Body**:
*JSON request body removed - includes the following fields:*

- `projectId`: Project identifier (GUID)
- `reportDate`: Date of the report (YYYY-MM-DD)
- `hoursWorked`: Number of hours worked
- `personnelOnSite`: Count of personnel present
- `weatherConditions`: Text description of weather
- `temperature`: Temperature in degrees Fahrenheit
- `humidity`: Humidity percentage
- `windSpeed`: Wind speed in mph
- `weatherImpact`: Description of weather impact on work
- `summary`: Brief summary of daily work
- `workAccomplished`: Detailed work description
- `workPlannedNextDay`: Next day planning
- `issues`: Issues or challenges encountered
- `safetyScore`: Safety rating (0-10)
- `qualityScore`: Quality rating (0-10)
- `dailyProgressContribution`: Progress percentage
- `additionalNotes`: Any additional information
- `tasksCompleted`: List of completed tasks with details
- `materialsUsed`: List of materials with quantities and costs

**Success Response (201)**:
*JSON success response removed - includes the following fields:*

- `success`: Operation status (boolean)
- `message`: Confirmation message
- `data`: Created daily report object with:
  - `id`: Report identifier (GUID)
  - `projectId`: Associated project ID
  - `projectName`: Project name
  - `reportDate`: Date of the report
  - `approvalStatus`: Current status (e.g., "Draft")
  - `hoursWorked`: Number of hours worked
  - `safetyScore`: Safety rating (0-10)
  - `qualityScore`: Quality rating (0-10)
  - `dailyProgressContribution`: Progress percentage
  - `createdAt`: Creation timestamp
  - `canEdit`: User permission to edit
  - `canSubmit`: User permission to submit
- `errors`: Array of validation errors (empty on success)

## 🔄 Update Daily Report

**PUT** `/api/v1/daily-reports/{id}`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Roles**: Admin, Manager, or report creator

Update an existing daily report. Users can only update their own reports within a configurable time limit (default: 24 hours). Admins and Managers can update any report.

**Headers**:
*Authentication headers removed - requires Bearer token and application/json content type*

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
*JSON request body removed - includes fields similar to the create endpoint that need to be updated*
  "issues": "Waiting for city inspector clearance before final connection. Weather delays.",
  "tasksCompleted": [
    {
      "taskId": "task3-id",
      "completionPercentage": 100
    },
    {
      "taskId": "task4-id",
      "completionPercentage": 85
    }
  ],
  "materialsUsed": [
    {
      "name": "Electrical cable",
      "quantity": 175,
      "unit": "feet"
    },
    {
      "name": "Conduit",
      "quantity": 45,
      "unit": "feet"
    }
  ]
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report updated successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "hoursWorked": 9.5,
    "updatedAt": "2025-06-15T19:30:00Z"
  },
  "errors": []
}
```

## 🗑️ Delete Daily Report

**DELETE** `/api/v1/daily-reports/{id}`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Required Roles**: Administrator, Manager

Delete a daily report. Only Administrators and Managers can delete reports.

**Headers**:
*Authentication headers removed - requires Bearer token*

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
*JSON success response removed - includes confirmation of successful deletion with empty data field*

## 🔄 Report Approval Workflow

### Submit Report for Approval

**POST** `/api/v1/daily-reports/{id}/submit`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Roles**: Report creator, Admin, Manager

Submit a daily report for management approval.

**Headers**:
*Authentication headers removed - requires Bearer token and application/json content type*

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
*JSON request body removed - includes submission comments field*

**Success Response (200)**:
*JSON success response removed - includes confirmation of submission with report ID, status, timestamp, and submitter information*

### Approve Daily Report

**POST** `/api/v1/daily-reports/{id}/approve`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Required Roles**: Administrator, ProjectManager

Approve a submitted daily report.

**Headers**:
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
```json
{
  "comments": "Excellent work quality and safety compliance. Approved for record."
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report approved successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "approvalStatus": "Approved",
    "approvedBy": "Sarah Manager",
    "approvedAt": "2025-06-15T20:30:00Z",
    "approvalComments": "Excellent work quality and safety compliance. Approved for record."
  },
  "errors": []
}
```

### Reject Daily Report

**POST** `/api/v1/daily-reports/{id}/reject`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Required Roles**: Administrator, ProjectManager

Reject a submitted daily report with reason.

**Headers**:
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
```

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
```json
{
  "rejectionReason": "Insufficient detail in work summary. Please provide more specific information about tasks completed and any safety incidents."
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report rejected",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "approvalStatus": "Rejected",
    "rejectedBy": "Sarah Manager",
    "rejectedAt": "2025-06-15T20:15:00Z",
    "rejectionReason": "Insufficient detail in work summary. Please provide more specific information about tasks completed and any safety incidents."
  },
  "errors": []
}
```

## 📎 Add Attachment to Report

**POST** `/api/v1/daily-reports/{id}/attachments`  
**🔒 Authorization**: Bearer JWT Token  
**🎯 Roles**: Admin, Manager, or report creator

Add a photo or document attachment to a daily report.

**Headers**:
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: multipart/form-data
```

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
- Multipart form data with file

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Attachment added successfully",
  "data": {
    "id": "att2-id",
    "fileName": "inverter_installation.jpg",
    "fileType": "image/jpeg",
    "fileSize": 2100000,
    "uploadedAt": "2025-06-15T19:45:00Z",
    "thumbnailUrl": "/api/v1/attachments/att2-id/thumbnail"
  },
  "errors": []
}
```

## 📊 Export Daily Reports

**GET** `/api/v1/daily-reports/export`

**🔒 Required Roles**: Admin, Manager

Export daily reports in various formats.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `startDate` (DateTime): Start date
- `endDate` (DateTime): End date
- `format` (string): Export format ("csv", "excel", "pdf")

**Success Response (200)**:
- File download response with appropriate content type

## 📑 Weekly Summary Report

**GET** `/api/v1/daily-reports/weekly-summary`

**🔒 Required Roles**: Admin, Manager

Get summary of hours, progress, and issues for a weekly period.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `weekStartDate` (DateTime): Beginning of week

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly summary retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "weekStarting": "2025-06-09",
    "weekEnding": "2025-06-15",
    "totalHoursLogged": 187.5,
    "totalTasksCompleted": 12,
    "overallProgress": 68,
    "dailySummaries": [
      {
        "date": "2025-06-09",
        "totalHours": 36.0,
        "employeeCount": 4,
        "majorTasks": ["Site preparation", "Material delivery"]
      },
      // More daily summaries...
    ],
    "issuesSummary": [
      {
        "category": "Weather",
        "occurrences": 2,
        "hoursImpacted": 8.0
      },
      {
        "category": "Permits",
        "occurrences": 1,
        "hoursImpacted": 4.0
      }
    ]
  },
  "errors": []
}
```

## 🏗️ Enhanced Project-Centric Daily Reports

### Get Project Daily Reports

**GET** `/api/v1/daily-reports/projects/{projectId}`

Retrieve daily reports for a specific project with enhanced filtering and analytics.

**Path Parameters**:
- `projectId` (Guid): Project ID

**Query Parameters**:
- `startDate` (DateTime): Filter from date
- `endDate` (DateTime): Filter to date
- `approvalStatuses` (array): Filter by approval status
- `reporterIds` (array): Filter by reporter IDs
- `minSafetyScore` (int): Minimum safety score filter
- `minQualityScore` (int): Minimum quality score filter
- `hasCriticalIssues` (bool): Filter reports with critical issues
- `includeAnalytics` (bool): Include analytics data
- `includeInsights` (bool): Include AI insights
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project daily reports retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "projectCode": "SIP-2025-001",
        "projectLocation": "Austin, TX",
        "clientName": "Green Energy Solutions",
        "reportDate": "2025-06-29",
        "approvalStatus": "Approved",
        "reporterId": "789e0123-e89b-12d3-a456-426614174002",
        "reporterName": "John Technician",
        "totalWorkHours": 8.5,
        "personnelOnSite": 4,
        "weatherCondition": "Sunny",
        "temperature": 85.0,
        "humidity": 45,
        "weatherDescription": "Clear skies, ideal working conditions",
        "weatherImpact": "None",
        "workSummary": "Completed installation of 24 solar panels on south-facing roof section",
        "workAccomplished": "Installation of mounting rails and 24 solar panels",
        "workPlannedNextDay": "Electrical connections and inverter installation",
        "issuesEncountered": "None",
        "safetyScore": 10,
        "qualityScore": 9,
        "dailyProgressContribution": 12.5,
        "overallProgressPercentage": 68.0,
        "hasCriticalIssues": false,
        "requiresManagerAttention": false,
        "teamMembers": [
          {
            "userId": "user1-id",
            "name": "John Technician",
            "role": "Lead Installer",
            "hoursWorked": 8.5,
            "specialAssignments": "Panel alignment verification"
          }
        ],
        "taskProgressSummary": [
          {
            "taskId": "task1-id",
            "taskName": "Install mounting rails",
            "startPercentage": 0,
            "endPercentage": 100,
            "progressMade": 100,
            "status": "Completed",
            "isOnSchedule": true
          }
        ],
        "attachments": [
          {
            "id": "att1-id",
            "fileName": "installation_progress.jpg",
            "fileType": "image/jpeg",
            "category": "Photo",
            "uploadedAt": "2025-06-29T16:30:00Z",
            "thumbnailUrl": "/api/v1/attachments/att1-id/thumbnail"
          }
        ],
        "photoCount": 3,
        "documentCount": 1,
        "autoGeneratedInsights": [
          "Excellent progress rate - 25% above average",
          "Weather conditions optimal for solar installation",
          "Team efficiency high - completing tasks ahead of schedule"
        ],
        "recommendedActions": [
          "Continue current pace to maintain schedule",
          "Consider additional safety check for tomorrow's electrical work"
        ],
        "createdAt": "2025-06-29T17:00:00Z",
        "lastModifiedAt": "2025-06-29T17:30:00Z"
      }
    ],
    "pagination": {
      "totalCount": 25,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 3,
      "hasPreviousPage": false,
      "hasNextPage": true
    },
    "analytics": {
      "averageDailyProgress": 11.2,
      "averageSafetyScore": 9.4,
      "averageQualityScore": 8.8,
      "totalCriticalIssues": 2,
      "weatherImpactDays": 3
    }
  },
  "errors": []
}
```

### Create Enhanced Daily Report

**POST** `/api/v1/daily-reports/enhanced`

Create a comprehensive daily report with enhanced validation and project context.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "reportDate": "2025-06-29",
  "weatherCondition": "Sunny",
  "temperature": 85.0,
  "humidity": 45,
  "windSpeed": 12.0,
  "weatherDescription": "Clear blue skies with light breeze",
  "weatherImpact": "None",
  "workSummary": "Completed installation of 24 solar panels on south-facing roof section with high efficiency and safety standards",
  "workAccomplished": "Successfully installed mounting rails and 24 high-efficiency solar panels",
  "workPlannedNextDay": "Electrical connections, inverter installation, and initial system testing",
  "issuesEncountered": "None - all work proceeded as planned",
  "totalWorkHours": 8.5,
  "personnelOnSite": 4,
  "safetyScore": 10,
  "qualityScore": 9,
  "dailyProgressContribution": 12.5,
  "additionalNotes": "Team worked efficiently and maintained high safety standards throughout the day",
  "workProgressItems": [
    {
      "activity": "Solar panel installation",
      "description": "Installed 24 high-efficiency solar panels on south-facing roof",
      "hoursWorked": 6.0,
      "percentageComplete": 100,
      "workersAssigned": 3,
      "notes": "Installation completed without issues"
    },
    {
      "activity": "Mounting rail installation",
      "description": "Installed and secured mounting rails for panel attachment",
      "hoursWorked": 2.5,
      "percentageComplete": 100,
      "workersAssigned": 4,
      "notes": "Rails properly aligned and secured"
    }
  ],
  "personnelLogs": [
    {
      "userId": "user1-id",
      "hoursWorked": 8.5,
      "role": "Lead Installer",
      "specialAssignments": "Panel alignment verification",
      "notes": "Excellent performance, no safety issues"
    }
  ],
  "materialUsages": [
    {
      "materialId": "mat1-id",
      "quantity": 24,
      "unit": "panels",
      "unitCost": 450.00,
      "notes": "High-efficiency monocrystalline panels"
    },
    {
      "materialId": "mat2-id",
      "quantity": 12,
      "unit": "rails",
      "unitCost": 75.00,
      "notes": "Aluminum mounting rails"
    }
  ],
  "equipmentLogs": [
    {
      "equipmentId": "eq1-id",
      "hoursUsed": 4.0,
      "condition": "Excellent",
      "issues": "None",
      "maintenanceNotes": "Regular maintenance up to date"
    }
  ]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Enhanced daily report created successfully",
  "data": {
    "id": "new-report-id",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "approvalStatus": "Draft",
    "reportDate": "2025-06-29",
    "totalWorkHours": 8.5,
    "dailyProgressContribution": 12.5,
    "safetyScore": 10,
    "qualityScore": 9,
    "createdAt": "2025-06-29T18:00:00Z"
  },
  "errors": []
}
```

## 📊 Analytics & Reporting

### Get Daily Report Analytics

**GET** `/api/v1/daily-reports/projects/{projectId}/analytics`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get comprehensive analytics for daily reports within a project.

**Query Parameters**:
- `startDate` (DateTime): Analysis start date (default: 30 days ago)
- `endDate` (DateTime): Analysis end date (default: today)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Analytics retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "analysisPeriodStart": "2025-05-30",
    "analysisPeriodEnd": "2025-06-29",
    "totalReports": 30,
    "totalHoursLogged": 255.5,
    "averageHoursPerDay": 8.5,
    "averageSafetyScore": 9.2,
    "averageQualityScore": 8.8,
    "totalProgressContribution": 85.0,
    "averageProgressPerDay": 2.8,
    "daysAheadBehindSchedule": 2,
    "totalCriticalIssues": 3,
    "weatherDelayDays": 4,
    "topIssueCategories": ["Weather", "Material Delivery", "Permits"],
    "averageTeamSize": 4,
    "productivityIndex": 1.15,
    "topPerformers": [
      {
        "userId": "user1-id",
        "name": "John Technician",
        "reportsSubmitted": 15,
        "averageHoursPerDay": 8.2,
        "averageSafetyScore": 9.8,
        "averageQualityScore": 9.1,
        "productivityScore": 1.25
      }
    ],
    "weatherConditionDays": {
      "Sunny": 20,
      "PartlyCloudy": 6,
      "Rainy": 4
    },
    "weatherImpactScore": 0.13,
    "progressTrend": [
      {
        "date": "2025-06-25",
        "value": 2.5
      },
      {
        "date": "2025-06-26",
        "value": 3.2
      }
    ],
    "safetyTrend": [
      {
        "date": "2025-06-25",
        "value": 9.0
      },
      {
        "date": "2025-06-26",
        "value": 9.5
      }
    ]
  },
  "errors": []
}
```

### Get Weekly Progress Report

**GET** `/api/v1/daily-reports/projects/{projectId}/weekly-report`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Generate a comprehensive weekly progress report.

**Query Parameters**:
- `weekStartDate` (DateTime): Week start date (default: current week)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Weekly progress report generated successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "weekStartDate": "2025-06-23",
    "weekEndDate": "2025-06-29",
    "reportsSubmitted": 7,
    "totalHours": 59.5,
    "progressMade": 18.5,
    "teamMemberCount": 4,
    "dailyProgress": [
      {
        "date": "2025-06-23",
        "hours": 8.5,
        "progress": 2.5,
        "teamSize": 4,
        "weatherCondition": "Sunny",
        "hasIssues": false,
        "safetyScore": 10.0,
        "qualityScore": 9.0
      }
    ],
    "keyAccomplishments": [
      "Completed installation of 48 solar panels",
      "Finished electrical connections for Phase 1",
      "Passed city safety inspection"
    ],
    "criticalIssues": [
      "Weather delay on Thursday due to thunderstorms"
    ],
    "upcomingMilestones": [
      "Inverter installation scheduled for next week",
      "Final inspection planned for end of month"
    ],
    "averageSafetyScore": 9.4,
    "averageQualityScore": 8.9,
    "isOnSchedule": true,
    "productivityIndex": 1.12
  },
  "errors": []
}
```

## 🔄 Bulk Operations

### Bulk Approve Daily Reports

**POST** `/api/v1/daily-reports/bulk-approve`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Approve multiple daily reports in a single operation.

**Request Body**:
```json
{
  "reportIds": [
    "report1-id",
    "report2-id",
    "report3-id"
  ],
  "comments": "Bulk approval for completed installation reports"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Bulk approval completed",
  "data": {
    "totalRequested": 3,
    "successCount": 3,
    "failureCount": 0,
    "results": [
      {
        "itemId": "report1-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "itemId": "report2-id",
        "success": true,
        "details": "Approved successfully"
      },
      {
        "itemId": "report3-id",
        "success": true,
        "details": "Approved successfully"
      }
    ],
    "summary": "All 3 reports approved successfully"
  },
  "errors": []
}
```

### Bulk Reject Daily Reports

**POST** `/api/v1/daily-reports/bulk-reject`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Reject multiple daily reports with reason.

**Request Body**:
```json
{
  "reportIds": [
    "report1-id",
    "report2-id"
  ],
  "rejectionReason": "Insufficient detail in work summary. Please provide more specific information about tasks completed and materials used."
}
```

## 🎯 AI-Powered Insights

### Get Daily Report Insights

**GET** `/api/v1/daily-reports/projects/{projectId}/insights`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get AI-generated insights and recommendations for daily reports.

**Query Parameters**:
- `reportId` (Guid): Specific report ID for targeted insights (optional)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Insights generated successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "generatedAt": "2025-06-29T18:00:00Z",
    "performanceInsights": [
      "Team productivity is 15% above industry average",
      "Safety scores consistently high with 9.2 average",
      "Weather-related delays are within expected range"
    ],
    "productivityRecommendations": [
      "Consider adding one additional team member to maintain current pace",
      "Schedule electrical work during optimal morning hours",
      "Implement pre-positioning of materials to reduce setup time"
    ],
    "riskLevel": "Low",
    "identifiedRisks": [
      "Potential weather delays during rainy season",
      "Material delivery schedule may impact timeline"
    ],
    "riskMitigationSuggestions": [
      "Maintain 2-day buffer for weather contingencies",
      "Establish backup material suppliers",
      "Consider covered work areas for sensitive operations"
    ],
    "isOnTrack": true,
    "progressVelocity": 2.8,
    "estimatedDaysToCompletion": 12,
    "safetyRecommendations": [
      "Continue current safety protocols",
      "Consider additional fall protection for roof work",
      "Regular safety briefings are effective"
    ],
    "qualityImprovements": [
      "Panel alignment procedures are excellent",
      "Consider implementing quality checkpoints",
      "Documentation quality could be enhanced"
    ],
    "trends": [
      {
        "category": "Safety",
        "trend": "Improving",
        "description": "Safety scores trending upward over past 2 weeks",
        "changePercent": 8.5,
        "recommendation": "Maintain current safety protocols"
      },
      {
        "category": "Productivity",
        "trend": "Stable",
        "description": "Consistent productivity levels maintained",
        "changePercent": 2.1,
        "recommendation": "Look for optimization opportunities"
      }
    ]
  },
  "errors": []
}
```

## ✅ Data Validation

### Validate Daily Report

**POST** `/api/v1/daily-reports/validate`

Validate daily report data before submission with AI-powered suggestions.

**Request Body**: Same as enhanced create daily report request

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Validation completed",
  "data": {
    "isValid": true,
    "errors": [],
    "warnings": [
      "Weather impact not specified - consider adding impact assessment"
    ],
    "suggestions": [
      "Add more detail to work accomplished section",
      "Consider including material cost information",
      "Safety score explanation would be helpful"
    ],
    "ruleResults": [
      {
        "ruleName": "Required Fields",
        "severity": "Error",
        "passed": true,
        "message": "All required fields are present"
      },
      {
        "ruleName": "Safety Score Minimum",
        "severity": "Warning", 
        "passed": true,
        "message": "Safety score is within acceptable range",
        "suggestion": "Consider documenting why safety score is perfect"
      }
    ],
    "autoCorrections": [
      {
        "field": "weatherCondition",
        "currentValue": "sunny",
        "suggestedValue": "Sunny",
        "reason": "Standardize weather condition format",
        "confidence": 0.95
      }
    ]
  },
  "errors": []
}
```

## 📝 Templates & Configuration

### Get Daily Report Templates

**GET** `/api/v1/daily-reports/projects/{projectId}/templates`

Get project-specific daily report templates for consistent data collection.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Templates retrieved successfully",
  "data": [
    {
      "id": "template1-id",
      "name": "Standard Solar Installation",
      "description": "Standard template for solar panel installation projects",
      "projectId": "456e7890-e89b-12d3-a456-426614174001",
      "projectType": "Solar Installation",
      "fields": [
        {
          "name": "workSummary",
          "label": "Work Summary",
          "type": "Text",
          "isRequired": true,
          "helpText": "Provide a comprehensive summary of work completed",
          "displayOrder": 1
        },
        {
          "name": "safetyScore",
          "label": "Safety Score",
          "type": "Number",
          "isRequired": true,
          "constraints": {
            "min": 1,
            "max": 10
          },
          "defaultValue": "10",
          "helpText": "Rate safety performance from 1-10",
          "displayOrder": 2
        }
      ],
      "requiredFields": ["workSummary", "safetyScore", "totalWorkHours"],
      "defaultValues": {
        "safetyScore": 10,
        "qualityScore": 10
      },
      "validationRules": [
        {
          "name": "Safety Score Minimum",
          "type": "Range",
          "field": "safetyScore",
          "parameters": {
            "min": 7
          },
          "errorMessage": "Safety score must be at least 7 for project approval",
          "severity": "Warning"
        }
      ],
      "isDefault": true,
      "isActive": true,
      "createdAt": "2025-06-01T10:00:00Z"
    }
  ],
  "errors": []
}
```

## 🔄 Workflow Management

### Get Pending Approvals

**GET** `/api/v1/daily-reports/pending-approval`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get daily reports pending approval.

**Query Parameters**:
- `projectId` (Guid): Filter by project (optional)
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Pending approvals retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "report1-id",
        "projectId": "project1-id",
        "projectName": "Solar Installation Alpha",
        "reportDate": "2025-06-28",
        "reporterName": "John Technician",
        "submittedAt": "2025-06-28T17:00:00Z",
        "status": "Submitted",
        "priority": "Normal",
        "hasIssues": false,
        "totalWorkHours": 8.5,
        "safetyScore": 10,
        "qualityScore": 9
      }
    ],
    "pagination": {
      "totalCount": 5,
      "pageNumber": 1,
      "pageSize": 20,
      "totalPages": 1,
      "hasPreviousPage": false,
      "hasNextPage": false
    }
  },
  "errors": []
}
```

### Get Approval History

**GET** `/api/v1/daily-reports/{reportId}/approval-history`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Get complete approval history for a daily report.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Approval history retrieved successfully",
  "data": [
    {
      "id": "history1-id",
      "action": "Submitted",
      "actorId": "user1-id",
      "actorName": "John Technician",
      "timestamp": "2025-06-28T17:00:00Z",
      "comments": "Initial submission"
    },
    {
      "id": "history2-id",
      "action": "Approved",
      "actorId": "manager1-id",
      "actorName": "Sarah Manager",
      "timestamp": "2025-06-28T18:30:00Z",
      "comments": "Excellent work quality and safety compliance"
    }
  ],
  "errors": []
}
```

## 📤 Enhanced Export

### Export Enhanced Daily Reports

**POST** `/api/v1/daily-reports/export-enhanced`

**🔒 Required Roles**: Admin, Manager, ProjectManager

Export daily reports with comprehensive data and analytics.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "startDate": "2025-06-01",
  "endDate": "2025-06-29",
  "format": "excel",
  "includeAttachments": true,
  "includeAnalytics": true,
  "fieldsToInclude": [
    "workSummary",
    "safetyScore",
    "qualityScore",
    "totalWorkHours",
    "weatherCondition",
    "progressContribution"
  ]
}
```

**Success Response (200)**:
- File download with comprehensive daily report data

## 🎯 Summary: Daily Reports as Project Intelligence Hub

Daily Reports serve as the **primary intelligence gathering and performance monitoring system** for solar installation projects, providing:

### 📊 Real-Time Project Intelligence
- **Live Progress Tracking**: Immediate visibility into daily progress with automatic master plan integration
- **Safety & Quality Monitoring**: Continuous compliance tracking with automated alerts for below-threshold scores
- **Resource Utilization Analytics**: Real-time tracking of personnel, equipment, and material efficiency
- **Weather Impact Analysis**: Comprehensive weather data collection with impact assessment on project timelines

### 🤖 AI-Powered Optimization
- **Predictive Analytics**: AI-driven insights for project completion forecasting and risk identification
- **Performance Benchmarking**: Automated comparison against industry standards and historical project data
- **Optimization Recommendations**: Machine learning-powered suggestions for productivity and safety improvements
- **Anomaly Detection**: Automatic identification of unusual patterns requiring management attention

### 🔄 Integrated Workflow Management
- **Approval Automation**: Streamlined approval workflows with role-based permissions and escalation rules
- **Template Standardization**: Project-specific templates ensuring consistent, high-quality data collection
- **Bulk Operations**: Efficient mass operations for large teams and multi-project management
- **Audit Trail**: Complete tracking of all report modifications, approvals, and workflow actions

### 📈 Strategic Business Value
- **Compliance Documentation**: Comprehensive records for regulatory compliance and client reporting
- **Performance Analytics**: Detailed insights into team productivity, safety performance, and project health
- **Cost Management**: Material usage tracking and cost analysis for budget optimization
- **Client Communication**: Professional progress reports with photos and detailed accomplishments

### 🔗 Ecosystem Integration
- **Master Plan Synchronization**: Automatic progress updates to project schedules and milestone tracking
- **Task Management**: Direct integration with task completion and assignment systems
- **Calendar Coordination**: Schedule updates and milestone notifications based on daily progress
- **Notification Hub**: Automated stakeholder communications for issues, achievements, and approvals

The Daily Reports system transforms routine documentation into a powerful project management and business intelligence platform, providing the data foundation for successful solar installation project delivery.

## ❌ Enhanced Daily Report Error Codes

### Core Operations
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR001** | Report not found | Verify report ID exists and user has access |
| **DR002** | Insufficient permissions | Check user role or report ownership |
| **DR003** | Invalid report data | Check request body for required fields and validation rules |
| **DR004** | Cannot update after 24h | Only Admins/Managers can update older reports |
| **DR005** | Project not found or access denied | Verify project ID exists and user has project access |
| **DR006** | File upload failed | Check file format, size, and upload permissions |

### Enhanced Validation & Business Logic
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR007** | Duplicate report for date | Only one report per user per day allowed |
| **DR008** | Invalid weather data | Check weather condition values and temperature ranges |
| **DR009** | Safety score below threshold | Safety score must be >= 7 for project approval |
| **DR010** | Invalid progress contribution | Progress contribution must be 0-100% |
| **DR011** | Work hours exceed daily limit | Total work hours cannot exceed 24 hours |
| **DR012** | Personnel count mismatch | Personnel on site must match personnel logs |
| **DR013** | Material usage validation failed | Check material IDs and quantity values |
| **DR014** | Equipment log validation failed | Verify equipment IDs and usage hours |

### Project & Access Control
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR015** | Project inactive or suspended | Report creation disabled for inactive projects |
| **DR016** | User not assigned to project | User must be assigned to project to create reports |
| **DR017** | Project template not found | Verify project has valid daily report template |
| **DR018** | Template validation failed | Check report data against project template requirements |

### Workflow & Approval
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR019** | Invalid approval status transition | Check valid status transitions (Draft→Submitted→Approved/Rejected) |
| **DR020** | Report already approved | Cannot modify approved reports |
| **DR021** | Approval permission denied | User lacks approval authority for this project |
| **DR022** | Bulk operation partially failed | Check individual operation results for details |
| **DR023** | Approval history not found | Report may not have been submitted for approval |

### Analytics & Reporting
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR024** | Analytics data unavailable | Insufficient data for requested analysis period |
| **DR025** | Export format not supported | Use supported formats: csv, excel, pdf, json |
| **DR026** | Export size limit exceeded | Reduce date range or field selection |
| **DR027** | Insights generation failed | AI service temporarily unavailable, try again later |

### Data Integrity & Validation
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR028** | Date validation failed | Report date cannot be future date or too far in past |
| **DR029** | Attachment size limit exceeded | Reduce file size or use compression |
| **DR030** | Invalid attachment type | Only images and documents allowed |
| **DR031** | Data consistency error | Report data conflicts with existing project data |
| **DR032** | Template field validation failed | Check required fields and field constraints |

### System & Performance
| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR033** | Rate limit exceeded | Too many requests, wait before retrying |
| **DR034** | Cache refresh required | Clear cache or wait for automatic refresh |
| **DR035** | Service temporarily unavailable | System maintenance in progress, try again later |
| **DR036** | Database connection failed | Contact system administrator |

---

## 🚀 Quick Start Guide

### Step 1: Login and Get JWT Token
```bash
# Login with admin credentials
curl -X POST "http://localhost:5001/api/v1/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin@example.com",
    "password": "Admin123!"
  }'

# Response includes token
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "username": "admin",
      "email": "admin@example.com",
      "roleName": "Admin"
    }
  }
}
```

### Step 2: Use Token for API Calls
```bash
# Set your token (replace with actual token from login)
TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Get all daily reports
curl -X GET "http://localhost:5001/api/v1/daily-reports" \
  -H "Authorization: Bearer $TOKEN"

# Create a new daily report
curl -X POST "http://localhost:5001/api/v1/daily-reports" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "projectId": "your-project-id",
    "reportDate": "2025-07-05",
    "hoursWorked": 8.0,
    "personnelOnSite": 4,
    "weatherConditions": "Sunny, 78°F",
    "summary": "Completed installation of 12 solar panels on east wing",
    "workAccomplished": "Panel mounting and initial electrical connections",
    "workPlannedNextDay": "Complete wiring and inverter installation",
    "issues": "None",
    "safetyScore": 10,
    "qualityScore": 9,
    "dailyProgressContribution": 8.5
  }'
```

### Step 3: Test Different Endpoints
```bash
# Get project-specific reports (all users)
curl -X GET "http://localhost:5001/api/v1/daily-reports/projects/{projectId}" \
  -H "Authorization: Bearer $TOKEN"

# Get analytics (Admin/Manager/ProjectManager only)
curl -X GET "http://localhost:5001/api/v1/daily-reports/projects/{projectId}/analytics" \
  -H "Authorization: Bearer $TOKEN"

# Submit report for approval
curl -X POST "http://localhost:5001/api/v1/daily-reports/{reportId}/submit" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"comments": "Ready for review"}'
```

### Step 4: Error Handling & Troubleshooting
```bash
# Example unauthorized response (401)
{
  "success": false,
  "message": "Unauthorized",
  "errors": ["Authentication required"]
}

# Example forbidden response (403)
{
  "success": false,
  "message": "Insufficient permissions",
  "errors": ["User lacks required permissions for this resource"]
}

# Example validation error (400)
{
  "success": false,
  "message": "Invalid input data",
  "errors": [
    "SafetyScore must be between 1 and 10",
    "ProjectId is required"
  ]
}
```

## 🔧 Troubleshooting Common Issues

### Authentication Problems
- **Issue**: Getting 401 Unauthorized errors
- **Solution**: Ensure your JWT token is valid and not expired
- **Check**: Login again to get a fresh token

### Permission Denied
- **Issue**: Getting 403 Forbidden errors
- **Solution**: Check that your user role has permission for the requested operation
- **Admin/Manager**: Full access to all endpoints
- **ProjectManager**: Limited to assigned projects
- **User**: Can only manage own reports

### Validation Errors
- **Issue**: Getting 400 Bad Request with validation errors
- **Solution**: Check required fields and data formats
- **Common**: Safety/Quality scores (1-10), valid project IDs, proper date formats

### Performance Tips
- Use pagination for large datasets (`pageSize`, `pageNumber`)
- Apply filters to reduce response size (`projectId`, `startDate`, `endDate`)
- Cache responses when possible (automatic with cache headers)

### Rate Limiting
- API implements rate limiting to prevent abuse
- If you hit rate limits, wait and retry
- Contact administrator if you need higher limits

## 📚 Additional Resources

### API Status & Development
- **Current Version**: v1.0
- **Environment**: Development (localhost:5001)
- **Database**: In-Memory (resets on restart)
- **Authentication**: JWT Bearer Token required
- **Hot Reload**: Supported via `dotnet watch run`

### Related API Documentation
- [Projects API](./01_PROJECTS.md) - Manage solar installation projects
- [Authentication](./00_AUTHENTICATION.md) - JWT authentication and user management
- [Tasks API](./02_TASKS.md) - Task management and tracking

### Development Notes
- All endpoints are currently implemented and tested
- Role-based authorization is enforced on all endpoints
- CSV import functionality is available for bulk project creation
- Live reload is enabled for development convenience

### Best Practices
1. **Always authenticate** before making API calls
2. **Check permissions** for your user role before attempting operations
3. **Use filters** to reduce response sizes and improve performance
4. **Handle errors gracefully** with proper error checking
5. **Follow rate limits** to avoid service disruption
6. **Cache responses** when appropriate to reduce API calls
7. **Test with different user roles** to understand permission boundaries
8. **Use development environment** for testing before production deployment

---

**🔔 Development Note**: This API is currently running in development mode with in-memory database. Data will be reset when the application restarts. For production use, configure a persistent database connection in `appsettings.json`.

*Last Updated: July 5, 2025*

## 🎯 Current API Status & Features

### ✅ **Fully Implemented & Tested**
- **CRUD Operations**: Create, Read, Update, Delete daily reports
- **JWT Authentication**: Role-based access control with Bearer tokens
- **Project Integration**: Reports linked to solar installation projects
- **Workflow Management**: Submit, approve, reject reports
- **File Attachments**: Photo and document uploads
- **Analytics**: Project-specific insights and reporting
- **Bulk Operations**: Mass approve/reject capabilities
- **Export Functions**: CSV, Excel, PDF export support
- **Hot Reload**: Live development with `dotnet watch run`

### 🔧 **Development Environment**
- **Local Server**: `http://localhost:5001`
- **Database**: In-Memory (resets on application restart)
- **Authentication**: JWT tokens (expire after configured time)
- **Role System**: Admin, Manager, ProjectManager, User roles
- **Import Support**: CSV import for bulk project creation

### 📊 **Available Endpoints Summary**
- **Core CRUD**: 6 endpoints (GET, POST, PUT, DELETE operations)
- **Workflow**: 3 endpoints (submit, approve, reject)
- **Analytics**: 4 endpoints (insights, weekly reports, analytics)
- **Bulk Operations**: 2 endpoints (bulk approve/reject)
- **File Management**: 2 endpoints (attachments, validation)
- **Export**: 2 endpoints (basic and enhanced export)

### 🚀 **Recent Updates (July 2025)**
- Enhanced JWT authentication with role-based authorization
- Improved Quick Start documentation with troubleshooting
- Added login flexibility (username or email accepted)
- Updated examples with current date (July 2025)
- Added comprehensive error handling guide
- Documented all available endpoints and permissions
