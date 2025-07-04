# Project Management

Project management endpoints for the Solar Projects API.

**Authentication Required**: All endpoints require JWT authentication  
**Role Required**: Admin/Manager (full CRUD), User/Viewer (read-only)

## Admin & Manager Capabilities

- Create new projects
- Update all project fields
- Modify project assignments and team configurations  
- Update technical specifications
- Edit location coordinates and connection details
- Change project status and timelines
- Delete projects (Admin only)

## Update Permissions

Admin & Manager roles have full update access to all project data fields:

| Data Category | Fields | API Endpoints |
|---------------|--------|---------------|
| Basic Info | `projectName`, `address`, `clientInfo` | PUT/PATCH `/projects/{id}` |
| Timeline | `startDate`, `estimatedEndDate`, `actualEndDate` | PUT/PATCH `/projects/{id}` |
| Team | `projectManagerId`, `team` assignments | PUT/PATCH `/projects/{id}` |
| Technical | `totalCapacityKw`, `pvModuleCount`, `connectionType` | PUT/PATCH `/projects/{id}` |
| Equipment | Inverter details and specifications | PUT/PATCH `/projects/{id}` |
| Financial | `ftsValue`, `revenueValue`, `pqmValue` | PUT/PATCH `/projects/{id}` |
| Location | `locationCoordinates.latitude`, `longitude` | PUT/PATCH `/projects/{id}` |
| Status | Project status and workflow management | PUT/PATCH `/projects/{id}` |

**Admin-Only Operations:**
- Project deletion (`DELETE /projects/{id}`)

**User & Viewer Access:**
- Read-only access to all project data
- Can submit reports related to their work

## Get All Projects

**GET** `/api/v1/projects`

Retrieve a paginated list of projects with filtering, sorting, and field selection.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10, max: 100)
- `status` (string): Filter by status 
- `search` (string): Search in project name or description
- `sortBy` (string): Sort field
- `sortOrder` (string): Sort direction ("asc" or "desc")
- `fields` (string): Comma-separated list of fields to include
- `managerId` (Guid): Filter by project manager ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "projectName": "สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "address": "ต.แม่กลอง อ.เมืองสมุทรสงคราม จ.สมุทรสงคราม 75000",
        "clientInfo": "101 สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "status": "Planning",
        "startDate": "2024-01-01T00:00:00Z",
        "estimatedEndDate": null,
        "actualEndDate": null,
        "updatedAt": "2025-06-15T10:00:00Z",
        "projectManager": {
          "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
          "username": "project.manager",
          "email": "pm@company.com",
          "fullName": "Project Manager",
          "roleName": "Manager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8,
        "team": "Solar Team Alpha",
        "connectionType": "LV",
        "connectionNotes": "ระบบจำหน่ายแรงต่ำ",
        "totalCapacityKw": 996.0,
        "pvModuleCount": 1800,
        "equipmentDetails": {
          "inverter125kw": 8,
          "inverter80kw": 0,
          "inverter60kw": 0,
          "inverter40kw": 0
        },
        "ftsValue": 18700000,
        "revenueValue": 22440000,
        "pqmValue": 3740000,
        "locationCoordinates": {
          "latitude": 13.4098,
          "longitude": 99.9969
        },
        "createdAt": "2025-06-01T14:30:00Z"
      }
      // More projects...
    ],
    "pagination": {
      "totalCount": 97,
      "pageNumber": 1,
      "pageSize": 10,
      "totalPages": 10,
      "hasPreviousPage": false,
      "hasNextPage": true
    }
  },
  "errors": []
}
```

## Get Project by ID

**GET** `/api/v1/projects/{id}`

Retrieve detailed information about a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "address": "123 Solar Street, Sunnydale, CA 90210",
    "clientInfo": "Residential solar panel installation for 50-home subdivision with 3MW total capacity",
    "status": "Active",
    "startDate": "2025-06-01T00:00:00Z",
    "estimatedEndDate": "2025-08-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-14T16:30:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "john.manager",
      "email": "john@company.com",
      "fullName": "John Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 12,
    "completedTaskCount": 8,
    "team": "Solar Installation Team Beta",
    "connectionType": "MV",
    "connectionNotes": "22kV medium voltage connection with smart metering",
    "totalCapacityKw": 3000.0,
    "pvModuleCount": 5000,
    "equipmentDetails": {
      "inverter125kw": 15,
      "inverter80kw": 5,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 45000000,
    "revenueValue": 54000000,
    "pqmValue": 9000000,
    "locationCoordinates": {
      "latitude": 34.0522,
      "longitude": -118.2437
    },
    "createdAt": "2025-05-15T10:00:00Z"
  },
  "errors": []
}
```

