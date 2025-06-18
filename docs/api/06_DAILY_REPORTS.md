# 📊 Daily Reports

**🔒 Authentication Required**  
**🎯 Role Required**: Admin, Manager (full access), Users (create own reports), All authenticated users (view)

Daily reports provide a way to track daily work progress, hours worked, and issues encountered on solar installation projects.

## ⚡ Daily Report Capabilities

### Admin & Manager
- ✅ Create reports for any project
- ✅ View all reports across projects
- ✅ Update any report details
- ✅ Generate summary reports
- ✅ Export report data
- ✅ Delete reports if needed

### Users (Technicians)
- ✅ Create their own daily reports
- ✅ Update their own reports (within 24h)
- ✅ Attach photos to reports
- ✅ Report issues and blockers
- ❌ Cannot modify others' reports
- ❌ Cannot delete reports

## 📋 Get All Daily Reports

**GET** `/api/v1/daily-reports`

Retrieve daily reports with filtering options.

**Query Parameters**:
- `projectId` (Guid): Filter by project
- `userId` (Guid): Filter by reporting user
- `startDate` (DateTime): Filter from date
- `endDate` (DateTime): Filter to date
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily reports retrieved successfully",
  "data": {
    "reports": [
      {
        "id": "123e4567-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "789e0123-e89b-12d3-a456-426614174002",
        "userName": "John Technician",
        "reportDate": "2025-06-14",
        "hoursWorked": 8.5,
        "weatherConditions": "Sunny, 85°F",
        "summary": "Completed installation of 24 solar panels on south-facing roof section",
        "issues": "None",
        "tasksCompleted": ["Install mounting rails", "Install panels in section A"],
        "createdAt": "2025-06-14T17:30:00Z",
        "hasAttachments": true
      },
      {
        "id": "234f6789-e89b-12d3-a456-426614174000",
        "projectId": "456e7890-e89b-12d3-a456-426614174001",
        "projectName": "Solar Installation Project Alpha",
        "userId": "890f1234-e89b-12d3-a456-426614174002",
        "userName": "Sarah Electrician",
        "reportDate": "2025-06-14",
        "hoursWorked": 7.0,
        "weatherConditions": "Sunny, 85°F",
        "summary": "Completed electrical wiring for inverter connections",
        "issues": "Permit inspector delayed until tomorrow",
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
    }
  },
  "errors": []
}
```

## 🔍 Get Daily Report by ID

**GET** `/api/v1/daily-reports/{id}`

Retrieve details of a specific daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "userId": "789e0123-e89b-12d3-a456-426614174002",
    "userName": "John Technician",
    "reportDate": "2025-06-14",
    "hoursWorked": 8.5,
    "weatherConditions": "Sunny, 85°F",
    "summary": "Completed installation of 24 solar panels on south-facing roof section",
    "issues": "None",
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

Create a new daily report for work completed.

**Request Body**:
```json
{
  "projectId": "456e7890-e89b-12d3-a456-426614174001",
  "reportDate": "2025-06-15",
  "hoursWorked": 9.0,
  "weatherConditions": "Partly cloudy, 82°F",
  "summary": "Completed electrical connections and initial testing of inverter system",
  "issues": "Waiting for city inspector clearance before final connection",
  "tasksCompleted": [
    {
      "taskId": "task3-id",
      "completionPercentage": 100
    },
    {
      "taskId": "task4-id",
      "completionPercentage": 75
    }
  ],
  "materialsUsed": [
    {
      "name": "Electrical cable",
      "quantity": 150,
      "unit": "feet"
    },
    {
      "name": "Conduit",
      "quantity": 40,
      "unit": "feet"
    }
  ]
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Daily report created successfully",
  "data": {
    "id": "345g7890-e89b-12d3-a456-426614174000",
    "projectName": "Solar Installation Project Alpha",
    "reportDate": "2025-06-15",
    "hoursWorked": 9.0,
    "createdAt": "2025-06-15T18:15:00Z"
  },
  "errors": []
}
```

## 🔄 Update Daily Report

**PUT** `/api/v1/daily-reports/{id}`

**🔒 Requires**: Admin, Manager, or report creator (within 24h)

Update an existing daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Request Body**:
```json
{
  "hoursWorked": 9.5,
  "weatherConditions": "Partly cloudy with afternoon rain, 80°F",
  "summary": "Completed electrical connections and initial testing of inverter system. Rain delayed final testing.",
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

**🔒 Required Roles**: Admin, Manager

Delete a daily report.

**Path Parameters**:
- `id` (Guid): Daily report ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Daily report deleted successfully",
  "data": null,
  "errors": []
}
```

## 📎 Add Attachment to Report

**POST** `/api/v1/daily-reports/{id}/attachments`

**🔒 Requires**: Admin, Manager, or report creator

Add a photo or document attachment to a daily report.

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

## ❌ Daily Report Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **DR001** | Report not found | Verify report ID exists |
| **DR002** | Insufficient permissions | Check user role or report ownership |
| **DR003** | Invalid report data | Check request body for required fields |
| **DR004** | Cannot update after 24h | Only Admins/Managers can update older reports |
| **DR005** | Project not found | Verify project ID exists |
| **DR006** | File upload failed | Check file format and size |

---
*Last Updated: June 15, 2025*