## Get My Projects

**GET** `/api/v1/projects/me`

Get projects associated with the current authenticated user.

**Query Parameters**:
- `pageNumber` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 10)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "User projects retrieved successfully",
  "data": {
    "items": [
      {
        "projectId": "6e729d9a-b2fc-4d54-8e79-81d77bd248d3",
        "projectName": "สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "address": "ต.แม่กลอง อ.เมืองสมุทรสงคราม จ.สมุทรสงคราม 75000",
        "clientInfo": "101 สำนักงานประปาสมุทรสงคราม กปภ.สาขาสมุทรสงคราม",
        "status": "Planning",
        "startDate": "2024-01-01T00:00:00Z",
        "estimatedEndDate": null,
        "actualEndDate": null,
        "updatedAt": "2025-06-15T10:00:00Z",
        "projectManager": {
          "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
          "username": "project.manager",
          "email": "pm@company.com",
          "fullName": "Project Manager",
          "roleName": "Manager",
          "isActive": true
        },
        "taskCount": 15,
        "completedTaskCount": 8,
        "team": "Solar Team Alpha",
        "connectionType": "LV",
        "connectionNotes": "ระบบจำหน่ายแรงต่ำ",
        "totalCapacityKw": 996.0,
        "pvModuleCount": 1800,
        "equipmentDetails": {
          "inverter125kw": 8,
          "inverter80kw": 0,
          "inverter60kw": 0,
          "inverter40kw": 0
        },
        "ftsValue": 18700000,
        "revenueValue": 22440000,
        "pqmValue": 3740000,
        "locationCoordinates": {
          "latitude": 13.4098,
          "longitude": 99.9969
        },
        "createdAt": "2025-06-01T14:30:00Z"
      }
      // More projects...
    ],
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNextPage": false,
    "hasPreviousPage": false
  },
  "errors": []
}
```

## Get Project Status

**GET** `/api/v1/projects/{id}/status`

Get real-time status information for a specific project.

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project status retrieved successfully",
  "data": {
    "projectId": "456e7890-e89b-12d3-a456-426614174001",
    "projectName": "Solar Installation Project Alpha",
    "status": "Active",
    "plannedStartDate": "2025-06-01T00:00:00Z",
    "plannedEndDate": "2025-08-15T00:00:00Z",
    "actualStartDate": "2025-06-01T00:00:00Z",
    "overallCompletionPercentage": 67.5,
    "isOnSchedule": true,
    "isOnBudget": true,
    "activeTasks": 4,
    "completedTasks": 8,
    "totalTasks": 12,
    "lastUpdated": "2025-06-14T16:30:00Z",
    "links": [
      {
        "href": "/api/v1/projects/456e7890-e89b-12d3-a456-426614174001",
        "rel": "project",
        "method": "GET"
      },
      {
        "href": "/api/v1/master-plans?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "master-plans",
        "method": "GET"
      },
      {
        "href": "/api/v1/tasks?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "tasks",
        "method": "GET"
      },
      {
        "href": "/api/v1/documents?projectId=456e7890-e89b-12d3-a456-426614174001",
        "rel": "documents",
        "method": "GET"
      }
    ]
  },
  "errors": []
}
```

## Create New Project

**POST** `/api/v1/projects`

**🔒 Required Roles**: Admin, Manager

Create a new solar installation project with detailed information.

**Request Body**:
```json
{
  "projectName": "โรงพยาบาลปากน้ำ",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "startDate": "2025-07-15T00:00:00Z",
  "estimatedEndDate": "2025-10-30T00:00:00Z",
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "team": "Solar Installation Team Gamma",
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
  "totalCapacityKw": 850.5,
  "pvModuleCount": 1546,
  "equipmentDetails": {
    "inverter125kw": 7,
    "inverter80kw": 0,
    "inverter60kw": 0,
    "inverter40kw": 0
  },
  "ftsValue": 15800000,
  "revenueValue": 18960000,
  "pqmValue": 3160000,
  "locationCoordinates": {
    "latitude": 13.5619,
    "longitude": 100.2743
  }
}
```

**Success Response (201)**:
```json
{
  "success": true,
  "message": "Project created successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Planning",
    "startDate": "2025-07-15T00:00:00Z",
    "estimatedEndDate": "2025-10-30T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": null,
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 0,
    "completedTaskCount": 0,
    "team": "Solar Installation Team Gamma",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 850.5,
    "pvModuleCount": 1546,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 15800000,
    "revenueValue": 18960000,
    "pqmValue": 3160000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## Update Project

**PUT** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update all fields of an existing project.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body**:
```json
{
  "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "status": "Active",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-10-15T00:00:00Z",
  "actualEndDate": null,
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
  "team": "Solar Installation Team Delta",
  "connectionType": "MV",
  "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
  "totalCapacityKw": 875.0,
  "pvModuleCount": 1600,
  "equipmentDetails": {
    "inverter125kw": 7,
    "inverter80kw": 0,
    "inverter60kw": 0,
    "inverter40kw": 0
  },
  "ftsValue": 16200000,
  "revenueValue": 19440000,
  "pqmValue": 3240000,
  "locationCoordinates": {
    "latitude": 13.5619,
    "longitude": 100.2743
  }
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project updated successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Active",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-10-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-15T15:45:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 5,
    "completedTaskCount": 2,
    "team": "Solar Installation Team Delta",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 875.0,
    "pvModuleCount": 1600,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 0
    },
    "ftsValue": 16200000,
    "revenueValue": 19440000,
    "pqmValue": 3240000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## Partially Update Project

**PATCH** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin, Manager

Update specific fields of an existing project without affecting other fields.

**Path Parameters**:
- `id` (Guid): Project ID

**Request Body** (All fields are optional):
```json
{
  "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
  "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
  "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
  "status": "InProgress",
  "startDate": "2025-07-01T00:00:00Z",
  "estimatedEndDate": "2025-10-15T00:00:00Z",
  "actualEndDate": null,
  "projectManagerId": "c73a80de-c8b2-4a8c-a881-17452dcd1118"
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project updated successfully",
  "data": {
    "projectId": "8f729d9a-b2fc-4d54-8e79-81d77bd248d5",
    "projectName": "โรงพยาบาลปากน้ำ (ปรับปรุง)",
    "address": "ต.บางหญ้าแพรก อ.เมืองสมุทรสาคร จ.สมุทรสาคร 74000",
    "clientInfo": "โรงพยาบาลประจำจังหวัดสมุทรสาคร",
    "status": "Active",
    "startDate": "2025-07-01T00:00:00Z",
    "estimatedEndDate": "2025-10-15T00:00:00Z",
    "actualEndDate": null,
    "updatedAt": "2025-06-15T16:00:00Z",
    "projectManager": {
      "userId": "c73a80de-c8b2-4a8c-a881-17452dcd1118",
      "username": "project.manager",
      "email": "pm@company.com",
      "fullName": "Project Manager",
      "roleName": "Manager",
      "isActive": true
    },
    "taskCount": 5,
    "completedTaskCount": 2,
    "team": "Solar Installation Team Delta",
    "connectionType": "MV",
    "connectionNotes": "ระบบจำหน่ายแรงสูง 22 kV",
    "totalCapacityKw": 875.0,
    "pvModuleCount": 1600,
    "equipmentDetails": {
      "inverter125kw": 7,
      "inverter80kw": 0,
      "inverter60kw": 0,
      "inverter40kw": 1
    },
    "ftsValue": 16200000,
    "revenueValue": 19440000,
    "pqmValue": 3240000,
    "locationCoordinates": {
      "latitude": 13.5619,
      "longitude": 100.2743
    },
    "createdAt": "2025-06-15T14:30:00Z"
  },
  "errors": []
}
```

## Delete Project

**DELETE** `/api/v1/projects/{id}`

**🔒 Required Roles**: Admin only

Delete a project and all associated data (tasks, reports, etc.).

**Path Parameters**:
- `id` (Guid): Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project deleted successfully",
  "data": null,
  "errors": []
}
```

**Error Response (403)**:
```json
{
  "success": false,
  "message": "Insufficient permissions",
  "data": null,
  "errors": ["Only Admin users can delete projects"]
}
```

## Project Statuses

| Status | Description | Allowed Transitions |
|--------|-------------|---------------------|
| **Planning** | Initial project setup | InProgress, Cancelled |
| **InProgress** | Current work in progress | OnHold, Completed |
| **OnHold** | Temporarily suspended | InProgress, Cancelled |
| **Completed** | All work finished | (Final state) |
| **Cancelled** | Project terminated | (Final state) |

## Project Analytics

**GET** `/api/v1/projects/analytics`

Get comprehensive project analytics and performance metrics.

**Query Parameters**:
- `timeframe` (string): Analytics timeframe ("30d", "90d", "1y", "all")
- `groupBy` (string): Group results by ("status", "manager", "month", "quarter")
- `includeFinancial` (bool): Include financial metrics (default: false)
- `includePerformance` (bool): Include performance metrics (default: true)

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project analytics retrieved successfully",
  "data": {
    "summary": {
      "totalProjects": 97,
      "activeProjects": 68,
      "completedProjects": 23,
      "totalCapacity": 48560.5,
      "averageCompletionTime": 127,
      "onTimeDeliveryRate": 87.3
    },
    "statusBreakdown": {
      "Planning": 12,
      "InProgress": 68,
      "OnHold": 6,
      "Completed": 23,
      "Cancelled": 2
    },
    "performanceMetrics": {
      "averageProjectDuration": 127,
      "budgetVariance": -2.4,
      "qualityScore": 94.2,
      "customerSatisfaction": 91.8,
      "teamEfficiency": 88.5
    },
    "trends": {
      "projectsPerMonth": [
        { "month": "2024-01", "count": 8, "completed": 5 },
        { "month": "2024-02", "count": 12, "completed": 9 }
      ],
      "capacityTrends": [
        { "month": "2024-01", "totalKw": 2850.5 },
        { "month": "2024-02", "totalKw": 3420.8 }
      ]
    },
    "topPerformers": {
      "managers": [
        {
          "managerId": "mgr001",
          "fullName": "Sarah Johnson",
          "projectCount": 15,
          "completionRate": 93.3,
          "averageDuration": 115
        }
      ],
      "projects": [
        {
          "projectId": "proj001",
          "projectName": "Solar Farm Alpha",
          "completionRate": 98.5,
          "onTimeDelivery": true,
          "budgetVariance": -1.2
        }
      ]
    }
  }
}
```

## Project Performance Tracking

**GET** `/api/v1/projects/{id}/performance`

Track detailed performance metrics for a specific project.

**Path Parameters**:
- `id` (guid) - Project ID

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project performance retrieved successfully",
  "data": {
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "projectName": "Building A Solar Installation",
    "performanceScore": 92.4,
    "kpis": {
      "timelineAdherence": 94.2,
      "budgetAdherence": 97.8,
      "qualityScore": 96.5,
      "safetyScore": 100.0,
      "clientSatisfaction": 91.2
    },
    "milestones": [
      {
        "milestoneId": "ms001",
        "title": "Design Approval",
        "targetDate": "2024-06-15T00:00:00Z",
        "actualDate": "2024-06-14T00:00:00Z",
        "status": "Completed",
        "varianceDays": -1
      }
    ],
    "resourceUtilization": {
      "teamUtilization": 87.3,
      "equipmentUtilization": 82.1,
      "materialEfficiency": 94.7
    },
    "riskAssessment": {
      "overallRiskLevel": "Low",
      "activeRisks": 2,
      "mitigatedRisks": 8,
      "riskTrend": "Decreasing"
    },
    "progressHistory": [
      {
        "date": "2024-06-20",
        "completionPercentage": 75.5,
        "tasksCompleted": 45,
        "hoursWorked": 8.5,
        "issues": 0
      }
    ]
  }
}
```

## Project Status Workflow

**PATCH** `/api/v1/projects/{id}/status`

Update project status with workflow validation.

**Path Parameters**:
- `id` (guid) - Project ID

**Request Body**:
```json
{
  "status": "InProgress",
  "reason": "All permits approved and team assigned",
  "effectiveDate": "2024-06-21T08:00:00Z",
  "notifyStakeholders": true
}
```

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project status updated successfully",
  "data": {
    "projectId": "8f83b2a1-c4e5-4d67-9abc-123456789def",
    "previousStatus": "Planning",
    "newStatus": "InProgress",
    "effectiveDate": "2024-06-21T08:00:00Z",
    "updatedBy": {
      "userId": "user123",
      "fullName": "John Manager"
    },
    "notifications": {
      "sent": 12,
      "failed": 0,
      "recipients": ["team", "client", "stakeholders"]
    }
  }
}
```

## Project Templates

**GET** `/api/v1/projects/templates`

Get available project templates for quick project creation.

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Project templates retrieved successfully",
  "data": {
    "templates": [
      {
        "templateId": "tmpl001",
        "name": "Residential Solar Installation",
        "description": "Standard template for residential solar projects",
        "category": "Residential",
        "estimatedDuration": 30,
        "defaultTasks": [
          {
            "title": "Site Survey",
            "estimatedHours": 4,
            "phase": "Planning"
          },
          {
            "title": "Permit Application",
            "estimatedHours": 8,
            "phase": "Planning"
          }
        ],
        "requiredEquipment": [
          "Solar Panels",
          "Inverter",
          "Mounting System"
        ],
        "usageCount": 45
      }
    ]
  }
}
```

**POST** `/api/v1/projects/from-template/{templateId}`

Create a new project from a template.

**Path Parameters**:
- `templateId` (guid) - Template ID

**Request Body**:
```json
{
  "projectName": "Smith Residence Solar",
  "address": "123 Oak Street, Anytown, ST 12345",
  "clientInfo": "John & Jane Smith",
  "totalCapacityKw": 8.5,
  "projectManagerId": "mgr001",
  "startDate": "2024-06-25T00:00:00Z",
  "customizations": {
    "skipTasks": ["permit_application"],
    "additionalTasks": [
      {
        "title": "HOA Approval",
        "estimatedHours": 2
      }
    ]
  }
}
```

## Advanced Project Search

**GET** `/api/v1/projects/search`

Advanced search with full-text search and filters.

**Query Parameters**:
- `q` (string): Search query
- `filters` (object): Advanced filter criteria
- `coordinates` (string): Location-based search (lat,lng,radius)
- `dateRange` (string): Date range filter
- `facets` (bool): Include facet information

**Success Response (200)**:
```json
{
  "success": true,
  "message": "Search results retrieved successfully",
  "data": {
    "query": "solar installation residential",
    "results": [
      {
        "projectId": "proj001",
        "projectName": "Smith Residence Solar",
        "relevanceScore": 95.2,
        "matchedFields": ["projectName", "description", "tags"],
        "highlights": [
          "Smith Residence <mark>Solar</mark>",
          "<mark>Residential</mark> <mark>installation</mark> project"
        ]
      }
    ],
    "facets": {
      "status": {
        "InProgress": 45,
        "Completed": 23,
        "Planning": 12
      },
      "capacity": {
        "0-10kW": 67,
        "10-50kW": 23,
        "50kW+": 7
      },
      "location": {
        "Urban": 78,
        "Suburban": 45,
        "Rural": 12
      }
    },
    "suggestions": ["solar panels", "residential installation", "rooftop mounting"],
    "totalResults": 97,
    "searchTime": 0.034
  }
}
```

## Project Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| **PRJ001** | Project not found | Verify project ID exists |
| **PRJ002** | Insufficient permissions | Verify user role requirements |
| **PRJ003** | Invalid project data | Check request body for required fields |
| **PRJ004** | Cannot delete active project | Change status before deletion |
| **PRJ005** | Project manager not found | Verify project manager ID |
| **PRJ006** | Invalid status transition | Follow allowed status transitions |
| **PRJ007** | Duplicate project name | Choose a unique project name |
| **PRJ008** | Missing required fields | Add all required fields to request |
| **PRJ009** | Template not found | Verify template ID exists |
| **PRJ010** | Invalid search query | Check search syntax and parameters |
| **PRJ011** | Location coordinates invalid | Provide valid latitude and longitude |
| **PRJ012** | Performance data unavailable | Project must have activity to show performance |

## Summary

### Key Features
- **Comprehensive CRUD Operations**: Full project lifecycle management
- **Advanced Search & Filtering**: Powerful search capabilities with facets
- **Performance Analytics**: Detailed metrics and KPI tracking
- **Template System**: Quick project creation from predefined templates
- **Status Workflow**: Structured status transitions with validation
- **Real-time Notifications**: Automatic stakeholder notifications
- **Geographic Integration**: Location-based project management

### Integration Points
- **Master Plans**: Automatic master plan creation for new projects
- **Task Management**: Linked task creation and tracking
- **Daily Reports**: Progress reporting tied to projects
- **File Management**: Document and image storage per project
- **Calendar Integration**: Project milestones and schedule management

### Best Practices
- Use templates for consistent project setup
- Maintain accurate project status throughout lifecycle
- Regular performance monitoring and reporting
- Proper role-based access control implementation
- Geographic data for location-based insights
- Regular analytics review for process improvement

---
*Last Updated: July 4, 2025*
